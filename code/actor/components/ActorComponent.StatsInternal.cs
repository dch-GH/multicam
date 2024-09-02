﻿using Sandbox;

using System;
using System.Collections.Generic;

namespace Ironsim.Actor;

public partial class ActorComponent
{
	/// <summary>
	/// We only want to network the computed stats of the actor, that way we dont have to do anything
	/// fancy or complicated on the client, and the stats only have to be calculated once.
	///
	/// These are our dynamically changing stats. Some stats will actually use a combination of these.
	/// </summary>
	private Dictionary<StatType, float> StatsDictionary { get; set; } = new();

	/// <summary>
	/// These are the real, internal stats of the player. They're not networked and are used to update the values
	/// that will be networked.
	/// </summary>
	private readonly Dictionary<StatType, StatValue> internalStatsDictionary = new();

	/// <summary>
	/// These are call backs that are run when a stat changes.
	/// Provides the old and new values of the stat.
	/// Only run serverside.
	/// </summary>
	private readonly Dictionary<StatType, Action<float, float>> statChangeCallbacks = new();

	public void AddStatModifier(StatModifier statModifier)
	{
		//apply this modifier to the internal StatValue
		var statValue = internalStatsDictionary[statModifier.StatType];

		var oldValue = statValue.Effective;

		statModifier.Apply(statValue);

		//set our networked stat to our effective stat value.
		StatsDictionary[statModifier.StatType] = statValue.Effective;

		if (statChangeCallbacks.TryGetValue(statModifier.StatType, out var action))
			action.Invoke(oldValue, statValue.Effective);
	}

	public void AddStatModifier(StatType statType, StatModifier.ModifierType modifierType, float value)
	{
		AddStatModifier(new StatModifier(statType, modifierType, value));
	}

	public void RemoveStatModifier(StatModifier statModifier)
	{
		var statValue = internalStatsDictionary[statModifier.StatType];

		var oldValue = statValue.Effective;

		statModifier.Remove(statValue);

		//set our networked stat to our effective stat value.
		StatsDictionary[statModifier.StatType] = statValue.Effective;

		if (statChangeCallbacks.TryGetValue(statModifier.StatType, out var action))
			action.Invoke(oldValue, statValue.Effective);
	}

	public void RemoveStatModifier(StatType statType, StatModifier.ModifierType modifierType, float value)
	{
		RemoveStatModifier(new StatModifier(statType, modifierType, value));
	}

	internal float GetStat(StatType type, float backup = 1)
	{
		if (!StatsDictionary.ContainsKey(type))
		{
			Log.Error($"Tried to get unregistered stat: {type.ToString()}");
			return backup;
		}

		return StatsDictionary[type];
	}

	public void ResetStats()
	{
		foreach (var keyValuePair in internalStatsDictionary)
		{
			keyValuePair.Value.Reset();
			StatsDictionary[keyValuePair.Key] = keyValuePair.Value.Effective;
		}
	}

	/// <summary>
	/// Called when we first spawn the actor so all our stat values are filled out.
	/// Sets the internal stat value on server.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="value"></param>
	private void InitializeStat(StatType type, float value)
	{
		StatsDictionary[type] = value;
		internalStatsDictionary[type] = new StatValue(value);
	}

	private void SetBaseStat(StatType type, float value)
	{
		var oldValue = internalStatsDictionary[type].Effective;

		internalStatsDictionary[type].Base = value;
		StatsDictionary[type] = internalStatsDictionary[type].Effective;

		if (statChangeCallbacks.TryGetValue(type, out var action))
			action.Invoke(oldValue, internalStatsDictionary[type].Effective);
	}

	private void AddStatCallback(StatType type, Action<float, float> action)
	{
		if (!statChangeCallbacks.ContainsKey(type))
			statChangeCallbacks[type] = action;
		else
			statChangeCallbacks[type] += action;
	}

	private void RemoveStatCallback(StatType type, Action<float, float> action)
	{
		statChangeCallbacks[type] -= action;
	}

