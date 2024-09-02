using Ironsim.Actor.Movement;

using Sandbox;

namespace Ironsim.Actor.StateMachine;

public class DeadState : StateComponent
{
	[Property]
	private StateComponent ReturnState { get; set; }

	public override string Name => "Dead";

	public override void OnEntered()
	{
		Behaviour.BroadcastAnimation("bKnockedDown", true);
		Actor.Body.EnableRagdoll();

		Behaviour.Movement.MovementMode = MovementComponent.MovementSource.None;
		Behaviour.Movement.RotationMode = MovementComponent.RotationSource.None;
	}

	public override void Update()
	{
		if (Actor.Alive)
		{
			Behaviour.SetState(ReturnState);
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
