namespace Ironsim.Actor.Marker;

public interface IPostureDamageDealtListener
{
	/// <summary>
	/// Called when we damage an actors posture.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="amount"></param>
	void OnDamagedPosture(ActorComponent target, float amount, float excess);

	/// <summary>
	/// Called when we break an actors posture.
	/// </summary>
	/// <param name="target"></param>
	void OnBreakPosture(ActorComponent target);
}
