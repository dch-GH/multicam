using Ironsim.Actor.Movement;

using Sandbox;

namespace Ironsim.Actor.StateMachine;

public class KnockdownState : StateComponent
{
	[Property]
	private StateComponent ReturnState { get; set; }

	[Property]
	private StateComponent StandUpState { get; set; }

	public override string Name => "Knockdown";

	public override void OnEntered()
	{
		Behaviour.BroadcastAnimation("bKnockedDown", true);
		Actor.Body.EnableRagdoll();

		Behaviour.IsKnockedDown = true;
		Behaviour.Movement.MovementMode = MovementComponent.MovementSource.None;
		Behaviour.Movement.RotationMode = MovementComponent.RotationSource.None;
	}

	public override void OnExited()
	{
		Behaviour.IsKnockedDown = false;
	}

	public override void Update()
	{
		if (TimeSinceEntered > 3f)
		{
			Behaviour.SetState(StandUpState);
			return;
		}

		if (!Actor.Body.IsRagdoll)
			return;

		var ragdollPos = Actor.Body.GetRagdollPosition();
		if (ragdollPos is not null)
		{
			GameObject.Transform.Position = ragdollPos.Value;
		}
	}
}
