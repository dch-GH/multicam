namespace Ironsim.AI;

public sealed class PatrolComponent : Component
{
	[Property] public PatrolPathComponent PatrolPath { get; private set; }
	public bool ShouldPatrol { get; set; }
	public bool HasPatrolPath => !PatrolPath.IsEmpty;

	/// <summary>
	/// Have we reached the end and are we now traversing backwards to the start? 
	/// </summary>
	/// <value></value>
	public bool IsReversing { get; private set; }
}
