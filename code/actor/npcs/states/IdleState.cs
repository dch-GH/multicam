using Ironsim.Actor.Movement;
using Sandbox;

namespace Ironsim.Actor.StateMachine;

public class IdleState : StateComponent
{
	/// <summary>
	/// The state that we should go to when we successfully detect something.
	/// </summary>
	[Property]
	private StateComponent DetectionState { get; set; }


	public override string Name => "Idle";

	public override void OnEntered()
	{
		Behaviour.Movement.MovementMode = MovementComponent.MovementSource.None;
		Behaviour.Movement.RotationMode = MovementComponent.RotationSource.None;
	}

	private bool alertPlayed;
	private TimeSince timeSinceLastAlertSound;
	public override void OnExited()
	{
		base.OnExited();

		if (timeSinceLastAlertSound > 5f || !alertPlayed)
		{
			alertPlayed = true;
			Sound.Play(Actor.Sounds.AlertSound, Transform.Position);
			timeSinceLastAlertSound = 0;
		}
	}

	public override void Tick()
	{
		Behaviour.TargetingComponent.UpdateTargetFromDistance();

		if (Behaviour.TargetingComponent.HasTarget && DetectionState is not null)
			Behaviour.SetState(DetectionState);
	}
}
