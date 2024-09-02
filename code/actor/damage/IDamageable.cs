using Sandbox;

namespace Ironsim.Actor.Damage;

public interface IDamageable
{
	public GameObject GameObject { get; }
	public GameTransform Transform { get; }
	public bool CauseHitBounce { get; set; }
	public bool PlayHitSound { get; set; }
	public void TakeDamage(DamageEventData data);
}
