using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace SerousCommonLib.API {
	/// <summary>
	/// A helper class for manipulating item/NPC loot
	/// </summary>
	public static class LootHelper {
		/// <summary>
		/// Recursively searches through all rules and subrules within <paramref name="rules"/> and returns the first successful match
		/// </summary>
		public static IItemDropRule FindRecursive(this List<IItemDropRule> rules, Predicate<IItemDropRule> predicate) {
			IItemDropRule rule = rules.Find(predicate);

			if (rule is not null)
				return rule;

			foreach (var item in rules) {
				rule = item.ChainedRules?.FindRecursive(predicate);

				if (rule is not null)
					return rule;
			}

			return null;
		}

		/// <summary>
		/// Recursively searches through all rules and subrules within <paramref name="rules"/> and returns the first successful match
		/// </summary>
		public static IItemDropRule FindRecursive(this List<IItemDropRuleChainAttempt> rules, Predicate<IItemDropRule> predicate) {
			IItemDropRule rule = rules.Find(x => predicate(x.RuleToChain))?.RuleToChain;

			if (rule is not null)
				return rule;

			foreach (var item in rules) {
				rule = item.RuleToChain?.ChainedRules?.FindRecursive(predicate);

				if (rule is not null)
					return rule;
			}

			return null;
		}

		/// <summary>
		/// Returns <see langword="true"/> if no Twins are alive
		/// </summary>
		public static bool IsLastTwinStanding(DropAttemptInfo info) {
			NPC npc2 = info.npc;
			if (npc2 is null)
				return false;

			if (npc2.type == NPCID.Retinazer)
				return !NPC.AnyNPCs(NPCID.Spazmatism);

			if (npc2.type == NPCID.Spazmatism)
				return !NPC.AnyNPCs(NPCID.Retinazer);

			return false;
		}
	}
}
