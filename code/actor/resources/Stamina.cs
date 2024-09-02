using Sandbox;

namespace Ironsim.Actor.StatSystem;

public struct Stamina
{
	public Stamina() { }

	/// <summary>
	/// Flat max stamina that this actor has.
	/// Note that this is added onto by Constitution.
	/// </summary>
	[Range(1, 100f, 1f)]
	public float Max { get; set; } = 30f;

	/// <summary>
	/// Flat stamina regen per second.
	/// Note that this is added onto by Dexterity.
	/// </summary>
	[Range(1, 50f)]
	public float Regen { get; set; } = 3f;

	/// <summary>
	/// How long in seconds after paying a stamina cost until we start regening.
	/// Note that this is decreased by Dexterity.
	/// </summary>
	[Range(0, 5)]
	public float RegenDelay { get; set; } = 1.5f;

	/// <summary>
	/// Multiplier to our stamina costs.
	/// Note that this is lowered by Strength.
	/// </summary>
	[Range(0, 3)]
	public float CostMultiplier { get; set; } = 1f;
}
