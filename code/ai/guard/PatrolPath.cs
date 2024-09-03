namespace Ironsim.AI;

public sealed class PatrolPath
{
	public List<PatrolNode> Nodes { get; private set; }

	/// <summary>
	/// Do we loop at the end?
	/// </summary>
	public bool IsLoop;
	public int Count => Nodes.Count;
	public bool IsEmpty => Nodes.Count <= 0;

	public void AddNode()
	{
		var node = new PatrolNode();
		if ( Nodes.Count <= 0 )
		{
			Nodes.Add( node );
			return;
		}

		var last = Nodes.Last();
		node.Position = last.Position + Vector3.Forward * 32;
		node.Index = last.Index + 1;
		Nodes.Add( node );
	}

	public PatrolNode this[int index]
	{
		get => Nodes[index];
		set => Nodes[index] = value;
	}
}
