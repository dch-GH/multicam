using Sandbox;

using System;
using Ironsim.Actor.StatSystem;

namespace Ironsim.Actor;

/// <summary>
/// An Actor is anything in the game that the various NPCs and their systems should interact with.
/// i.e. all npcs are Actors, the player is an Actor, etc
/// </summary>
public partial class ActorComponent : Component
{
	[Property, ToggleGroup("Attributes")]
	public StatsResource BaseStats { get; set; }

	//public StatusEffectManagerComponent StatusEffectsManager { get; set; }

	public Stats Stats { get; private set; }

	protected override void OnAwake()
	{
		Body = Components.Get<BodyComponent>(FindMode.EverythingInSelfAndDescendants);
		//StatusEffectsManager = Components.Get<StatusEffectManagerComponent>(FindMode.EverythingInSelfAndDescendants);
		//ClassManager = Components.Get<ClassManagerComponent>(FindMode.EverythingInSelfAndDescendants);
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		Stats = new Stats(this);

		BaseStats.OnReload += ReloadBaseStats;
		InitializeStats();

		Alive = true;

		Blackboard.Register(this);
	}


	protected override void OnDisabled()
	{
		base.OnDisabled();

		BaseStats.OnReload -= ReloadBaseStats;

		Blackboard.UnRegister(this);
	}

	protected override void OnDestroy()
	{
		Blackboard.UnRegister(this);
	}

	protected override void OnUpdate()
	{
		if (!IsProxy)
			UpdateStats();
	}
}
