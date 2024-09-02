using Ironsim.Actor.Marker;
using Ironsim.Actor.Movement;

using Sandbox;

namespace Ironsim.Actor.StateMachine;

public class StunState : StateComponent
{
	[Property]
	private StateComponent ReturnState { get; set; }

	/// <summary>
	/// Causes the actor to exit the stun state as soon as the stun duration has ended, otherwise it will wait for the
	/// animation event.
	/// </summary>
	[Property]
	private bool ExitImmediatelyAfterStunDuration { get; set; }

	public override string Name => "Stunned";

	public override void OnEntered()
	{
		Behaviour.BroadcastAnimation("bStunned", true);
		Behaviour.IsStunned = true;

		if (Behaviour.Movement is null)
			return;

		Behaviour.Movement.MovementMode = MovementComponent.MovementSource.RootMotion;
		Behaviour.Movement.RotationMode = MovementComponent.RotationSource.RootMotion;
	}

	public override void OnExited()
	{
		Actor.ResetPosture();

		Behaviour.BroadcastAnimation("bStunned", false);
		Behaviour.IsStunned = false;

		foreach (var stunListener in Actor.Components.GetAll<IStunListener>())
		{
			stunListener.OnStunFinished();
		}
	}

	public override void Update()
	{
		if (TimeSinceEntered > 3f)
		{
			Behaviour.BroadcastAnimation("bStunned", false);

			if (ExitImmediatelyAfterStunDuration)
				Behaviour.SetState(ReturnState);
		}
	}

	public override void OnGenericAnimEvent(SceneModel.GenericEvent genericEvent)
	{
		switch (genericEvent.Type)
		{
			case "StunFinished":
				{
					Behaviour.SetState(ReturnState);
					break;
				}
		}
	}
}
