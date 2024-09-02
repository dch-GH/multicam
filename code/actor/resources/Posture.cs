using Sandbox;

namespace Ironsim.Actor.StatSystem;

public struct Posture
{
	public Posture() { }

	/// <summary>
	/// Flat max posture for this actor.
	/// Note that this is added onto by Willpower.
	/// </summary>
	[Range(1, 100, 1f)]
	public float Max { get; set; } = 30;

	/// <summary>
	/// Posture regeneration per second.
	/// </summary>
	[Range(0, 10)]
	public float Regen { get; set; } = 0;

	/// <summary>
	/// How long after taking damage we wait before regening posture.
	/// Reduced by Will.
	/// </summary>
	[Range(0, 5)]
	public float RegenDelay { get; set; } = 1.5f;
}
