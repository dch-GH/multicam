﻿namespace Ironsim;

/// <summary>
/// Data struct for damage after it has been processed by an actor component.
/// Can be networked to the client.
/// </summary>
public class DamageEventData
{
	/// <summary>
	/// The final damage actually received, post modifiers.
	/// </summary>
	public float DamageResult { get; set; }

	/// <summary>
	/// The original damage value dealt before modifiers.
	/// </summary>
	public float DamageOriginal { get; set; }

	public float PostureDamageResult { get; set; }
	public float PostureDamageOriginal { get; set; }

	/// <summary>
	/// Was this damage done during a successful block?
	/// </summary>
	public bool WasBlocked { get; set; }

	/// <summary>
	/// Was this damage done during a successful parry?
	/// </summary>
	public bool WasParried { get; set; }

	/// <summary>
	/// The strength of the knockback this damage stat should have.
	/// </summary>
	public float KnockBackOriginal { get; set; }

	/// <summary>
	/// Knockback after applying resistances.
	/// </summary>
	public float KnockBackResult { get; set; }

	/// <summary>
	/// Whether or not this damage was from a critical hit.
	/// </summary>
	public bool IsCritical { get; set; }

	/// <summary>
	/// Some damage sources will have their own critical multiplier.
	/// This is additive with the inflictors critical multiplier, so a value of 0 will not change the damage.
	/// </summary>
	public float AdditiveCriticalMultiplier { get; set; }

	/// <summary>
	/// This is the swing direction for our attack, as used by the weapon system.
	/// Used to determine whether we did an execution.
	/// </summary>
	public Vector2 AttackVector { get; set; }

	/// <summary>
	/// Where the damage was dealt in world space.
	/// Used for damage number spawning.
	/// </summary>
	public Vector3 Position { get; set; }

	/// <summary>
	/// The direction this damage was dealt in.
	/// </summary>
	public Vector3 Direction { get; set; }

	/// <summary>
	/// The various types this damage is.
	/// </summary>
	public DamageType DamageTypes { get; set; }

	/// <summary>
	/// The various flags this damage has assigned to it.
	/// </summary>
	public DamageFlags DamageFlags { get; set; }

	public TagSet Tags { get; set; }

	/// <summary>
	/// Who this damage event was originated by.
	/// </summary>
	public GameObject Originator { get; set; }

	/// <summary>
	/// What dealt this damage. Usually something like a weapon or spell.
	/// </summary>
	public GameObject Inflictor { get; set; }

	/// <summary>
	/// Target
	/// </summary>
	public IDamageable Target { get; set; }

	public Surface Surface { get; set; }

	public int HitboxBoneIndex { get; set; } = -1;

	public bool HasDamageType( DamageType damageType )
	{
		return DamageTypes.HasFlag( damageType );
	}

	public DamageEventData WithPosition( Vector3 position )
	{
		Position = position;
		return this;
	}

	public DamageEventData WithDirection( Vector3 direction )
	{
		Direction = direction;
		return this;
	}

	public DamageEventData WithDamage( float damage )
	{
		DamageOriginal = damage;
		DamageResult = damage;
		return this;
	}

	public DamageEventData WithPostureDamage( float damage )
	{
		PostureDamageOriginal = damage;
		PostureDamageResult = damage;
		return this;
	}

	public DamageEventData WithFlag( DamageFlags flags )
	{
		DamageFlags |= flags;
		return this;
	}

	public DamageEventData WithKnockBack( float knockback )
	{
		KnockBackOriginal = knockback;
		KnockBackResult = knockback;
		return this;
	}

	public DamageEventData AsCritical( bool isCrit )
	{
		IsCritical = isCrit;
		return this;
	}

	public DamageEventData WithAdditiveCriticalMultiplier( float additiveMult )
	{
		AdditiveCriticalMultiplier = additiveMult;
		return this;
	}

	public DamageEventData WithType( DamageType damageType )
	{
		DamageTypes |= damageType;
		return this;
	}

	public DamageEventData WithTags( params string[] tags )
	{
		Tags = new TagSet();
		foreach ( var tag in tags )
		{
			Tags.Add( tag );
		}
		return this;
	}

	public DamageEventData WithTarget( IDamageable target )
	{
		Target = target;
		return this;
	}

	public DamageEventData WithSurface( Surface surface )
	{
		Surface = surface;
		return this;
	}

	public DamageEventData WithOriginator( GameObject originator )
	{
		Originator = originator;
		return this;
	}

	public DamageEventData WithInflictor( GameObject inflictor )
	{
		Inflictor = inflictor;
		return this;
	}

	public DamageEventData WithAttackVector( Vector2 vector )
	{
		AttackVector = vector;
		return this;
	}

	public DamageEventData WithFlags( params DamageFlags[] flags )
	{
		foreach ( var flag in flags )
		{
			DamageFlags |= flag;
		}

		return this;
	}

	public DamageEventData WithHitbox( Hitbox hitbox )
	{
		HitboxBoneIndex = hitbox.Bone.Index;
		return this;
	}

	public DamageEventData WithHitbox( int boneIndex )
	{
		HitboxBoneIndex = boneIndex;
		return this;
	}

	public bool HasFlag( DamageFlags flags )
	{
		return DamageFlags.HasFlag( flags );
	}

	public DamageEventData UsingTraceResult( SceneTraceResult traceResult )
	{
		Position = traceResult.HitPosition;
		Direction = -traceResult.Normal;
		Surface = traceResult.Surface;
		HitboxBoneIndex = traceResult.Hitbox?.Bone?.Index ?? -1;
		return this;
	}

	public static void CreateDamageEffects( Surface surface, Vector3 position, Vector3 direction )
	{
		Sound.Play( surface.Sounds.ImpactHard, position );

		if ( surface.ImpactEffects.Regular is null || surface.ImpactEffects.Regular.Count == 0 )
			return;

		var dir = -direction.Normal;
		var angles = (Rotation.LookAt( dir ) * Rotation.FromPitch( 90 )).Angles();

		/*var particle = QuickParticle.Create( surface.ImpactEffects.Regular.FirstOrDefault(), Position );
		particle.Set( "Normal", dir );
		particle.Set("RingPitch", angles.pitch  );
		particle.Set("RingYaw", angles.yaw  );
		particle.Set("RingRoll", angles.roll  );*/
	}

	public void CreateScrapeEffect()
	{
		var surface = Surface;
		if ( surface is null )
			return;

		Sound.Play( surface.Sounds.ImpactHard, Position );

		if ( surface.ImpactEffects.Regular is null || surface.ImpactEffects.Regular.Count == 0 )
			return;

		var dir = -Direction.Normal;
		var angles = (Rotation.LookAt( dir ) * Rotation.FromPitch( 90 )).Angles();

		/*var particle = QuickParticle.Create( surface.ImpactEffects.Regular.FirstOrDefault(), Position );
		particle.Set( "Normal", dir );
		particle.Set("RingPitch", angles.pitch  );
		particle.Set("RingYaw", angles.yaw  );
		particle.Set("RingRoll", angles.roll  );*/
	}
}
