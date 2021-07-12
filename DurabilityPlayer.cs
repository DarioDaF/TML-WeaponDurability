using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Durability
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
                if (!player.HeldItem.IsAir)
                {
                    DurabilityItem.RestoreDurability(player.HeldItem, player);
                }
            }
        }

        public override void PostUpdate()
        {
            if (player.dash == 2 && lastEocDash == 0 && player.eocDash > 0)
            {
                //Durability.log += $"\n@@@ DASH {player.eocDash} - {player.dashDelay} - {player.dashTime}!!!";
                for (int i = 3; i < 8 + player.extraAccessorySlots; i++)
                {
                    if (player.armor[i].type == ItemID.EoCShield)
                    {
                        DurabilityItem.ConsumeDurability(player.armor[i], player);
                    }
                }
            }
            lastEocDash = player.eocDash;
        }

        public override bool PreItemCheck()
        {
            duringUsing = false;
            forcedUsing = false;
            var item = player.HeldItem;
            if (item.IsAir)
                return true;

            if (player.controlUseItem)
            {
                // CanUseItem seems to ignore mana ammo and conditions, what is this for??? only modded probably...
                // Now spamclicking mage is very bad... but sword works??? idk...
                forcedUsing = !item.autoReuse && player.releaseUseItem && player.itemTime == 0;
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
            var item = player.HeldItem;
            if (item.IsAir || !DurabilityItem.IsWeapon(item))
                return;

            if (forcedUsing && player.itemAnimation > 0)
            {
                //Durability.log += $"\n### FORCED {item.useStyle}";
                DurabilityItem.ConsumeDurability(item, player);

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
                if (player.itemTime == item.useTime)
                {
                    //Durability.log += $"\n### DURING {item.useStyle}";
                    DurabilityItem.ConsumeDurability(item, player);
                }
            }
        }
    }
}
