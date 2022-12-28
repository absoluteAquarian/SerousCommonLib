using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace SerousCommonLib.API {
	public static class LootHelper {
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
