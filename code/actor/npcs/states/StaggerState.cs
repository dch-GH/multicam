using Ironsim.Actor.Movement;

using Sandbox;

namespace Ironsim.Actor.StateMachine;

public class StaggerState : StateComponent
{
	[Property]
	private StateComponent ReturnState { get; set; }

	public int Direction { get; set; }

	public override string Name => "Stagger";

	public override void OnEntered()
	{
		Behaviour.BroadcastStaggerAnimation((int)Behaviour.LastStaggerDirection);
		Behaviour.Movement.MovementMode = MovementComponent.MovementSource.RootMotion;
		Behaviour.Movement.RotationMode = MovementComponent.RotationSource.RootMotion;
		Behaviour.IsStaggered = true;
	}

	public override void OnExited()
	{
		Behaviour.IsStaggered = false;
	}

	public override void OnGenericAnimEvent(SceneModel.GenericEvent genericEvent)
	{
		switch (genericEvent.Type)
		{
			case "StaggerFinished":
				{
					Behaviour.SetState(ReturnState);
					break;
				}
		}
	}
}
