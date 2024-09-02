using Sandbox;

namespace Ironsim.Actor.Movement;

/// <summary>
/// NPCs are assigned a movement component, and this handles all their locomotion and traversal.
/// The idea being that it is abstracted - you simply tell it where to go and how fast and it handles the rest.
/// </summary>
public class MovementComponent : Component
{
	public enum MovementSource
	{
		None,
		Destination,
		RootMotion
	}

	public enum RotationSource
	{
		None,
		Movement,
		Target,
		RootMotion
	}

	[RequireComponent]
	public NavMeshAgent NavMeshAgent { get; set; }

	[RequireComponent]
	private BehaviourComponent Behaviour { get; set; }

	[RequireComponent]
	private ActorComponent Actor { get; set; }

	[RequireComponent]
	private CharacterController CharacterController { get; set; }

	private bool RotationLocked { get; set; }

	private Vector3 WishVelocity = Vector3.Zero;
	private Rotation WishRotation = Rotation.Identity;

	public Vector3 Destination { get; set; } = Vector3.Zero;

	public MovementSource MovementMode { get; set; } = MovementSource.None;
	public RotationSource RotationMode { get; set; } = RotationSource.None;

	public Vector3 ExternalVelocity { get; set; } = Vector3.Zero;

	public void Update()
	{
		switch (MovementMode)
		{
			case MovementSource.None:
				{
					WishVelocity = ExternalVelocity;
					break;
				}

			case MovementSource.Destination:
				{
					WishVelocity = NavMeshAgent.WishVelocity + ExternalVelocity;
					break;
				}

			case MovementSource.RootMotion:
				{
					if (Actor.Body.Model is not null)
					{
						var rootMotion = Actor.Body.Model.RootMotion;
						rootMotion = rootMotion.RotateAround(Vector3.Zero, Actor.Body.Model.Transform.Rotation);
						WishVelocity = rootMotion.Position / Time.Delta;
						CharacterController.Velocity = WishVelocity + ExternalVelocity;
						break;
					}
					//WishVelocity = 0;
					break;
				}
		}

		switch (RotationMode)
		{
			case RotationSource.None:
				{
					break;
				}
			case RotationSource.Movement:
				{
					FaceDirection(WishVelocity);
					break;
				}
			case RotationSource.Target:
				{
					FaceTarget();
					break;
				}
			case RotationSource.RootMotion:
				{
					break;
				}
		}

		ExternalVelocity = ExternalVelocity.LerpTo(0, Time.Delta * 3f);

		if (CharacterController.IsOnGround)
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ(0);
			CharacterController.Accelerate(WishVelocity);
			CharacterController.ApplyFriction(3.0f);
		}

		if (!CharacterController.IsOnGround)
		{
			CharacterController.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
		}
		else
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ(0);
		}

		CharacterController.Move();

		Behaviour.BroadcastAnimation("bMoving", CharacterController.Velocity.Length > 1);
	}

	[Broadcast]
	private void FaceDirection(Vector3 direction)
	{
		if (RotationLocked || direction.IsNearZeroLength)
			return;

		direction = direction.WithZ(0).Normal;

		Actor.Body.Transform.Rotation = Rotation.Lerp(Actor.Body.Transform.Rotation, Rotation.LookAt(direction), Time.Delta * 8f);
	}

	private void FaceTarget()
	{
		if (!Behaviour.TargetingComponent.HasTarget)
			return;

		var dir = Behaviour.TargetingComponent.DirectionToTarget;

		FaceDirection(dir);
	}

	private void MoveToDestination()
	{
		var dir = Destination - Transform.Position;
		dir = dir.WithZ(0).Normal;

		WishVelocity = dir * 70f * Actor.Stats.MoveSpeedMultiplier;

		Actor.Body.Set("bMoving", true);
	}
}
