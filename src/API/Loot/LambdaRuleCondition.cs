using System;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

namespace SerousCommonLib.API.Loot {
	/// <summary>
	/// A rule condition that uses a lambda expression to determine if an item can drop
	/// </summary>
	public class LambdaRuleCondition : IItemDropRuleCondition {
		private readonly Func<DropAttemptInfo, bool> _condition;
		private readonly Func<bool> _showInUI;
		private readonly LocalizedText _description;

		/// <summary>
		/// Creates a new instance of <see cref="LambdaRuleCondition"/> with the provided condition, UI visibility and description
		/// </summary>
		/// <param name="condition">The drop condition</param>
		/// <param name="showInUI">A function indicating whether the loot rule is visible</param>
		/// <param name="description">The description of the loot rule</param>
		public LambdaRuleCondition(Func<DropAttemptInfo, bool> condition, Func<bool> showInUI, LocalizedText description) {
			_condition = condition;
			_showInUI = showInUI;
			_description = description;
		}

		/// <summary>
		/// Creates a new instance of <see cref="LambdaRuleCondition"/> with the provided condition and description that is always visible
		/// </summary>
		/// <param name="condition">The drop condition</param>
		/// <param name="description">The description of the loot rule</param>
		public LambdaRuleCondition(Func<DropAttemptInfo, bool> condition, LocalizedText description) : this(condition, AlwaysVisible, description) { }

		/// <summary>
		/// Creates a new instance of <see cref="LambdaRuleCondition"/> with the provided condition and description with a constant UI visibility
		/// </summary>
		/// <param name="condition">The drop condition</param>
		/// <param name="showInUI">Whether the loot rule is visible</param>
		/// <param name="description">The description of the loot rule</param>
		public LambdaRuleCondition(Func<DropAttemptInfo, bool> condition, bool showInUI, LocalizedText description) : this(condition, showInUI ? AlwaysVisible : NeverVisible, description) { }

		private static bool AlwaysVisible() => true;

		private static bool NeverVisible() => false;

		/// <inheritdoc/>
		public bool CanDrop(DropAttemptInfo info) => _condition(info);
		
		/// <inheritdoc/>
		public bool CanShowItemDropInUI() => _showInUI();
		
		/// <inheritdoc/>
		public string GetConditionDescription() => _description.Value;
	}
}
