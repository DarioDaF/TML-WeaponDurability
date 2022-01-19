using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace WeaponDurability
{
    class DurabilityItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        // @TML: Why would I need this and also the `new` one doesnt populate Mod field crashing on world save
        public override GlobalItem Clone(Item item, Item itemClone) => base.Clone(item, itemClone);

        public float durability;

        public static int GetValueWithPrefix(Item item)
        {
            var x = new Item();
            x.SetDefaults(item.type);
            x.Prefix(item.prefix);
            return x.value;
        }

        public void UpdateValue(Item item)
        {
            item.value = (int)(durability * GetValueWithPrefix(item));
        }

        public static List<int> ProjShow = new List<int> {
            ProjectileID.RocketFireworkBlue,
            ProjectileID.RocketFireworkGreen,
            ProjectileID.RocketFireworkRed,
            ProjectileID.RocketFireworkYellow
        };

        public override void SetDefaults(Item item)
        {
            durability = 1;
            if (item.type == ModContent.ItemType<DepleatedEoCShield>())
            {
                durability = 0;
            }

            var conf = ModContent.GetInstance<DurabilityConfig>();
            if (conf.noStacking && IsWeapon(item) && (!item.consumable))
            {
                item.maxStack = 1; // Yep seems harsh but solves some bugz
            }
        }

        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (IsWeapon(item))
            {

                if (item.consumable)
                {
                    var r = new Rectangle((int)position.X, (int)position.Y, (int)(frame.Width * scale), (int)(frame.Height * scale));
                    var bottomR = r;
                    bottomR.Y += bottomR.Height;
                    bottomR.Height = (int)(bottomR.Height * .2f);
                    bottomR.X += bottomR.Width / 2;
                    bottomR.Width = (int)(bottomR.Height);
                    bottomR.X -= bottomR.Width / 2;

                    var border = bottomR;
                    border.Inflate(1, 1);
                    Drawing.DrawRect(spriteBatch, border, Color.Black);
                }
                else
                {
                    var r = new Rectangle((int)position.X, (int)position.Y, (int)(frame.Width * scale), (int)(frame.Height * scale));
                    var bottomR = r;
                    bottomR.Y += bottomR.Height;
                    bottomR.Height = (int)(bottomR.Height * .2f);
                    bottomR.X += bottomR.Width / 2;
                    bottomR.Width = (int)(Main.inventoryScale * 40);
                    bottomR.X -= bottomR.Width / 2;

                    var border = bottomR;
                    border.Inflate(1, 1);
                    Drawing.DrawRect(spriteBatch, border, Color.Black);

                    Drawing.DrawRect(spriteBatch, bottomR, Color.Red);

                    bottomR.Width = (int)(bottomR.Width * durability);
                    Drawing.DrawRect(spriteBatch, bottomR, Color.Green);
                }
            }
            return true;
        }

        // Legacy method
        public TagCompound Save(Item item)
        {
            var s = new TagCompound();
            s.Add("DaF_Durability", durability);
            return s;
        }

        // Legacy method
        public void Load(Item item, TagCompound tag)
        {
            durability = tag.GetFloat("DaF_Durability");
            UpdateValue(item);
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            if (IsWeapon(item) && (durability < 1.0))
                tag.Add("DaF_Durability", durability);
        }
        public override void LoadData(Item item, TagCompound tag)
        {
            if (IsWeapon(item))
            {
                if (tag.ContainsKey("DaF_Durability"))
                {
                    durability = tag.GetFloat("DaF_Durability");
                    UpdateValue(item);
                }
            }
        }

        public static bool IsWeapon(Item item)
        {
            // Should do damage and not be ammo (is any ammo standardly throwable??? idk)
            return (item.damage > 0) && (item.ammo == AmmoID.None);
        }

        // Legacy method
        public bool NeedsSaving(Item item)
        {
            return IsWeapon(item);
        }

        /*
        public override bool Shoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (IsWeapon(item))
            {
                //Durability.log += "\nGot use shoot";
            }
            return true;
        }
        */

        public override bool PreReforge(Item item)
        {
            RestoreDurability(item, Main.clientPlayer);
            return true;
        }

        public static bool ConsumeDurability(Item item, Player player)
        {
            var conf = ModContent.GetInstance<DurabilityConfig>();
            var ditem = item.GetGlobalItem<DurabilityItem>();

            if (item.consumable)
            {
                var p = player.GetModPlayer<DurabilityPlayer>();
                if (!player.HasBuff(ModContent.BuffType<ConsumableDizzyness>()))
                {
                    p.thrownCount = 1;
                }
                player.AddBuff(ModContent.BuffType<ConsumableDizzyness>(), 60*conf.coolThrow);
            }
            else
            {
                ditem.durability -= 1f / (conf.uses * item.stack);
                if (ditem.durability <= 0)
                {
                    ditem.durability = 0;
                    if (item.type == ItemID.EoCShield)
                    {
                        item.SetDefaults(ModContent.ItemType<DepleatedEoCShield>());
                    }
                }
                ditem.UpdateValue(item);
            }

            return ditem.durability > 0;
        }

        public override void ExtractinatorUse(int extractType, ref int resultType, ref int resultStack)
        {
            if (extractType == ItemID.SiltBlock)
            {
                var conf = ModContent.GetInstance<DurabilityConfig>();

                //resultType = 74;
                var source = Main.player[Main.myPlayer].GetProjectileSource_TileInteraction(
                    (int)Math.Round(Main.MouseWorld.X),
                    (int)Math.Round(Main.MouseWorld.Y)
                );
                if (resultType == ItemID.PlatinumCoin)
                {
                    // EXPLODE ALL!!!
                    for (var i = 0; i < 20; ++i)
                    {
                        var proj = Main.rand.Next(ProjShow);
                        var x = 20f * (Main.rand.NextFloat() - .5f);
                        var y = -(5f + 5f * Main.rand.NextFloat());        
                        Projectile.NewProjectileDirect(source, Main.MouseWorld, new Vector2(x, y), proj, 0, 0, Main.LocalPlayer.whoAmI);
                    }
                }
                else if (Main.rand.Next() % conf.exProb == 0)
                {
                    var proj = Main.rand.Next(ProjShow);
                    var x = 5f * (Main.rand.NextFloat() - .5f);
                    var y = -(5f + 5f * Main.rand.NextFloat());
                    Projectile.NewProjectileDirect(source, Main.MouseWorld, new Vector2(x, y), proj, 0, 0, Main.LocalPlayer.whoAmI);
                    // Cool effects
                }
            }
        }

        public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
        {
            reforgePrice = GetValueWithPrefix(item);
            return true;
        }

        public static bool RestoreDurability(Item item, Player player)
        {
            var ditem = item.GetGlobalItem<DurabilityItem>();
            if (IsWeapon(item))
            {
                ditem.durability = 1;
                if (item.type == ModContent.ItemType<DepleatedEoCShield>())
                {
                    item.SetDefaults(ItemID.EoCShield);
                }
                ditem.UpdateValue(item);
                return true;
            }
            return false;
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (!IsWeapon(item))
                return true;
            if (item.consumable)
            {
                return !player.HasBuff(ModContent.BuffType<ConsumableSickness>());
            }
            else
            {
                return durability > 0;
            }
        }

        /*
        public override bool UseItem(Item item, Player player)
        {
            if (IsWeapon(item))
            {
                if (player.itemAnimation == 1)
                {
                    //Durability.log += "\nGot use in use";
                }
            }
            // Fix summioning and turrets?
            return false;
        }
        */

    }
}
