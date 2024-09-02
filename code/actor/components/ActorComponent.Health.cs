using System;
using System.Linq;
using Ironsim.Actor.Marker;
using Ironsim.Actor.Damage;
using Sandbox;

namespace Ironsim.Actor;

public partial class ActorComponent
{
	[HostSync]
	public float Health { get; private set; }

	[HostSync]
	public bool Alive { get; private set; }

	[HostSync]
	private TimeSince TimeSinceDamaged { get; set; }

	public event Action<DamageEventData> DeathTrigger;
	public event Action ReviveTrigger;

	public void Heal(float amount)
	{
		Health += amount * Stats.HealMultiplier;
		Health = Health.Clamp(0, Stats.MaxHealth);
	}

	public void DamageHealth(float amount)
	{
		Health -= amount;
		Health = Health.Clamp(0, Stats.MaxHealth);

		TimeSinceDamaged = 0;
	}

	public void FullHeal()
	{
		Health = Stats.MaxHealth;
		Stamina = Stats.MaxStamina;
	}

	public void Kill()
	{
		/*if ( Ga is Player player )
			player.Respawn();
		else
			Entity.Delete();*/
	}

	private void OnConstitutionChanged(float oldCon, float newCon)
	{
		var oldMaxHealth = Stats.GetMaxHealthForConScore(oldCon);
		var frac = Health / oldMaxHealth;

		frac = frac.Clamp(0, 1);

		Health = Stats.MaxHealth * frac;

		var oldMaxStamina = Stats.GetMaxStaminaForConScore(oldCon);
		frac = Stamina / oldMaxStamina;

		frac = frac.Clamp(0, 1);

		Stamina = Stats.MaxStamina * frac;
	}

	private void OnMaxHealthChanged(float oldMaxHealthBonus, float newMaxHealthBonus)
	{
		var oldMaxHealth = Stats.GetMaxHealthForMaxHealthBonus(oldMaxHealthBonus);
		var frac = Health / oldMaxHealth;

		frac = frac.Clamp(0, 1);

		Health = Stats.MaxHealth * frac;
	}

	private void OnDeath(DamageEventData damageEventData)
	{
		if (ShouldRagdollOnDeath)
		{
			//Body?.EnableRagdoll();
		}

		foreach (var deathListener in Components.GetAll<IDeathListener>().ToList())
		{
			deathListener.OnDeath(damageEventData);
		}

		DeathTrigger?.Invoke(damageEventData);
	}

	public void Revive()
	{
		Health = Stats.MaxHealth;
		Stamina = Stats.MaxStamina;
		Concentration = Stats.MaxConcentration;
		Posture = Stats.MaxPosture;

		Alive = true;

		foreach (var reviveListener in Components.GetAll<IReviveListener>(FindMode.EverythingInSelfAndDescendants))
		{
			reviveListener.OnRevive();
		}

		ReviveTrigger?.Invoke();
	}
}
