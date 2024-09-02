using Ironsim.Actor.Marker;

using Sandbox;

using System;

namespace Ironsim.Actor;

public partial class ActorComponent
{

	[HostSync]
	[Sync]
	public float Posture { get; private set; }

	public TimeSince TimeSincePostureDamaged { get; set; }
	public TimeSince TimeSincePostureBroken { get; set; }

	public bool PostureBroken { get; set; }

	public void RecoverPosture(float amount)
	{
		Posture += amount;
		Posture = Posture.Clamp(0, Stats.MaxPosture);
	}

	public void DamagePosture(float amount, GameObject inflictor)
	{
		var oldPosture = Posture;
		Posture -= amount;
		Posture = Posture.Clamp(0, Stats.MaxPosture);

		TimeSincePostureDamaged = 0;

		foreach (var postureListener in Components.GetAll<IPostureDamageTakenListener>())
		{
			postureListener.OnPostureDamaged(oldPosture, Posture, inflictor);
		}

		foreach (var postureDamageDealtListener in inflictor.Components.GetAll<IPostureDamageDealtListener>())
		{
			var excess = MathF.Max(amount - oldPosture, 0);

			postureDamageDealtListener.OnDamagedPosture(this, amount, excess);
		}
	}

	public void BreakPosture(GameObject inflictor)
	{
		PostureBroken = true;
		TimeSincePostureBroken = 0;

		foreach (var postureDamageTakenListener in Components.GetAll<IPostureDamageTakenListener>())
		{
			postureDamageTakenListener.OnPostureBroken(inflictor);
		}

		foreach (var postureDamageDealtListener in inflictor.Components.GetAll<IPostureDamageDealtListener>())
		{
			postureDamageDealtListener.OnBreakPosture(this);
		}
	}

	public void ResetPosture()
	{
		PostureBroken = false;
		Posture = Stats.MaxPosture;
	}
}
