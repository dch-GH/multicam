namespace Ironsim.AI;

public sealed class PatrolPathComponent : Component
{
	[Property] public List<PatrolNodeComponent> Path { get; private set; }

	/// <summary>
	/// Do we loop at the end?
	/// </summary>
	/// /// 
	[Property] public bool IsLoop;

	public int Count => Path is null ? 0 : Path.Count;
	public bool IsEmpty => Path is null ? true : Path.Count <= 0;

	public PatrolNodeComponent this[int index]
	{
		get => Path[index];
		set => Path[index] = value;
	}

	[Button( "Add Node", Icon = "language" )]
	private void AddNode()
	{
		Path ??= new();

		var nodeGO = Scene.CreateObject( true );
		nodeGO.SetParent( GameObject );

		var node = nodeGO.Components.Create<PatrolNodeComponent>();

		if ( Path.Count <= 0 )
		{
			nodeGO.Name = string.Format( "Node: {0} - {1}", 0, GameObject.Id );
			Path.Add( node );
			return;
		}

		var last = Path.Last();
		node.Position = last.Position + Vector3.Forward * 32;
		node.Index = last.Index + 1;
		nodeGO.Name = string.Format( "Node: {0} - {1}", node.Index, GameObject.Id );
		Path.Add( node );
	}

	[Button( "Remove Node", Icon = "close" )]
	private void RemoveNode()
	{
		if ( IsEmpty )
			return;

		var index = Path.Count - 1;
		Path.ElementAt( index ).GameObject.Destroy();
		Path.RemoveAt( index );
	}

	protected override void DrawGizmos()
	{
		if ( !Gizmo.IsSelected )
			return;

		if ( IsEmpty )
			return;

		{
			var index = 0;
			foreach ( var p in Path )
			{
				using ( Gizmo.Scope( index.ToString(), p.Position ) )
				{
					if ( index == 0 )
					{
						Gizmo.Draw.Color = Gizmo.Colors.Green;
						Gizmo.Draw.LineSphere( Vector3.Zero, 16 );
					}
					else if ( index % 2 == 0 )
					{
						Gizmo.Draw.Color = Gizmo.Colors.Blue;
						Gizmo.Draw.LineSphere( Vector3.Zero, 16 );
					}
					else
					{
						Gizmo.Draw.Color = Color.White;
						Gizmo.Draw.LineSphere( Vector3.Zero, 16 );
					}

					Gizmo.Draw.Color = Color.White;
					Gizmo.Draw.WorldText( $"Index: {p.Index}", new Transform().WithRotation( Rotation.LookAt( Gizmo.Camera.Rotation.Right, Gizmo.Camera.Rotation.Backward ) ) );

					if ( Gizmo.Control.Position( "pTransform", p.Position, out var newP1 ) )
					{
						p.Position = newP1;
					}
				}

				index++;
			}
		}

		for ( int i = 0; i < Path.Count; i++ )
		{
			if ( i + 1 >= Path.Count )
				continue;

			var p1 = Path[i];
			var p2 = Path[i + 1];

			Gizmo.Draw.Line( p1.Position, p2.Position );

		}

		if ( IsLoop )
		{
			var first = Path[0];
			var last = Path.Last();

			Gizmo.Draw.Line( first.Position, last.Position );
		}
	}
}
