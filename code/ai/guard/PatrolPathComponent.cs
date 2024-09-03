namespace Ironsim.AI;

public sealed class PatrolPathComponent : Component
{
	[Property] public bool IsLoop { get; private set; }
	public List<PatrolNode> Path { get; private set; }

}
