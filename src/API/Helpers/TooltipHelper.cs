using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace SerousCommonLib.API {
	/// <summary>
	/// A helper class for manipulating tooltips
	/// </summary>
	public static class TooltipHelper {
		/// <summary>
		/// Finds the first <see cref="TooltipLine"/> instance in <paramref name="tooltips"/> which contains the phrase <paramref name="searchPhrase"/>, then replaces it with <paramref name="replacePhrase"/>
		/// </summary>
		/// <param name="tooltips">The list of tooltips</param>
		/// <param name="searchPhrase">The search text to look for</param>
		/// <param name="replacePhrase">The replacement text</param>
		public static void FindAndModify(List<TooltipLine> tooltips, string searchPhrase, string replacePhrase) {
			int searchIndex = tooltips.FindIndex(t => t.Text.Contains(searchPhrase));
			if (searchIndex >= 0)
				tooltips[searchIndex].Text = tooltips[searchIndex].Text.Replace(searchPhrase, replacePhrase);
		}

		/// <summary>
		/// Finds the first <see cref="TooltipLine"/> instance in <paramref name="tooltips"/> whose text is exactly equal to <paramref name="fullLine"/> and removes it
		/// </summary>
		/// <param name="tooltips">The list of tooltips</param>
		/// <param name="fullLine">The line to search for</param>
		public static void FindAndRemoveLine(List<TooltipLine> tooltips, string fullLine) {
			int searchIndex = tooltips.FindIndex(t => t.Text == fullLine);
			if (searchIndex >= 0)
				tooltips.RemoveAt(searchIndex);
		}

		/// <summary>
		/// Finds the forst <see cref="TooltipLine"/> instance in <paramref name="tooltips"/> whose text is exactly equal to <paramref name="searchLine"/>, removes it, then adds a series of tooltip lines starting where it was located
		/// </summary>
		/// <param name="mod">The mod adding the tooltips</param>
		/// <param name="tooltips">The list of tooltips</param>
		/// <param name="searchLine">The line to search for</param>
		/// <param name="lineNames">A function taking an integer and returning a line of text</param>
		/// <param name="replaceLines">The lines to insert, separated by <c>'\n'</c></param>
		public static void FindAndInsertLines(Mod mod, List<TooltipLine> tooltips, string searchLine, Func<int, string> lineNames, string replaceLines) {
			int searchIndex = tooltips.FindIndex(t => t.Text == searchLine);
			if (searchIndex >= 0) {
				tooltips.RemoveAt(searchIndex);

				int inserted = 0;
				foreach (var line in replaceLines.Split('\n')) {
					tooltips.Insert(searchIndex++, new TooltipLine(mod, lineNames(inserted), line));
					inserted++;
				}
			}
		}

		private static readonly FieldInfo ModLoader_attackSpeedScalingTooltipVisibility = typeof(ModLoader).GetField("attackSpeedScalingTooltipVisibility", BindingFlags.NonPublic | BindingFlags.Static);

		/// <summary>
		/// Returns the index of the last "TooltipX" line in <paramref name="tooltips"/> or the index of the tooltip line that would be immediately before the tooltip if no "TooltipX" line exists
		/// </summary>
		public static int FindLastTooltipLine(Item item, List<TooltipLine> tooltips) {
			int index = tooltips.FindLastIndex(t => t.Mod == "Terraria" && t.Name.StartsWith("Tooltip"));

			static void TryUpdateIndex(ref int index, List<TooltipLine> tooltips, string searchName) {
				int find = tooltips.FindIndex(t => t.Mod == "Terraria" && t.Name == searchName);

				if (find >= 0)
					index = find;
			}

			if (index < 0) {
				// No tooltip existed.  Find the line that would've been before it
				TryUpdateIndex(ref index, tooltips, "ItemName");

				if (item.favorited) {
					TryUpdateIndex(ref index, tooltips, "FavoriteDesc");

					if (Main.LocalPlayer.chest != -1) {
						ChestUI.GetContainerUsageInfo(out bool _, out Item[] chestinv);

						if (ChestUI.IsBlockedFromTransferIntoChest(item, chestinv))
							TryUpdateIndex(ref index, tooltips, "NoTransfer");
					}
				}

				if (item.social)
					TryUpdateIndex(ref index, tooltips, "SocialDesc");
				else {
					if (item.damage > 0 && (!item.notAmmo || item.useStyle != ItemUseStyleID.None) && (item.type < ItemID.CopperCoin || item.type > ItemID.PlatinumCoin || Main.LocalPlayer.HasItem(905)) && item.DamageType.ShowStatTooltipLine(Main.LocalPlayer, "Damage")) {
						TryUpdateIndex(ref index, tooltips, "Damage");

						if (item.DamageType.UseStandardCritCalcs && item.DamageType.ShowStatTooltipLine(Main.LocalPlayer, "CritChance"))
							TryUpdateIndex(ref index, tooltips, "CritChance");

						if (item.useStyle != ItemUseStyleID.None && item.DamageType.ShowStatTooltipLine(Main.LocalPlayer, "Speed")) {
							TryUpdateIndex(ref index, tooltips, "Speed");

							int attackSpeedScalingTooltipVisibility = (int)ModLoader_attackSpeedScalingTooltipVisibility.GetValue(null);

							if (item.DamageType == DamageClass.MeleeNoSpeed || ItemID.Sets.BonusAttackSpeedMultiplier[item.type] == 0f) {
								if (attackSpeedScalingTooltipVisibility == 0)
									TryUpdateIndex(ref index, tooltips, "NoSpeedScaling");
							} else if (ItemID.Sets.BonusAttackSpeedMultiplier[item.type] != 1f) {
								if (attackSpeedScalingTooltipVisibility <= 1)
									TryUpdateIndex(ref index, tooltips, "SpecialSpeedScaling");
							}
						}

						if (item.DamageType.ShowStatTooltipLine(Main.LocalPlayer, "Knockback"))
							TryUpdateIndex(ref index, tooltips, "Knockback");
					}

					if (item.fishingPole > 0)
						TryUpdateIndex(ref index, tooltips, "NeedsBait");

					if (item.bait > 0)
						TryUpdateIndex(ref index, tooltips, "BaitPower");

					if (item.headSlot > 0 || item.bodySlot > 0 || item.legSlot > 0 || item.accessory || Main.projHook[item.shoot] || item.mountType != -1 || (item.buffType > 0 && (Main.lightPet[item.buffType] || Main.vanityPet[item.buffType]))) {
						if (!((item.type == ItemID.DiscountCard || item.type == ItemID.GreedyRing) && Main.npcShop > 0))
							TryUpdateIndex(ref index, tooltips, "Equipable");
					}

					if (item.tileWand > 0)
						TryUpdateIndex(ref index, tooltips, "WandConsumes");

					if (item.questItem)
						TryUpdateIndex(ref index, tooltips, "Quest");

					if (item.vanity)
						TryUpdateIndex(ref index, tooltips, "Vanity");

					if (!item.vanity && item.FitsAccessoryVanitySlot)
						TryUpdateIndex(ref index, tooltips, "VanityLegal");

					if (item.defense > 0)
						TryUpdateIndex(ref index, tooltips, "Defense");

					if (item.pick > 0)
						TryUpdateIndex(ref index, tooltips, "PickPower");

					if (item.axe > 0)
						TryUpdateIndex(ref index, tooltips, "AxePower");

					if (item.hammer > 0)
						TryUpdateIndex(ref index, tooltips, "HammerPower");

					if (item.tileBoost != 0)
						TryUpdateIndex(ref index, tooltips, "TileBoost");

					if (item.healLife > 0)
						TryUpdateIndex(ref index, tooltips, "HealLife");

					if (item.healMana > 0)
						TryUpdateIndex(ref index, tooltips, "HealMana");

					if (item.mana > 0 && (item.type != ItemID.SpaceGun || !Main.LocalPlayer.spaceGun))
						TryUpdateIndex(ref index, tooltips, "UseMana");

					if (item.createWall > 0 || item.createTile > -1) {
						if (item.type != ItemID.StaffofRegrowth && item.tileWand < 1)
							TryUpdateIndex(ref index, tooltips, "Placeable");
					} else if (item.ammo > 0 && !item.notAmmo)
						TryUpdateIndex(ref index, tooltips, "Ammo");
					else if (item.consumable)
						TryUpdateIndex(ref index, tooltips, "Consumable");

					if (item.material)
						TryUpdateIndex(ref index, tooltips, "Material");

					// Tooltip would be here
				}
			}

			return index;
		}
	}
}
