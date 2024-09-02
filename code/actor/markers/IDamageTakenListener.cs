using Ironsim.Actor.Damage;

namespace Ironsim.Actor.Marker;

public interface IDamageTakenListener
{
	/// <summary>
	/// Called when processing damage taken, in case we want to modify it.
	/// </summary>
	/// <param name="damageEventData"></param>
	void OnProcessDamageTaken(DamageEventData damageEventData);

	/// <summary>
	/// Called when we actually take damage. We shouldn't modify it in here.
	/// </summary>
	/// <param name="damageEventData"></param>
	/// <param name="isLethal"></param>
	void OnDamageTaken(DamageEventData damageEventData, bool isLethal);
}
