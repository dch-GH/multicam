using Sandbox;

namespace Ironsim.Actor.StatSystem;

public struct Concentration
{
	public Concentration() { }

	/// <summary>
	/// Flat max concentration for this actor.
	/// Note that this is increased by the actors spell casting attribute.
	/// </summary>
	[Range(1, 100, 1f)]
	public float Max { get; set; } = 30;

	/// <summary>
	/// Concentration regeneration per second.
	/// </summary>
	[Range(0, 10)]
	public float Regen { get; set; } = 0;

	/// <summary>
	/// How long after taking concentration damage we wait before regenerating concentration.
	/// Reduced by Intelligence.
	/// </summary>
	[Range(0, 5)]
	public float RegenDelay { get; set; } = 2.5f;

	/// <summary>
	/// How long after taking concentration damage we wait before regenerating concentration.
	/// Reduced by Will.
	/// </summary>
	[Range(0, 2)]
	public float CostMultiplier { get; set; } = 1f;
}
