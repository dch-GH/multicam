using Ironsim.Actor.Damage;

namespace Ironsim.Actor.Marker;

public interface IDamageDealtListener
{
	/// <summary>
	/// Called when processing the damage dealt to a target, in case we want to modify it.
	/// </summary>
	/// <param name="damageEventData"></param>
	void OnProcessDamageDealt(DamageEventData damageEventData);

	/// <summary>
	/// Called when damage is actually dealt to a target. It shouldn't be modified in here.
	/// </summary>
	/// <param name="damageEventData"></param>
	/// <param name="isLethal"></param>
	void OnDamageDealt(DamageEventData damageEventData, bool isLethal);
}
