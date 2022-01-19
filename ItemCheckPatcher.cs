using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace WeaponDurability
{
    static class ILHelperExtensions
    {
        public static Instruction NextTakeAll(this Instruction i, ref List<int> visited)
        {
            if (i.OpCode == OpCodes.Ret)
            {
                return null;
            }

            visited.Add(i.Offset);

            Instruction res;
            var target = (i.Operand as ILLabel)?.Target;
            if (target != null && !visited.Contains(target.Offset))
            {
                res = target;
            } else
            {
                res = i.Next;
            }

            if (res.OpCode == OpCodes.Ret)
            {
                return null;
            }

            return res;
        }

        public static void DumpIL(this ILContext il, StreamWriter f, Dictionary<string, Instruction> namedLabels)
        {
            foreach (var i in il.Body.Instructions)
            {
                var sp = namedLabels.FirstOrDefault(sp => sp.Value == i);
                if (sp.Value != null)
                {
                    f.Write($"=== {sp.Key} ===\n");
                }
                var ilLabel = (i.Previous != null && i.Offset == 0) ? "IL_XXXX" : $"IL_{i.Offset:x04}";
                var opStr = i.Operand;
                if (i.Operand is ILLabel l)
                {
                    opStr = (l.Target.Previous != null && l.Target.Offset == 0) ? "IL_XXXX" : $"IL_{l.Target.Offset:x04}";
                }
                f.Write($"{ilLabel}: {i.OpCode} {opStr}\n");
            }
        }

    }

    class MyILBlock
    {
        public MyILBlock(ILContext il) : this(new ILCursor(il)) {}
        public MyILBlock(ILCursor start)
        {
            cStart = start.DefineLabel();
            start.MarkLabel(cStart);
            cEnd = start.Clone();
        }
        public ILLabel cStart;
        public ILCursor cEnd;
    }

    class ItemCheckPatcher : ILoadable
    {
        private Mod mod;

        public const bool PATCH_COOL = false; // PATCH_COOL should be better for the game/tML but breaks the mod...

        public void Load(Mod mod)
        {
            this.mod = mod;

            var cfg = ModContent.GetInstance<DurabilityConfig>();
            if (cfg.fixItemCheck)
            {
                try
                {
                    IL.Terraria.Player.ItemCheck_Inner += Player_ItemCheck_Inner;
                }
                catch (InvalidProgramException)
                {
                    CantPatch = true;
                    mod.Logger.Warn("IL editing for ItemCheckPatcher cannot be compiled, probably it's a Release build");
                }
            }
        }

        enum PartName {
            Intro,
            ItemCheckPart1,
            HandleItemHolding,
            ItemCheckPart2,
            DecrementItemAnimation,
            DecrementItemTime
        };

        public static bool CantPatch { get; private set; } = false;

        private void Player_ItemCheck_Inner(ILContext il)
        {
            /*
            {
                var cPatch = new ILCursor(il);

                cPatch.Emit(OpCodes.Ldarg, 0);
                cPatch.EmitDelegate<Func<Player, float>>(delegate (Player p)
                {
                    return p.HeightOffsetHitboxCenter;
                });
                cPatch.Emit(OpCodes.Stloc, 6);

                cPatch.Emit(OpCodes.Ldarg, 0);
                cPatch.EmitDelegate<Func<Player, Item>>(delegate (Player p)
                {
                    return p.inventory[p.selectedItem];
                });
                cPatch.Emit(OpCodes.Stloc, 5);
            }
            */

            var splitPoints = new Dictionary<PartName, ILCursor>();

            {
                var holdItem = typeof(ItemLoader).GetMethod("HoldItem", BindingFlags.Static | BindingFlags.Public);
                var c = new ILCursor(il);
                if (!c.TryGotoNext(MoveType.Before,
                    i => i.MatchLdloc(out _),
                    i => i.MatchLdarg(0),
                    i => i.MatchCallOrCallvirt(holdItem)
                )) goto ILNotFound;  
                splitPoints.Add(PartName.HandleItemHolding, c);
            }

            {
                var handleMount = typeof(Player).GetMethod("ItemCheck_HandleMount", BindingFlags.Instance | BindingFlags.NonPublic);
                var c = new ILCursor(il);
                if (!c.TryGotoNext(MoveType.Before,
                    i => i.MatchLdarg(0),
                    i => i.MatchCallOrCallvirt(handleMount)
                )) goto ILNotFound;
                splitPoints.Add(PartName.ItemCheckPart1, c);
            }

            {
                var c = new ILCursor(il);
                if (!c.TryGotoNext(MoveType.Before,
                    i => i.MatchLdarg(0),
                    i => i.MatchLdfld<Player>("itemTime"),
                    i => i.MatchLdcI4(0),
                    i => i.MatchCgt() || i.MatchBge(out _) // ???? what if it was a branch ????
                )) goto ILNotFound;
                splitPoints.Add(PartName.DecrementItemTime, c);
            }

            {
                var c = new ILCursor(il);
                if (!c.TryGotoNext(MoveType.Before,
                    i => i.MatchLdarg(0),
                    i => i.MatchLdfld<Player>("itemAnimation"),
                    i => i.MatchLdcI4(0),
                    i => i.MatchBgt(out _)
                    // This is not a very meaningful match...
                )) goto ILNotFound;
                splitPoints.Add(PartName.DecrementItemAnimation, c);
            }

            {
                var emitHeldItemLight = typeof(Player).GetMethod("ItemCheck_EmitHeldItemLight", BindingFlags.Instance | BindingFlags.NonPublic);
                var c = new ILCursor(il);
                if (!c.TryGotoNext(MoveType.Before,
                    i => i.MatchCallOrCallvirt(emitHeldItemLight)
                )) goto ILNotFound;
                if (!c.TryGotoPrev(MoveType.Before,
                    i => i.MatchLdarg(0),
                    i => i.MatchLdfld<Player>("JustDroppedAnItem")
                )) goto ILNotFound;
                splitPoints.Add(PartName.ItemCheckPart2, c);
            }


            // DUMPIL SPLITPOINT DICT
            Dictionary<string, Instruction> namedLabels = new(
                splitPoints.Select(p => KeyValuePair.Create(p.Key.ToString(), p.Value.Next))
            );


            // DUMP CODE
            using (var f = new StreamWriter("D:/ItemCheck_Inner.il"))
            {
                il.DumpIL(f, namedLabels);
            }

            
            // If the patch above worked the order should not be vanilla
            var targetSequence = PATCH_COOL
                ? new[] {
                    // This is the better patch, but breaks the mod :(
                    PartName.ItemCheckPart1,
                    PartName.HandleItemHolding,
                    PartName.ItemCheckPart2,
                    PartName.DecrementItemAnimation,
                    PartName.DecrementItemTime
                }
                : new[] {
                    // Old boring vanilla pre PR ordering
                    PartName.ItemCheckPart1,
                    PartName.DecrementItemAnimation,
                    PartName.HandleItemHolding,
                    PartName.DecrementItemTime,
                    PartName.ItemCheckPart2
                };

            var partBlocks = new Dictionary<PartName, MyILBlock>();

            { // Crawl the method principal branch flow and order the targetSequence into currSeq
                mod.Logger.Debug("Order before patch:");
                PartName currPart = PartName.Intro;
                MyILBlock currBlock = new MyILBlock(il);

                var visited = new List<int>();

                var firstInst = il.Instrs[0];
                while (firstInst != null)
                {
                    currBlock.cEnd = new ILCursor(il).Goto(firstInst);
                    var c = splitPoints.FirstOrDefault(sp => sp.Value.Next.Offset == firstInst.Offset);
                    if (c.Value != null)
                    {
                        // Save and new block
                        mod.Logger.Debug($"  {currPart}");
                        partBlocks.Add(currPart, currBlock);
                        currPart = c.Key;
                        currBlock = new MyILBlock(c.Value);
                    }
                    firstInst = firstInst.NextTakeAll(ref visited);
                }
                mod.Logger.Debug($"  {currPart}");
                partBlocks.Add(currPart, currBlock);
            }

            // Debug info the IL
            mod.Logger.Debug("Parts dump:");
            foreach (var b in partBlocks)
            {
                mod.Logger.Debug($"  {b.Key} -> 0x{b.Value.cStart.Target.Offset:X} to 0x{b.Value.cEnd.Next.Offset:X}");
            }

            // Do the gluing
            mod.Logger.Debug("Target order:");
            var lastPart = PartName.Intro;
            var lastBlock = partBlocks[PartName.Intro];
            foreach (var bName in targetSequence)
            {
                mod.Logger.Debug($"  {lastPart}");
                var nextBlock = partBlocks[bName];

                if (lastBlock.cEnd.Next.Operand is ILLabel nextEmitInst && nextEmitInst.Target.OpCode == OpCodes.Ret)
                {
                    // (lastPart == PartName.HandleItemHolding) // Awckward branch :(
                    lastBlock.cEnd.MoveAfterLabels(); 
                } else
                {
                    lastBlock.cEnd.MoveBeforeLabels();
                }

                if (PATCH_COOL && bName == PartName.HandleItemHolding)
                {
                    // Before jumping to HandleItemHolding prepare the static vars (hardcoded loc is very bad...)
                    var cPatch = lastBlock.cEnd;

                    cPatch.Emit(OpCodes.Ldarg, 0);
                    cPatch.EmitDelegate<Func<Player, float>>(delegate (Player p)
                    {
                        return p.HeightOffsetHitboxCenter;
                    });
                    cPatch.Emit(OpCodes.Stloc, 6);

                    cPatch.Emit(OpCodes.Ldarg, 0);
                    cPatch.EmitDelegate<Func<Player, Item>>(delegate (Player p)
                    {
                        return p.inventory[p.selectedItem];
                    });
                    cPatch.Emit(OpCodes.Stloc, 5);
                }

                lastBlock.cEnd.Emit(OpCodes.Br, nextBlock.cStart);

                lastPart = bName;
                lastBlock = nextBlock;
            }
            mod.Logger.Debug($"  {lastPart}");
            lastBlock.cEnd.Emit(OpCodes.Ret);

            // DUMP CODE
            using (var f = new StreamWriter("D:/ItemCheck_Inner_After.il"))
            {
                il.DumpIL(f, namedLabels);
            }

            // Check Patch (manually on console)
            { // Sequence show on the console
                mod.Logger.Debug("Order after patch:");
                PartName currPart = PartName.Intro;
                MyILBlock currBlock = new MyILBlock(il);

                var visited = new List<int>();

                var firstInst = il.Instrs[0];
                while (firstInst != null)
                {
                    currBlock.cEnd = new ILCursor(il).Goto(firstInst);
                    var c = splitPoints.FirstOrDefault(sp => sp.Value.Next.Offset == firstInst.Offset);
                    if (c.Value != null)
                    {
                        // Save and new block
                        mod.Logger.Debug($"  {currPart}");
                        currPart = c.Key;
                        currBlock = new MyILBlock(c.Value);
                    }
                    firstInst = firstInst.NextTakeAll(ref visited);
                }
                mod.Logger.Debug($"  {currPart}");
            }

            CantPatch = false;
            return;

        ILNotFound:
            CantPatch = true;
            mod.Logger.Warn("No IL Hooks for ItemCheckPatcher, probably it's a Release build");
            return;
        }

        public void Unload()
        {
            
        }
    }
}
