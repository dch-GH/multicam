using Ironsim.Actor.Damage;

namespace Ironsim.Actor.Marker;

public interface IDeathListener
{
	void OnDeath(DamageEventData damageEventData);
}
