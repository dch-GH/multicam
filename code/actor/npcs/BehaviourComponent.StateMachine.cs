using Ironsim.Actor.StateMachine;

using Sandbox;

namespace Ironsim.Actor;

public partial class BehaviourComponent
{
	[Property, ToggleGroup("State Machine")]
	protected StateComponent StartingState { get; set; }
	protected StateComponent StaggerState { get; set; }
	protected StateComponent StunState { get; set; }
	protected StateComponent KnockdownState { get; set; }
	protected StateComponent DeadState { get; set; }

	protected StateComponent ActiveState { get; set; }

	public void SetState(StateComponent state)
	{
		if (ActiveState is not null)
		{
			ActiveState.OnExited();
			ActiveState.TimeSinceExited = 0;
			ActiveState.Enabled = false;
		}

		ActiveState = state;
		ActiveState.Enabled = true;
		ActiveState.OnEntered();
		ActiveState.TimeSinceEntered = 0;
	}
}
