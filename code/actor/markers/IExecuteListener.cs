using Ironsim.Actor.Damage;

namespace Ironsim.Actor.Marker;

/// <summary>
/// Called when we execute an actor.
/// </summary>
public interface IExecuteListener
{
	/// <summary>
	/// Called when we execute something.
	/// </summary>
	/// <param name="actor">The actor we executed.</param>
	/// <param name="damageEventData">The damage that triggered the execute. This has already got the execute multiplier applied to it.</param>
	public void OnExecute(ActorComponent actor, DamageEventData damageEventData);
}
