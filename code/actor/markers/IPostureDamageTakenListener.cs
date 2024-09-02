using Sandbox;

namespace Ironsim.Actor.Marker;

public interface IPostureDamageTakenListener
{
	void OnPostureDamaged(float oldPosture, float newPosture, GameObject inflictor);
	void OnPostureBroken(GameObject inflictor);
}
