using Ironsim.Actor.Movement;

using Sandbox;

namespace Ironsim.Actor.StateMachine;

public class StandUpState : StateComponent
{
	[Property]
	private StateComponent ReturnState { get; set; }

	public override string Name => "Standing Up";

	public override void OnEntered()
	{
		Actor.Body.DisableRagdoll();

		Behaviour.BroadcastAnimation("bKnockedDown", false);

		Behaviour.Movement.MovementMode = MovementComponent.MovementSource.None;
		Behaviour.Movement.RotationMode = MovementComponent.RotationSource.None;

		Actor.Body.Transform.Rotation = Rotation.LookAt(Actor.Body.Transform.World.Forward.WithZ(0));
	}

	public override void OnGenericAnimEvent(SceneModel.GenericEvent genericEvent)
	{
		switch (genericEvent.Type)
		{
			case "KnockdownFinished":
				{
					Behaviour.SetState(ReturnState);
					break;
				}
		}
	}
}
