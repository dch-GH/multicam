using Sandbox;

namespace Ironsim.Actor.Damage;

public struct HitboxDamageModifier
{
	public string Tag { get; set; }

	[Range(0, 5, 0.25f)]
	public float Multiplier { get; set; }
	public bool IsCritical { get; set; }
	public bool CauseHitBounce { get; set; }
}
