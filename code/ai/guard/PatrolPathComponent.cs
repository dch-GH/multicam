
namespace Ironsim.AI;

public sealed class PatrolPathComponent : Component
{
	[Property] public List<PatrolNodeComponent> Nodes { get; private set; }

	/// <summary>
	/// Do we loop at the end?
	/// </summary>
	/// /// 
	[Property] public bool IsLoop;

	public bool IsBeingEdited { get; private set; }

	public int Count => Nodes is null ? 0 : Nodes.Count;
	public bool IsEmpty => Nodes is null ? true : Nodes.Count <= 0;

	public PatrolNodeComponent this[int index]
	{
		get => Nodes[index];
		set => Nodes[index] = value;
	}

	public PatrolNodeComponent Next( PatrolNodeComponent node )
	{
		return Nodes.First( x => x.Index == node.Index + 1 );
	}

	[Button( "Add Node", Icon = "language" )]
	private void AddNode()
	{
		Nodes ??= new();

		var nodeGO = Scene.CreateObject( true );
		nodeGO.SetParent( GameObject );

		var node = nodeGO.Components.Create<PatrolNodeComponent>();

		if ( Nodes.Count <= 0 )
		{
			nodeGO.Transform.Position = Transform.Position;
			nodeGO.Name = string.Format( "Node: {0} - {1}", 0, GameObject.Id );
			Nodes.Add( node );
			return;
		}

		var last = Nodes.Last();
		node.Transform.Position = last.Transform.Position + Vector3.Forward * 32;
		node.Index = last.Index + 1;
		nodeGO.Name = string.Format( "Node: {0} - {1}", node.Index, GameObject.Id );
		Nodes.Add( node );
	}

	[Button( "Remove Node", Icon = "close" )]
	private void RemoveNode()
	{
		if ( IsEmpty )
			return;

		var index = Nodes.Count - 1;
		Nodes.ElementAt( index ).GameObject.Destroy();
		Nodes.RemoveAt( index );
	}

	protected override Task OnLoad()
	{
		Nodes ??= new();
		foreach ( var x in GameObject.Children )
		{
			if ( x.Components.TryGet<PatrolNodeComponent>( out var p ) )
				Nodes.Add( p );
		}

		return Task.CompletedTask;
	}

	protected override void DrawGizmos()
	{
		IsBeingEdited = Gizmo.IsSelected;

		if ( !IsBeingEdited )
		{
			using ( Gizmo.Scope( "main" ) )
			{
				Gizmo.Draw.Color = Color.Green;
				Gizmo.Draw.SolidSphere( Vector3.Zero, 8 );
				Gizmo.Draw.Color = Color.White;
				Gizmo.Draw.WorldText( string.Format( "AI Path - {0}", GameObject.Name ), global::Transform.Zero.WithPosition( Vector3.Up * 32 ).WithRotation( GizmoUtils.FaceGizmoCamera ) );
			}
		}

		if ( !Gizmo.IsSelected )
			return;

		if ( IsEmpty )
			return;

		for ( int i = 0; i < Nodes.Count; i++ )
		{
			if ( i + 1 >= Nodes.Count )
				continue;

			var p1 = Nodes[i];
			var p2 = Nodes[i + 1];

			using ( Gizmo.Scope( i.ToString(), Scene.Transform.World ) )
			{
				Gizmo.Draw.Color = Color.Blue;
				Gizmo.Draw.Arrow( p1.Transform.LocalPosition, p2.Transform.LocalPosition );
			}
		}

		if ( IsLoop )
		{
			var first = Nodes[0];
			var last = Nodes.Last();

			using ( Gizmo.Scope( "loopAround", Scene.Transform.World ) )
			{
				Gizmo.Draw.Color = Color.Green;
				Gizmo.Draw.Arrow( last.Transform.LocalPosition, first.Transform.LocalPosition );
			}
		}
	}
}
