namespace Ironsim.AI;

public sealed class PatrolPathComponent : Component
{
	[Property] public PatrolPath Path { get; private set; }
	[Property] public bool IsLoop { get => Path.IsLoop; set => Path.IsLoop = value; }

	[Button( "Add Node", Icon = "wifi" )]
	private void AddNode()
	{
		Path.AddNode();
	}

	protected override void DrawGizmos()
	{
		{
			var index = 0;
			foreach ( var p in Path.Nodes )
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
			var first = Path.Nodes[0];
			var last = Path.Nodes.Last();

			Gizmo.Draw.Line( first.Position, last.Position );
		}
	}
}
