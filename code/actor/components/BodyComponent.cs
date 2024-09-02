using Ironsim.Actor.Damage;
using Sandbox;

namespace Ironsim.Actor;

public partial class BodyComponent : Component
{
	[Property]
	private bool HasRagdoll { get; set; } = true;
	
	[Property]
	private GameObject Collider { get; set; }

	public Rigidbody Rigidbody { get; set; }
	public SkinnedModelRenderer Model { get; set; }
	public ModelPhysics ModelPhysics { get; set; }
	public ModelHitboxes ModelHitboxes { get; set; }
	private ActorComponent Actor { get; set; }

	public bool IsRagdoll { get; private set; }

	protected override void OnAwake()
	{
		Actor = Components.GetInAncestorsOrSelf<ActorComponent>(true);
		Rigidbody = Components.Get<Rigidbody>(true);
		Model = Components.Get<SkinnedModelRenderer>(true);
		ModelPhysics = Components.Get<ModelPhysics>(true);
		ModelHitboxes = Components.Get<ModelHitboxes>(true);
	}

	protected override void OnEnabled()
	{
		if (Model is null)
			return;

		Model.OnFootstepEvent += OnFootStepEvent;
	}

	protected override void OnDisabled()
	{
		if (Model is null)
			return;

		Model.OnFootstepEvent -= OnFootStepEvent;
	}


	public void Set(string property, float value)
	{
		Model?.Set(property, value);
	}

	public void Set(string property, bool value)
	{
		Model?.Set(property, value);
	}

	public void Set(string property, int value)
	{
		Model?.Set(property, value);
	}

	public void Set(string property, Vector3 value)
	{
		Model?.Set(property, value);
	}

	public void ApplyKnockBack(DamageEventData damageEventData, float multiplier = 1f)
	{
		if (IsRagdoll)
		{
			ApplyKnockBackToRagdoll(damageEventData.Position, damageEventData.Direction, damageEventData.KnockBackResult, multiplier);
			return;
		}

		if (Rigidbody is null)
			return;

		ApplyKnockBack(Rigidbody.PhysicsBody, damageEventData.Position, damageEventData.Direction, damageEventData.KnockBackResult, multiplier);
	}

	[Broadcast]
	private void ApplyKnockBackToRagdoll(Vector3 position, Vector3 direction, float knockback, float multiplier)
	{
		var modelPhysics = ModelPhysics;
		if (modelPhysics?.PhysicsGroup != null)
		{
			PhysicsBody closestBody = null;
			var closestDistance = float.PositiveInfinity;
			foreach (var body in modelPhysics.PhysicsGroup.Bodies)
			{
				var distance = body.Position.Distance(position);
				if (distance < closestDistance)
				{
					closestBody = body;
					closestDistance = distance;
				}
			}

			if (closestBody is not null)
			{
				ApplyKnockBack(closestBody, position, direction, knockback, 1f);
				return;
			}
		}
	}

	private void ApplyKnockBack(PhysicsBody body, Vector3 position, Vector3 direction, float knockback, float multiplier = 1f)
	{
		if (!body.IsValid())
			return;

		var dir = direction;
		var baseMult = 2000f;

		var baseForce = dir * body.Mass * baseMult;
		var knockbackForce = dir * knockback * baseMult * 5f;
		var additiveForce = dir * multiplier;
		body.ApplyForceAt(body.FindClosestPoint(position),
			baseForce + knockbackForce + additiveForce);
	}

	[Broadcast]
	public void EnableRagdoll()
	{
		Collider.Enabled = false;
		
		if (!HasRagdoll)
			return;

		if (ModelPhysics is not null)
			ModelPhysics.Enabled = true;

		IsRagdoll = true;

		GameObject.Tags.Add("ragdoll");
	}

	[Broadcast]
	public void DisableRagdoll()
	{
		Collider.Enabled = true;
		
		if (!HasRagdoll)
			return;

		if (ModelPhysics is not null)
		{
			ModelPhysics.Enabled = false;
		}

		GameObject.Transform.LocalPosition = Vector3.Zero;

		IsRagdoll = false;

		GameObject.Tags.Remove("ragdoll");
	}

	public Vector3? GetRagdollPosition()
	{
		if (ModelPhysics is null || ModelPhysics.PhysicsGroup is null || !HasRagdoll)
			return null;

		return ModelPhysics.PhysicsGroup.Pos;
	}

	public void ProceduralHitReaction(int boneId, float damageScale = 1.0f, Vector3 force = default)
	{
		if (Model is null)
			return;

		var bone = Model.GetBoneObject(boneId);

		if (bone is null)
			return;

		var localToBone = bone.Transform.Local.Position;
		if (localToBone == Vector3.Zero) localToBone = Vector3.One;

		Model.Set("hit", true);
		Model.Set("hit_bone", boneId);
		Model.Set("hit_offset", localToBone);
		Model.Set("hit_direction", force.Normal);
		Model.Set("hit_strength", (force.Length / 1000.0f) * damageScale);
	}
}
