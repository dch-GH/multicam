using Sandbox.States;

namespace Ironsim.AI;

[Title( "Guard" )]
[Category( "Ironsim/AI" )]
public sealed class GuardComponent : Component
{
	[Property] private PatrolPathComponent? _patrol;
	public bool ShouldPatrol { get; set; }
	public bool HasPatrolPath => !_patrol.IsEmpty;

	/// <summary>
	/// Have we reached the end and are we now traversing backwards to the start? 
	/// </summary>
	/// <value></value>
	public bool IsReversing { get; private set; }

	private StateMachineComponent _stateMachine;
	private NavMeshAgent _agent;
	public PatrolNodeComponent Current;

	protected override void OnStart()
	{
		if ( _patrol is not null )
			Current = _patrol.Nodes.First();

		Components.TryGet<NavMeshAgent>( out _agent );
	}

	protected override void OnFixedUpdate()
	{
		if ( _patrol is not null && ShouldPatrol && HasPatrolPath )
		{
			if ( Transform.Position.AlmostEqual( Current.Transform.Position, 16 ) )
			{
				Current.Visited = true;
				Current = _patrol.Next( Current );
			}
		}

		_agent.MoveTo( Current.Transform.Position );
	}
}