	/// <summary>
	/// This is called purely for hotloading purposes.
	/// Allows us to update our base stats when we change our StatsResource object.
	/// </summary>
	private void ReloadBaseStats()
	{
		SetBaseStat(StatType.Strength, BaseStats.Core.Strength);
		SetBaseStat(StatType.Dexterity, BaseStats.Core.Dexterity);
		SetBaseStat(StatType.Constitution, BaseStats.Core.Constitution);
		SetBaseStat(StatType.Wisdom, BaseStats.Core.Wisdom);
		SetBaseStat(StatType.Intelligence, BaseStats.Core.Intelligence);
		SetBaseStat(StatType.Charisma, BaseStats.Core.Charisma);

		//Health
		SetBaseStat(StatType.MaxHealth, BaseStats.Health.Max);
		SetBaseStat(StatType.HealthRegen, BaseStats.Health.Regen);
		SetBaseStat(StatType.HealthRegenDelay, BaseStats.Health.RegenDelay);
		SetBaseStat(StatType.HealMultiplier, BaseStats.Health.HealMultiplier);

		//Stamina
		SetBaseStat(StatType.MaxStamina, BaseStats.Stamina.Max);
		SetBaseStat(StatType.StaminaRegen, BaseStats.Stamina.Regen);
		SetBaseStat(StatType.StaminaRegenDelay, BaseStats.Stamina.RegenDelay);
		SetBaseStat(StatType.StaminaCostMultiplier, BaseStats.Stamina.CostMultiplier);

		//Concentration
		SetBaseStat(StatType.MaxConcentration, BaseStats.Concentration.Max);
		SetBaseStat(StatType.ConcentrationRegen, BaseStats.Concentration.Regen);
		SetBaseStat(StatType.ConcentrationRegenDelay, BaseStats.Concentration.RegenDelay);
		SetBaseStat(StatType.ConcentrationCostMultiplier, BaseStats.Concentration.CostMultiplier);

		//Posture
		SetBaseStat(StatType.MaxPosture, BaseStats.Posture.Max);
		SetBaseStat(StatType.PostureRegen, BaseStats.Posture.Regen);
		SetBaseStat(StatType.PostureRegenDelay, BaseStats.Posture.RegenDelay);

		//Actions
		SetBaseStat(StatType.ActionSpeed, BaseStats.Actions.ActionSpeed);
		SetBaseStat(StatType.SpellCastingSpeed, BaseStats.Actions.SpellCastingSpeed);

		//Movement
		SetBaseStat(StatType.MoveSpeedMultiplier, BaseStats.Actions.MoveSpeedMultiplier);
		SetBaseStat(StatType.JumpHeightMultiplier, BaseStats.Actions.JumpHeightMultiplier);

		//Combat
		SetBaseStat(StatType.PhysicalPower, BaseStats.Combat.PhysicalPower);
		SetBaseStat(StatType.SpellPower, BaseStats.Combat.SpellPower);

		SetBaseStat(StatType.FirePower, BaseStats.Combat.FirePower);
		SetBaseStat(StatType.FrostPower, BaseStats.Combat.FrostPower);
		SetBaseStat(StatType.ElectricPower, BaseStats.Combat.ElectricPower);
		SetBaseStat(StatType.PoisonPower, BaseStats.Combat.PoisonPower);
		SetBaseStat(StatType.NecroticPower, BaseStats.Combat.NecroticPower);

		SetBaseStat(StatType.ArcanePower, BaseStats.Combat.ArcanePower);
		SetBaseStat(StatType.DivinePower, BaseStats.Combat.DivinePower);
		SetBaseStat(StatType.OccultPower, BaseStats.Combat.OccultPower);

		SetBaseStat(StatType.ArmorPenetration, BaseStats.Combat.ArmorPenetration);
		SetBaseStat(StatType.WardingPenetration, BaseStats.Combat.WardingPenetration);

		SetBaseStat(StatType.FirePenetration, BaseStats.Combat.FirePenetration);
		SetBaseStat(StatType.FrostPenetration, BaseStats.Combat.FrostPenetration);
		SetBaseStat(StatType.ElectricPenetration, BaseStats.Combat.ElectricPenetration);
		SetBaseStat(StatType.PoisonPenetration, BaseStats.Combat.PoisonPenetration);
		SetBaseStat(StatType.NecroticPenetration, BaseStats.Combat.NecroticPenetration);

		SetBaseStat(StatType.ArcanePenetration, BaseStats.Combat.ArcanePenetration);
		SetBaseStat(StatType.DivinePenetration, BaseStats.Combat.DivinePenetration);
		SetBaseStat(StatType.OccultPenetration, BaseStats.Combat.OccultPenetration);

		SetBaseStat(StatType.CriticalMultiplier, BaseStats.Combat.CriticalMultiplier);
		SetBaseStat(StatType.KnockBack, BaseStats.Combat.KnockBack);

		//Defense
		SetBaseStat(StatType.Armor, BaseStats.Resistances.Armor);
		SetBaseStat(StatType.Warding, BaseStats.Resistances.Warding);
		SetBaseStat(StatType.CriticalNegation, BaseStats.Resistances.CriticalNegation);
		SetBaseStat(StatType.KnockBackResistance, BaseStats.Resistances.KnockBackResistance);

		SetBaseStat(StatType.FireResistance, BaseStats.Resistances.FireResistance);
		SetBaseStat(StatType.FrostResistance, BaseStats.Resistances.FrostResistance);
		SetBaseStat(StatType.ElectricResistance, BaseStats.Resistances.ElectricResistance);
		SetBaseStat(StatType.PoisonResistance, BaseStats.Resistances.PoisonResistance);
		SetBaseStat(StatType.NecroticResistance, BaseStats.Resistances.NecroticResistance);

		SetBaseStat(StatType.ArcaneResistance, BaseStats.Resistances.ArcaneResistance);
		SetBaseStat(StatType.DivineResistance, BaseStats.Resistances.DivineResistance);
		SetBaseStat(StatType.OccultResistance, BaseStats.Resistances.OccultResistance);

		//Misc
		SetBaseStat(StatType.CarryWeight, BaseStats.Misc.CarryWeight);
		SetBaseStat(StatType.Luck, BaseStats.Misc.Luck);
	}

