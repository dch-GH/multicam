using Sandbox;

using System.Collections.Generic;

namespace Ironsim.Actor.Damage;

public class HurtBoxComponent : Component
{
	[ConVar]
	private static bool debug_hurtboxes { get; set; }

	public Vector3 Direction
	{
		get
		{
			return DirectionMoment;
		}
	}

	private Vector3 DirectionMoment = Vector3.Zero;
	private Vector3 LastHitDirectionPosition = Vector3.Zero;

	public Capsule Capsule => new Capsule(Center1,
		Center2, Radius);

	[Property]
	private GameObject HitDirection { get; set; }

	[Property]
	private Vector3 Center1 { get; set; }

	[Property]
	private Vector3 Center2 { get; set; }

	[Property]
	private float Radius { get; set; }

	protected override void DrawGizmos()
	{
		if (!Scene.IsEditor)
			return;

		//Gizmo.Transform = Transform.Local;
		Gizmo.Draw.Color = Color.Green;

		Gizmo.Draw.LineCapsule(Capsule);
	}

	protected override void OnUpdate()
	{
		DirectionMoment = (HitDirection.Transform.Position - LastHitDirectionPosition).Normal;
		LastHitDirectionPosition = HitDirection.Transform.Position;
	}

	public IEnumerable<SceneTraceResult> PerformTrace(params GameObject[] ignoreTargets)
	{
		var trace = Scene.Trace.Ray(Transform.World.PointToWorld(Center1), Transform.World.PointToWorld(Center2))
			.WithoutTags("nohit", "debris")
			.UseHitboxes()
			.Radius(Radius);

		foreach (var ignoreTarget in ignoreTargets)
		{
			trace = trace.IgnoreGameObjectHierarchy(ignoreTarget);
		}

		if (debug_hurtboxes)
		{
			using (Gizmo.ObjectScope(this, Transform.Local))
			{
				var tr = trace.Run();

				if (tr.Hit)
					Gizmo.Draw.Color = Color.Red;
				else
					Gizmo.Draw.Color = Color.White;

				Gizmo.Draw.SolidCapsule(tr.StartPosition, tr.EndPosition, Radius, 8, 8);
			}
		}

		return trace.RunAll();
	}
}
