using Ironsim.Actor.Movement;
using Ironsim.Actor.StateMachine;

using Sandbox;

namespace Ironsim.Actor;

/// <summary>
/// Controls how our lil actor should think, act and feel about life in general.
/// </summary>
public partial class BehaviourComponent : Component
{
	[ConVar]
	private static bool dd_debug_npc_state { get; set; } = false;

	public static class BehaviourAnimationEvents
	{
		public const string LockRotation = "LockRotation";
		public const string UnlockRotation = "UnlockRotation";
		public const string ActivateHitBoxes = "ActivateHitBoxes";
		public const string DeactivateHitBoxes = "DeactivateHitBoxes";
		public const string FinishAttack = "FinishAttack";
		public const string FinishCast = "FinishCast";
		public const string CastSpell = "CastSpell";
	}

	public enum StaggerDirection
	{
		Left,
		Right,
		Forward,
		Back
	}

	public bool IsStaggered { get; set; }
	public bool IsStunned { get; set; }
	public bool IsKnockedDown { get; set; }

	public CharacterController CharacterController { get; set; }

	public TargetingComponent TargetingComponent { get; set; }

	/// <summary>
	/// The actor this behaviour is controlling.
	/// </summary>
	[RequireComponent]
	protected ActorComponent Actor { get; set; }

	public MovementComponent Movement { get; set; }

	private float TickRate { get; set; } = 5f;

	private TimeUntil TimeUntilNextTick = 0;

	protected override void OnStart()
	{
		StaggerState = Components.Get<StaggerState>(true);
		StunState = Components.Get<StunState>(true);
		KnockdownState = Components.Get<KnockdownState>(true);
		DeadState = Components.Get<DeadState>(true);

		CharacterController = Components.Get<CharacterController>(true);
		Movement = Components.Get<MovementComponent>(true);
		TargetingComponent = Components.Get<TargetingComponent>(true);

		if (IsProxy)
			return;

		HookupAnimEvents();

		if (StartingState is not null)
			SetState(StartingState);
	}

	protected virtual void HookupAnimEvents()
	{
		if (Actor.Body?.Model is null)
			return;

		Actor.Body.Model.SceneModel.OnGenericEvent += OnGenericAnimEvent;
	}

	protected virtual void OnGenericAnimEvent(SceneModel.GenericEvent genericEvent)
	{
		ActiveState?.OnGenericAnimEvent(genericEvent);
	}

	protected override void OnUpdate()
	{
		Actor.Body.Set("fActionSpeed", Actor.Stats.ActionSpeed);
		Actor.Body.Set("fMoveSpeed", Actor.Stats.MoveSpeedMultiplier);

		if (dd_debug_npc_state)
		{
			using (Gizmo.Scope(GameObject.Id.ToString(), Transform.Local))
			{
				var transform = new Transform(Vector3.Up * 64);

				if (ActiveState is not null)
					Gizmo.Draw.Text(ActiveState.Name, transform);
			}
		}

		if (IsProxy)
			return;

		ActiveState?.Update();
		Movement?.Update();

		if (TimeUntilNextTick)
		{
			Tick();
			TimeUntilNextTick = 1 / TickRate;
		}
	}

	protected virtual void Tick()
	{
		ActiveState?.Tick();
	}

	[Broadcast(NetPermission.OwnerOnly)]
	public void BroadcastAnimation(string parameter, bool b)
	{
		Actor.Body.Set(parameter, b);
	}

	[Broadcast(NetPermission.OwnerOnly)]
	public void BroadcastAnimation(string parameter, float f)
	{
		Actor.Body.Set(parameter, f);
	}

	[Broadcast(NetPermission.OwnerOnly)]
	public void BroadcastAttack(int choice)
	{
		Actor.Body.Set("iAttackChoice", choice);
		Actor.Body.Set("bAttackStart", true);
	}
}
