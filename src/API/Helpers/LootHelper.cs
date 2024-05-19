using SerousCommonLib.API.Loot;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SerousCommonLib.API {
	/// <summary>
	/// A helper class for manipulating item/NPC loot
	/// </summary>
	public static class LootHelper {
		/// <summary>
		/// Recursively searches through all rules and subrules within <paramref name="loot"/> and returns the first successful match
		/// </summary>
		public static IItemDropRule FindRecursive(this ILoot loot, Predicate<IItemDropRule> predicate, bool includeGlobalDrops = true) => loot.Get(includeGlobalDrops).FindRecursive(predicate);

		/// <inheritdoc cref="FindRecursive(ILoot, Predicate{IItemDropRule}, bool)"/>
		public static IItemDropRule FindRecursive(this ILoot loot, Func<IItemDropRule, bool> predicate, bool includeGlobalDrops = true) => loot.FindRecursive(new Predicate<IItemDropRule>(predicate), includeGlobalDrops);

		/// <summary>
		/// Recursively searches through all rules and subrules within <paramref name="rule"/> and returns the first successful match<br/>
		/// If <paramref name="rule"/> is a match, it will be returned
		/// </summary>
		public static IItemDropRule FindRecursive(this IItemDropRule rule, Predicate<IItemDropRule> predicate) {
			if (predicate(rule))
				return rule;

			if (rule.ChainedRules is not null && rule.ChainedRules.FindRecursive(predicate) is IItemDropRule chainedRule)
				return chainedRule;

			return null;
		}

		/// <inheritdoc cref="FindRecursive(IItemDropRule, Predicate{IItemDropRule})"/>
		public static IItemDropRule FindRecursive(this IItemDropRule rule, Func<IItemDropRule, bool> predicate) => rule.FindRecursive(new Predicate<IItemDropRule>(predicate));

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

		/// <inheritdoc cref="FindRecursive(List{IItemDropRule}, Predicate{IItemDropRule})"/>/>
		public static IItemDropRule FindRecursive(this List<IItemDropRule> rules, Func<IItemDropRule, bool> predicate) => rules.FindRecursive(new Predicate<IItemDropRule>(predicate));

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

		/// <inheritdoc cref="FindRecursive(List{IItemDropRuleChainAttempt}, Predicate{IItemDropRule})"/>
		public static IItemDropRule FindRecursive(this List<IItemDropRuleChainAttempt> rules, Func<IItemDropRule, bool> predicate) => rules.FindRecursive(new Predicate<IItemDropRule>(predicate));

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

		/// <inheritdoc cref="LambdaRuleCondition(Func{DropAttemptInfo, bool}, Func{bool}, LocalizedText)"/>
		public static LambdaRuleCondition Lambda(Func<DropAttemptInfo, bool> condition, Func<bool> showInUI, LocalizedText description) => new(condition, showInUI, description);

		/// <inheritdoc cref="LambdaRuleCondition(Func{DropAttemptInfo, bool}, LocalizedText)"/>
		public static LambdaRuleCondition Lambda(Func<DropAttemptInfo, bool> condition, LocalizedText description) => new(condition, description);

		/// <inheritdoc cref="LambdaRuleCondition(Func{DropAttemptInfo, bool}, bool, LocalizedText)"/>
		public static LambdaRuleCondition Lambda(Func<DropAttemptInfo, bool> condition, bool showInUI, LocalizedText description) => new(condition, showInUI, description);
	}
}
