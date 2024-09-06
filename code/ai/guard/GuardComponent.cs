using Sandbox.States;

namespace Ironsim.AI;

[Title( "Guard" )]
[Category( "Ironsim/AI" )]
public sealed class GuardComponent : Component
{
	[Property] private PatrolPathComponent? _patrol;
	[Property] public bool ShouldPatrol { get; set; }
	[Property, Range( 0, 500 )] public float MoveSpeed { get; set; }
	public bool HasPatrolPath => !_patrol.IsEmpty;

	/// <summary>
	/// Have we reached the end and are we now traversing backwards to the start? 
	/// Only applies to non-looping patrol paths.
	/// </summary>
	/// <value></value>
	public bool IsReversing { get; private set; }

	private StateMachineComponent _stateMachine;
	private NavMeshAgent _agent;
	private CharacterController CharacterController;
	private Vector3 WishVelocity;
	public PatrolNodeComponent CurrentNode;
	public GameObject? Target;

	protected override void OnStart()
	{
		if ( _patrol is not null )
			CurrentNode = _patrol.Nodes.First();

		Components.TryGet<NavMeshAgent>( out _agent );
		Components.TryGet<CharacterController>( out CharacterController );
	}

	protected override void OnFixedUpdate()
	{
		if ( _patrol is not null && ShouldPatrol && HasPatrolPath )
		{
			if ( Transform.Position.AlmostEqual( CurrentNode.Transform.Position, 32 ) )
			{
				CurrentNode = _patrol.Next( CurrentNode );
			}
		}

		WishVelocity = (CurrentNode.Transform.Position - Transform.Position).Normal * MoveSpeed;

		if ( CharacterController.IsOnGround )
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
			CharacterController.Accelerate( WishVelocity );
			CharacterController.ApplyFriction( 3.0f );
		}

		if ( !CharacterController.IsOnGround )
		{
			CharacterController.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
		}
		else
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
		}

		CharacterController.Move();
	}
}
