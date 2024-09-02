using Ironsim.Actor.Damage;

namespace Ironsim.Actor.Marker;

public interface IBlockListener
{
	void OnBlock(DamageEventData damageEvent, bool isParry);
}
