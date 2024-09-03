using Ironsim.Actor.Movement;
using Sandbox.States;

namespace Ironsim.AI;

[Title( "Guard" )]
[Category( "Ironsim/AI" )]
public sealed class GuardComponent : Component
{
	[Property] private PatrolComponent? _patrol;

	private StateMachineComponent _stateMachine;
	[RequireComponent] private MovementComponent _mover { get; set; }

	protected override void OnFixedUpdate()
	{
		if ( _patrol is not null && _patrol.ShouldPatrol && _patrol.HasPatrolPath )
		{
		}
	}
}
