namespace Ironsim.Actor.Marker;

public interface IEquipListener
{
	public void OnEquipped(ActorComponent actorComponent);

	public void OnUnequipped(ActorComponent actorComponent);
}
