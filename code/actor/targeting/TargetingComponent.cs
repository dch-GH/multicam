using Ironsim.Actor.Targeting;
using Sandbox;

namespace Ironsim.Actor;

public class TargetingComponent : Component
{
	/// <summary>
	/// The factions this targeting component targets.
	/// </summary>
	[Property]
	public Faction EnemyFactions { get; set; }

	[Property, Range(0, 1500)]
	public float DetectionRange { get; set; }

	/// <summary>
	/// The currently set target for this component.
	/// Can be nulL!
	/// </summary>
	public TargetComponent Target { get; set; }

	/// <summary>
	/// Target component on this GameObject. Can be null!
	/// </summary>
	private TargetComponent TargetComponent { get; set; }
	private ActorComponent ActorComponent { get; set; }

	public bool HasTarget => Target is not null && Target.GameObject.IsValid();
	public float DistanceToTarget => Target is null ? 0 : Target.Transform.Position.Distance(Transform.Position);
	public Vector3 DirectionToTarget => Target is null ? Vector3.Zero : (Target.Transform.Position - Transform.Position).Normal;

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if (!Scene.IsEditor || !Gizmo.IsSelected)
			return;

		using var scope = Gizmo.Scope($"{GetHashCode()}");

		Gizmo.Transform = Scene.Transform.World;
		Gizmo.Draw.Color = Color.Green.WithAlpha(0.5f);

		Gizmo.Draw.LineSphere(Transform.Position, DetectionRange);
	}

	protected override void OnStart()
	{
		ActorComponent = Components.Get<ActorComponent>();
		TargetComponent = Components.Get<TargetComponent>();

		if (ActorComponent is null)
		{
			Log.Error($"No Actor Component on {GameObject.Name} for Targeting Component!");
		}
	}

	private bool CanTarget(TargetComponent targetComponent)
	{
		if (targetComponent.GameObject == GameObject)
			return false;

		if ((EnemyFactions & targetComponent.Factions) != Faction.None)
			return true;

		return false;
	}

	private bool CanSee(TargetComponent targetComponent)
	{
		var tr = Scene.Trace.Ray(Transform.Position, targetComponent.PrimaryTargetPoint.Transform.Position)
			.IgnoreGameObjectHierarchy(GameObject)
			.IgnoreGameObjectHierarchy(targetComponent.GameObject)
			.UseHitboxes(false)
			.WithoutTags("prop", "actor")
			.Run();

		return !tr.Hit;
	}

	public void UpdateTargetFromDistance()
	{
		var minDistance = float.PositiveInfinity;
		TargetComponent closestTarget = null;
		foreach (var target in Blackboard.Targets)
		{
			if (!CanTarget(target))
				continue;

			if (!CanSee(target))
				continue;

			var distance = target.Transform.Position.Distance(Transform.Position);
			if (distance > DetectionRange || distance > minDistance)
				continue;

			closestTarget = target;
			minDistance = distance;
		}

		Target = closestTarget;
	}
}
