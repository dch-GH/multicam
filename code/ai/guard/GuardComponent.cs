using Sandbox.States;

namespace Ironsim.AI;

[Title( "Guard" )]
[Category( "Ironsim/AI" )]
public sealed class GuardComponent : Component
{
	[Property] private PatrolComponent? _patrol;
	private StateMachineComponent _stateMachine;
	private NavMeshAgent _agent;
	private PatrolNodeComponent _next;

	protected override void OnFixedUpdate()
	{
		if ( _patrol is not null && _patrol.ShouldPatrol && _patrol.HasPatrolPath )
		{
			_agent.MoveTo( _next.Position );
		}
	}
}
