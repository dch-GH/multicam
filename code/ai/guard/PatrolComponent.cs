namespace Ironsim.AI;

public sealed class PatrolComponent : Component
{
	[Property] public List<GameObject> Points { get; private set; }
	public bool ShouldPatrol { get; set; }
	public bool HasPatrolPath => Points.Count > 0;
	public int CurrentNodeIndex { get; private set; }
	public bool Reversing { get; private set; }
	private List<PatrolNode> _path = new();

	protected override void OnStart()
	{
		foreach ( var p in Points )
		{
			_path.Add( new PatrolNode() { Visited = false, Position = p.Transform.Position } );
		}
	}

	// public PatrolNode Next()
	// {
	// We are at the end of the loop.
	// if ( CurrentNodeIndex >= _path.Count )
	// }
}
