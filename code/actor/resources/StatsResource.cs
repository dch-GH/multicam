using Sandbox;

using System;
using System.Text.Json.Serialization;

namespace Ironsim.Actor.StatSystem;

[GameResource("Stats", "stats", "Contains data about an actors base stats.")]
public class StatsResource : GameResource
{
	public Core Core { get; set; }

	public Health Health { get; set; }

	public Stamina Stamina { get; set; }

	public Posture Posture { get; set; }

	public Concentration Concentration { get; set; }

	public Actions Actions { get; set; }

	public Combat Combat { get; set; }

	public Resistances Resistances { get; set; }

	public Misc Misc { get; set; }

	[JsonIgnore]
	public Action OnReload;

	protected override void PostReload()
	{
		base.PostReload();
		OnReload?.Invoke();
	}
}
