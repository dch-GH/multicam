namespace Ironsim.Actor;

public partial class ActorComponent
{
	public int Level { get; set; } = 1;

	public float Experience { get; set; } = 0;

	public float ExperienceRequirement { get; set; } = 100;

	/// <summary>
	/// If this is true, the actor will automatically spend stat points gained from its level ups.
	/// Experimental
	/// TODO: implement this i guess
	/// </summary>
	private bool AutoApplyLevelUps { get; set; }

	/// <summary>
	/// How many attribute points we have to spend.
	/// We get 2 per level.
	/// </summary>
	public int AttributePoints { get; set; } = 10;
}
