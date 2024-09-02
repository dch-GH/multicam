using Ironsim.Actor.Damage;
using Ironsim.Actor.Marker;

using Sandbox;

using System;

namespace Ironsim.Actor;

public partial class BehaviourComponent : IDamageTakenListener, IDeathListener, IPostureDamageTakenListener, IReviveListener
{
	public StaggerDirection LastStaggerDirection { get; set; }

	private TimeSince TimeSinceLastStagger = 1000;
	public virtual void OnDamageTaken(DamageEventData damageEventData, bool isLethal)
	{
		ActiveState?.OnDamageTaken(damageEventData, isLethal);

		if (isLethal || Actor.TimeSincePostureBroken == 0)
			return;

		ApplyKnockBack(damageEventData);

		//Actor.DamagePosture( damageEventData.DamageResult/2f );

		/*if ( damageEventData.Hitbox.Tags.Has( "leg" ) )
		{
			Knockdown( damageEventData );
		}*/

		if (CanKnockdown())
		{
			if (damageEventData.Originator.Stats.Strength >= 24)
			{
				Knockdown(damageEventData);
				return;
			}
		}

		if ((Actor.Posture < Actor.Stats.MaxPosture / 2f || damageEventData.IsCritical) && TimeSinceLastStagger > 3f || IsStunned)
		{
			if (CanStagger())
			{
				Stagger(damageEventData);
				TimeSinceLastStagger = 0;
				return;
			}
		}
	}

	public virtual void OnProcessDamageTaken(DamageEventData damageEventData)
	{
		ActiveState?.OnProcessDamageTaken(damageEventData);
	}

	public virtual void OnDeath(DamageEventData damageEventData)
	{
		ActiveState?.OnDeath(damageEventData);

		if (CharacterController is not null)
			CharacterController.Velocity = 0f;

		if (Movement is not null)
			Movement.NavMeshAgent.Enabled = false;

		if (DeadState is not null)
			SetState(DeadState);

		Enabled = false;
	}

	public void OnRevive()
	{
		Enabled = true;

		if (Movement is not null)
			Movement.NavMeshAgent.Enabled = false;

		if (StartingState is not null)
			SetState(StartingState);
	}

	private void ApplyKnockBack(DamageEventData damageEventData)
	{
		var knockback = damageEventData.Direction * damageEventData.KnockBackResult * 3f;

		if (Movement is not null)
			Movement.ExternalVelocity += knockback;
	}

	private StaggerDirection GetStaggerDirection(DamageEventData damageEventData)
	{
		var horizontalImpact = damageEventData.Direction.Dot(Actor.Body.Transform.Rotation.Left);
		var forwardImpact = damageEventData.Direction.Dot(Actor.Body.Transform.Rotation.Forward);

		if (MathF.Abs(horizontalImpact) > MathF.Abs(forwardImpact))
		{
			return horizontalImpact > 0 ? StaggerDirection.Left : StaggerDirection.Right;
		}
		else
		{
			return forwardImpact > 0 ? StaggerDirection.Forward : StaggerDirection.Back;
		}
	}

	private bool CanStagger()
	{
		if (!Actor.Alive || StaggerState is null)
			return false;

		return (!IsStaggered && !IsKnockedDown) || IsStunned;
	}

	private void Stagger(DamageEventData damageEventData)
	{
		Stagger(GetStaggerDirection(damageEventData));
	}

	private void Stagger(StaggerDirection direction)
	{
		LastStaggerDirection = direction;
		SetState(StaggerState);
	}

	private bool CanStun()
	{
		return Actor.Alive && !IsStunned && !IsKnockedDown;
	}

	private void Stun(GameObject inflcitor)
	{
		SetState(StunState);

		foreach (var stunListener in Components.GetAll<IStunListener>())
		{
			stunListener.OnStunned(inflcitor);
		}
	}

	private bool CanKnockdown()
	{
		return Actor.Alive && IsStunned;
	}

	private void Knockdown(DamageEventData damageEventData)
	{
		SetState(KnockdownState);
	}

	public void OnPostureDamaged(float oldPosture, float newPosture, GameObject inflictor)
	{
		if (newPosture > 0)
			return;

		if (CanStagger())
		{
			Stagger(StaggerDirection.Back);
			return;
		}
	}

	public void OnPostureBroken(GameObject inflictor)
	{
		if (CanStun())
		{
			Stun(inflictor);
			return;
		}
	}

	[Broadcast(NetPermission.OwnerOnly)]
	public void BroadcastStaggerAnimation(int direction)
	{
		Actor.Body.Set("eStaggerDirection", direction);
		Actor.Body.Set("bStagger", true);
	}
}
