using Ironsim.Actor.Damage;

using Sandbox;

namespace Ironsim.Actor.StateMachine;

public abstract class StateComponent : Component
{
	public virtual string Name => "Default!!";
	protected ActorComponent Actor { get; set; }
	protected BehaviourComponent Behaviour { get; set; }

	public TimeSince TimeSinceEntered { get; set; }
	public TimeSince TimeSinceExited { get; set; }

	protected override void OnAwake()
	{
		Behaviour = Components.Get<BehaviourComponent>(true);
		Actor = Components.Get<ActorComponent>(true);
	}

	public virtual void OnEntered() { }
	public virtual void OnExited() { }
	public virtual void Update() { }
	public virtual void Tick() { }
	public virtual void OnGenericAnimEvent(SceneModel.GenericEvent genericEvent) { }
	public virtual void OnProcessDamageTaken(DamageEventData damageEventData) { }
	public virtual void OnDamageTaken(DamageEventData damageEventData, bool isLethal) { }
	public virtual void OnDeath(DamageEventData damageEventData) { }

	public virtual bool CanEnter()
	{
		return true;
	}


}
