using Ironsim.Actor.Marker;
using Ironsim.Actor.Damage;

namespace Ironsim.Actor;

public partial class ActorComponent : IDamageable
{
	[Property, ToggleGroup("Damage")]
	public bool CauseHitBounce { get; set; } = false;

	[Property, ToggleGroup("Damage")]
	public bool PlayHitSound { get; set; } = true;

	[Property, ToggleGroup("Damage")]
	public bool ShowDamageNumbers { get; set; } = true;

	[Property, ToggleGroup("Damage")]
	public GameObject ImpactEffectPrefab { get; set; }

	[Property, ToggleGroup("Damage")]
	public GameObject PostureBreakEffectPrefab { get; set; }

	[Property, ToggleGroup("Damage")]
	public List<HitboxDamageModifier> HitboxDamageModifiers { get; set; }

	/// <summary>
	/// When we take damage, we assume it's going to be mostly unprocessed damage.
	/// </summary>
	/// <param name="damageEventData"></param>
	public void TakeDamage(DamageEventData damageEventData)
	{
		//apply our resistances as we take damage
		ApplyResistances(damageEventData);
		ApplyHitboxModifiers(damageEventData);

		var damageDealtListeners =
			damageEventData.Originator.Components.GetAll<IDamageDealtListener>(
				FindMode.EverythingInSelfAndDescendants).ToList();

		var damageListeners =
			Components.GetAll<IDamageTakenListener>(FindMode.EverythingInSelfAndDescendants).ToList();

		foreach (var damageDealtListener in damageDealtListeners)
		{
			damageDealtListener.OnProcessDamageDealt(damageEventData);
		}

		foreach (var damageListener in damageListeners)
		{
			damageListener.OnProcessDamageTaken(damageEventData);
		}

		if (damageEventData.IsCritical)
			ApplyCritical(damageEventData);

		DamageEventData.CreateDamageEffects(damageEventData.Surface, damageEventData.Position, damageEventData.Direction);

		if (damageEventData.DamageResult > 0)
			DamageHealth(damageEventData.DamageResult);

		if (Health > 0 && damageEventData.PostureDamageResult > 0)
			DamagePosture(damageEventData.PostureDamageResult, damageEventData.Originator.GameObject);

		if (Posture <= 0 && !PostureBroken)
		{
			BreakPosture(damageEventData.Originator.GameObject);

			BroadcastPostureBreakEffect(damageEventData.Position);
		}

		foreach (var damageDealtListener in damageDealtListeners)
		{
			damageDealtListener.OnDamageDealt(damageEventData, Health < 0);
		}

		foreach (var damageListener in damageListeners)
		{
			damageListener.OnDamageTaken(damageEventData, Health < 0);
		}

		//i'm dead now!!
		if (Health <= 0 && Alive)
		{
			Alive = false;

			OnDeath(damageEventData);

			if (EnableDeathSounds && Sounds.DeathSound is not null)
				Sound.Play(Sounds.DeathSound, Body.Transform.Position);
		}
		else if (Alive && damageEventData.DamageResult > 0)
		{
			if (EnablePainSounds && Sounds.PainSound is not null)
				Sound.Play(Sounds.PainSound, Body.Transform.Position);
		}

		if (damageEventData.DamageResult <= 0)
			return;

		BroadcastDamageEffects(damageEventData.Position);

		if (EnableKnockBack)
			Body?.ApplyKnockBack(damageEventData);
	}
	
	private void BroadcastPostureBreakEffect(Vector3 position)
	{
		PostureBreakEffectPrefab?.Clone(position);

		Sound.Play("posture.break", Body.Transform.Position);
	}
	
	private void BroadcastDamageEffects(Vector3 position)
	{
		if (ImpactEffectPrefab is not null)
		{
			ImpactEffectPrefab.Clone(position);
		}

		if (EnableImpactSounds && Sounds.ImpactSound is not null)
			Sound.Play(Sounds.ImpactSound, position);
	}
	
