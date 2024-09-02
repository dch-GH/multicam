using Ironsim.Actor.Movement;

using Sandbox;

namespace Ironsim.Actor.StateMachine;

public class ChaseState : StateComponent
{
	/// <summary>
	/// The state that we return to with no target.
	/// </summary>
	[Property]
	private StateComponent ReturnState { get; set; }

	[Property]
	private StateComponent AttackState { get; set; }

	[Property, Range(0, 300)]
	private float AttackRange { get; set; }

	private TargetingComponent TargetingComponent { get; set; }

	public override string Name => "Chase";

	protected override void OnAwake()
	{
		base.OnAwake();

		TargetingComponent = Components.Get<TargetingComponent>(true);
	}

	public override void OnEntered()
	{
		Behaviour.Movement.MovementMode = MovementComponent.MovementSource.Destination;
		Behaviour.Movement.RotationMode = MovementComponent.RotationSource.Movement;

		UpdateChaseState();
	}

	public override void OnExited()
	{
	}

	private void UpdateChaseState()
	{
		TargetingComponent.UpdateTargetFromDistance();

		if (!TargetingComponent.HasTarget)
		{
			if (ReturnState is not null)
				Behaviour.SetState(ReturnState);

			return;
		}

		Behaviour.Movement.NavMeshAgent.MoveTo(TargetingComponent.Target.Transform.Position);
		//Behaviour.Movement.Destination = TargetingComponent.Target.Transform.Position;

		if (TargetingComponent.DistanceToTarget < AttackRange && AttackState.CanEnter())
			Behaviour.SetState(AttackState);
	}

	public override void Tick()
	{
		UpdateChaseState();
	}
}
