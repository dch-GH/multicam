using Sandbox;

namespace Ironsim.Actor.Marker;

public interface IStunListener
{
	/// <summary>
	/// Called when we inflict a stun on a target.
	/// </summary>
	/// <param name="target"></param>
	public void OnStunInflicted(BehaviourComponent target);

	/// <summary>
	/// Called when we're stunned.
	/// </summary>
	public void OnStunned(GameObject inflictor);

	/// <summary>
	/// Called when our stun ends.
	/// </summary>
	public void OnStunFinished();
}