	private void ApplyHitboxModifiers(DamageEventData damageEventData)
	{
		if (damageEventData.HitboxBoneIndex < 0 || HitboxDamageModifiers is null)
			return;

		HitboxSet.Box hitbox = null;
	
		if (Body.Model is not null)
		{
			foreach (var box in Body.Model.Model.HitboxSet.All)
			{
				if (box.Bone.Index == damageEventData.HitboxBoneIndex)
				{
					hitbox = box;
				}
			}
		}

		if (hitbox is null)
			return;
		
		foreach (var hitboxDamageModifier in HitboxDamageModifiers)
		{
			if (!hitbox.Tags.Has(hitboxDamageModifier.Tag))
				continue;

			damageEventData.DamageResult *= hitboxDamageModifier.Multiplier;

			//we might be able to guarantee crits later down the line or some such behaviour, so dont downgrade our crit status
			if (!damageEventData.IsCritical)
				damageEventData.IsCritical = hitboxDamageModifier.IsCritical;
		}
	}

	private void ApplyCritical(DamageEventData damageEventData)
	{
		if (damageEventData.Originator is null)
			return;

		var critMult = damageEventData.AdditiveCriticalMultiplier + damageEventData.Originator.Stats.CriticalMultiplier - Stats.CriticalNegation;
		critMult = MathF.Max(critMult, 1);

		damageEventData.DamageResult *= critMult;
	}

	//TODO: Proper nasty function. I should really try and figure out if there's a better way to handle this.

	/// <summary>
	/// Apply our resistances to a damage event.
	/// </summary>
	/// <param name="actorComponent"></param>
	/// <param name="damageEventData"></param>
	private void ApplyResistances(DamageEventData damageEventData)
	{
		var resist = float.PositiveInfinity;
		var validOriginator = damageEventData.Originator is not null;

		if (damageEventData.HasDamageType(DamageType.Physical))
		{
			resist = Stats.Armor - (validOriginator ? damageEventData.Originator.Stats.ArmorPenetration : 0); ;
		}

		var warding = Stats.Warding - (validOriginator ? damageEventData.Originator.Stats.WardingPenetration : 0);
		if (damageEventData.HasDamageType(DamageType.Magical) && warding < resist)
		{
			resist = warding;
		}

		var fireResist = Stats.FireResistance - (validOriginator ? damageEventData.Originator.Stats.FirePenetration : 0);
		if (damageEventData.HasDamageType(DamageType.Fire) && fireResist < resist)
		{
			resist = fireResist;
		}

		var frostResist = Stats.FrostResistance - (validOriginator ? damageEventData.Originator.Stats.FrostPenetration : 0);
		if (damageEventData.HasDamageType(DamageType.Frost) && frostResist < resist)
		{
			resist = frostResist;
		}

		var electricResist = Stats.ElectricResistance - (validOriginator ? damageEventData.Originator.Stats.ElectricPenetration : 0);
		if (damageEventData.HasDamageType(DamageType.Electric) && electricResist < resist)
		{
			resist = electricResist;
		}

		var poisonResist = Stats.PoisonResistance - (validOriginator ? damageEventData.Originator.Stats.PoisonPenetration : 0);
		if (damageEventData.HasDamageType(DamageType.Poison) && poisonResist < resist)
		{
			resist = poisonResist;
		}

		var necroticResist = Stats.NecroticResistance - (validOriginator ? damageEventData.Originator.Stats.NecroticPenetration : 0);
		if (damageEventData.HasDamageType(DamageType.Necrotic) && necroticResist < resist)
		{
			resist = necroticResist;
		}

		var arcaneResist = Stats.ArcaneResistance - (validOriginator ? damageEventData.Originator.Stats.ArcaneResistance : 0);
		if (damageEventData.HasDamageType(DamageType.Arcane) && Stats.ArcaneResistance < resist)
		{
			resist = arcaneResist;
		}

		var divineResist = Stats.DivineResistance - (validOriginator ? damageEventData.Originator.Stats.DivineResistance : 0);
		if (damageEventData.HasDamageType(DamageType.Divine) && divineResist < resist)
		{
			resist = divineResist;
		}

		var occultResist = Stats.OccultResistance - (validOriginator ? damageEventData.Originator.Stats.OccultResistance : 0);
		if (damageEventData.HasDamageType(DamageType.Occult) && occultResist < resist)
		{
			resist = occultResist;
		}

		//this is here cause if our resist is infinity, it just means it never got set.
		if (float.IsPositiveInfinity(resist))
		{
			resist = 0;
		}

		var resistMult = damageEventData.HasFlag(DamageFlags.IgnoreResistance)
			? 1
			: GameBalanceResource.ActiveBalance.ResistanceScalingCurve.Evaluate(resist);

		damageEventData.DamageResult -= damageEventData.DamageResult * resistMult;
		damageEventData.KnockBackResult -= Stats.KnockBackResistance;
	}
}