	private void InitializeStats()
	{
		//Core stats
		InitializeStat(StatType.Strength, BaseStats.Core.Strength);
		InitializeStat(StatType.Dexterity, BaseStats.Core.Dexterity);

		InitializeStat(StatType.Constitution, BaseStats.Core.Constitution);
		AddStatCallback(StatType.Constitution, OnConstitutionChanged);

		InitializeStat(StatType.Wisdom, BaseStats.Core.Wisdom);
		InitializeStat(StatType.Intelligence, BaseStats.Core.Intelligence);
		InitializeStat(StatType.Charisma, BaseStats.Core.Charisma);

		//Health
		InitializeStat(StatType.MaxHealth, BaseStats.Health.Max);
		AddStatCallback(StatType.MaxHealth, OnMaxHealthChanged);

		InitializeStat(StatType.HealthRegen, BaseStats.Health.Regen);
		InitializeStat(StatType.HealthRegenDelay, BaseStats.Health.RegenDelay);
		InitializeStat(StatType.HealMultiplier, BaseStats.Health.HealMultiplier);

		//Stamina
		InitializeStat(StatType.MaxStamina, BaseStats.Stamina.Max);
		AddStatCallback(StatType.MaxStamina, OnMaxStaminaChanged);

		InitializeStat(StatType.StaminaRegen, BaseStats.Stamina.Regen);
		InitializeStat(StatType.StaminaRegenDelay, BaseStats.Stamina.RegenDelay);
		InitializeStat(StatType.StaminaCostMultiplier, BaseStats.Stamina.CostMultiplier);

		//Concentration
		InitializeStat(StatType.MaxConcentration, BaseStats.Concentration.Max);
		InitializeStat(StatType.ConcentrationRegen, BaseStats.Concentration.Regen);
		InitializeStat(StatType.ConcentrationRegenDelay, BaseStats.Concentration.RegenDelay);
		InitializeStat(StatType.ConcentrationCostMultiplier, BaseStats.Concentration.CostMultiplier);

		InitializeStat(StatType.MaxPosture, BaseStats.Posture.Max);
		InitializeStat(StatType.PostureRegen, BaseStats.Posture.Regen);
		InitializeStat(StatType.PostureRegenDelay, BaseStats.Posture.RegenDelay);

		//Actions
		InitializeStat(StatType.ActionSpeed, BaseStats.Actions.ActionSpeed);
		InitializeStat(StatType.SpellCastingSpeed, BaseStats.Actions.SpellCastingSpeed);

		//Movement
		InitializeStat(StatType.MoveSpeedMultiplier, BaseStats.Actions.MoveSpeedMultiplier);
		InitializeStat(StatType.JumpHeightMultiplier, BaseStats.Actions.JumpHeightMultiplier);

		//Combat
		InitializeStat(StatType.PhysicalPower, BaseStats.Combat.PhysicalPower);
		InitializeStat(StatType.SpellPower, BaseStats.Combat.SpellPower);

		InitializeStat(StatType.FirePower, BaseStats.Combat.FirePower);
		InitializeStat(StatType.FrostPower, BaseStats.Combat.FrostPower);
		InitializeStat(StatType.ElectricPower, BaseStats.Combat.ElectricPower);
		InitializeStat(StatType.PoisonPower, BaseStats.Combat.PoisonPower);
		InitializeStat(StatType.NecroticPower, BaseStats.Combat.NecroticPower);

		InitializeStat(StatType.ArcanePower, BaseStats.Combat.ArcanePower);
		InitializeStat(StatType.DivinePower, BaseStats.Combat.DivinePower);
		InitializeStat(StatType.OccultPower, BaseStats.Combat.OccultPower);

		InitializeStat(StatType.ArmorPenetration, BaseStats.Combat.ArmorPenetration);
		InitializeStat(StatType.WardingPenetration, BaseStats.Combat.WardingPenetration);

		InitializeStat(StatType.FirePenetration, BaseStats.Combat.FirePenetration);
		InitializeStat(StatType.FrostPenetration, BaseStats.Combat.FrostPenetration);
		InitializeStat(StatType.ElectricPenetration, BaseStats.Combat.ElectricPenetration);
		InitializeStat(StatType.PoisonPenetration, BaseStats.Combat.PoisonPenetration);
		InitializeStat(StatType.NecroticPenetration, BaseStats.Combat.NecroticPenetration);

		InitializeStat(StatType.ArcanePenetration, BaseStats.Combat.ArcanePenetration);
		InitializeStat(StatType.DivinePenetration, BaseStats.Combat.DivinePenetration);
		InitializeStat(StatType.OccultPenetration, BaseStats.Combat.OccultPenetration);

		InitializeStat(StatType.CriticalMultiplier, BaseStats.Combat.CriticalMultiplier);
		InitializeStat(StatType.KnockBack, BaseStats.Combat.KnockBack);

		//Defense
		InitializeStat(StatType.Armor, BaseStats.Resistances.Armor);
		InitializeStat(StatType.Warding, BaseStats.Resistances.Warding);
		InitializeStat(StatType.CriticalNegation, BaseStats.Resistances.CriticalNegation);
		InitializeStat(StatType.KnockBackResistance, BaseStats.Resistances.KnockBackResistance);

		InitializeStat(StatType.FireResistance, BaseStats.Resistances.FireResistance);
		InitializeStat(StatType.FrostResistance, BaseStats.Resistances.FrostResistance);
		InitializeStat(StatType.ElectricResistance, BaseStats.Resistances.ElectricResistance);
		InitializeStat(StatType.PoisonResistance, BaseStats.Resistances.PoisonResistance);
		InitializeStat(StatType.NecroticResistance, BaseStats.Resistances.NecroticResistance);

		InitializeStat(StatType.ArcaneResistance, BaseStats.Resistances.ArcaneResistance);
		InitializeStat(StatType.DivineResistance, BaseStats.Resistances.DivineResistance);
		InitializeStat(StatType.OccultResistance, BaseStats.Resistances.OccultResistance);

		//Misc
		InitializeStat(StatType.CarryWeight, BaseStats.Misc.CarryWeight);
		InitializeStat(StatType.Luck, BaseStats.Misc.Luck);

		Health = Stats.MaxHealth;
		Stamina = Stats.MaxStamina;
		Concentration = Stats.MaxConcentration;
		Posture = Stats.MaxPosture;
	}

	private void UpdateStats()
	{
		if (TimeSinceDamaged > Stats.HealthRegenDelay && MathF.Abs(Stats.HealthRegen) > 0.01f)
			Heal(Stats.HealthRegen * Time.Delta);

		if (!PostureBroken && TimeSincePostureDamaged > Stats.PostureRegenDelay)
			RecoverPosture(Stats.PostureRegen * Time.Delta);

		if (TimeSinceLastStaminaCost > Stats.StaminaRegenDelay)
			AddStamina(Stats.StaminaRegen * Time.Delta);
	}
}
