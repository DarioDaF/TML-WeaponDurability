using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

// NEEDS MACE MAX DURATION

namespace WeaponDurability
{
    class DurabilityPlayer : ModPlayer
    {
        private bool duringUsing = false;
        private bool forcedUsing = false;
        private int lastEocDash = 0;
        public int thrownCount = 0;

        private Dictionary<int, int> buffKiller = new Dictionary<int, int>();

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Durability.HotReload.JustPressed)
            {
                //Durability.log = "...";
                if (!Player.HeldItem.IsAir)
                {
                    if (DurabilityItem.RestoreDurability(Player.HeldItem, Player))
                    {
                        if (Player.selectedItem == 58)
                        {
                            Main.mouseItem = Player.inventory[58].Clone(); // Refresh mouse item???
                        }
                        // Write in chat CHEATER
                        Main.NewText("Item durability forcefully restored", Color.Red);
                    }
                }
            }
            if (Durability.FastSwap.JustPressed)
            {
                if (Player.itemAnimation == 0 && Player.itemTime == 0 && Player.reuseDelay == 0)
                {
                    // Switch rows
                    for (int col = 0; col < 10; ++col)
                    {
                        Item oldItem = Player.inventory[col];
                        for (int row = 4; row >= 0; --row)
                        {
                            int idx = row * 10 + col;
                            // Swap oldItem with inventory[idx]
                            Item tmp = Player.inventory[idx];
                            Player.inventory[idx] = oldItem;
                            oldItem = tmp;
                        }
                    }
                }
            }
        }

        public override void PostUpdate()
        {
            if (Player.dash == 2 && lastEocDash == 0 && Player.eocDash > 0)
            {
                //Durability.log += $"\n@@@ DASH {player.eocDash} - {player.dashDelay} - {player.dashTime}!!!";
                for (int i = 3; i < Player.armor.Length; i++)
                {
                    if (Player.armor[i].type == ItemID.EoCShield)
                    {
                        DurabilityItem.ConsumeDurability(Player.armor[i], Player);
                    }
                }
            }
            lastEocDash = Player.eocDash;
        }

        public override bool PreItemCheck()
        {
            duringUsing = false;
            forcedUsing = false;
            var item = Player.HeldItem;
            if (item.IsAir)
                return true;

            if (Player.controlUseItem)
            {
                // CanUseItem seems to ignore mana ammo and conditions, what is this for??? only modded probably...
                // Now spamclicking mage is very bad... but sword works??? idk...
                forcedUsing = !item.autoReuse && Player.releaseUseItem && Player.itemTime == 0;
                duringUsing = item.useTime != 0;
            }
            return true;
        }

        public override void PreUpdateBuffs()
        {
            foreach (var t in buffKiller.Keys.ToList()) // Allow edit
            {
                buffKiller[t] -= 1;
                if (buffKiller[t] <= 0)
                {
                    //player.ClearBuff(t);
                    buffKiller.Remove(t);
                }
            }
        }

        public override void PostItemCheck()
        {
            var item = Player.HeldItem;
            if (item.IsAir || !DurabilityItem.IsWeapon(item))
                return;

            if (ItemID.Sets.gunProj[item.type])
            {
                // Skip it cause it's special and computed at proj subspawn level
            }
            else if (forcedUsing && Player.itemAnimation > 0)
            {
                //Durability.log += $"\n### FORCED {item.useStyle}";
                DurabilityItem.ConsumeDurability(item, Player);

                /*
                // Should always do?
                if (item.buffType > 0)
                {
                    if (!buffKiller.ContainsKey(item.buffType))
                    {
                        //Durability.log += $"\nAdded buff {item.buffType}";
                        var conf = ModContent.GetInstance<DurabilityConfig>();
                        buffKiller[item.buffType] = conf.summonTime * 60;
                    }
                }
                */
            }
            else if (duringUsing)
            {
                //Durability.log += $"\n### DURING {item.useStyle} - {player.itemAnimation} / {player.itemAnimationMax}";
                //Durability.log += $"\n>>> {player.reuseDelay}";
                var killDatSickle = Player.itemAnimation == Player.itemAnimationMax - 1 && Player.itemTime == 0;
                if ((Player.itemTime == item.useTime) || killDatSickle)
                {
                    //Durability.log += $"\n### DURING {item.useStyle}";
                    DurabilityItem.ConsumeDurability(item, Player);
                }
            }
        }
    }
}
