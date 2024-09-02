using Ironsim.Actor.Damage;

namespace Ironsim.Actor.Marker;

/// <summary>
/// Called when we are executed by a player.
/// </summary>
public interface IExecutedListener
{
	/// <summary>
	/// Called when we are executed by a player.
	/// </summary>
	/// <param name="playerController">The player who executed us.</param>
	/// <param name="damageEventData">The damage that triggered the execute. This has already got the execute multiplier applied to it.</param>
	public void OnExecuted(PlayerController playerController, DamageEventData damageEventData);
}
