
namespace Ironsim.AI;

public class PatrolNodeComponent : Component
{
	[Property] public int Index;
	[Property] public bool Visited;

	protected override void DrawGizmos()
	{
		if ( !GameObject.Parent.Components.TryGet<PatrolPathComponent>( out var path ) || !path.IsBeingEdited )
			return;

		using ( Gizmo.Scope( null, Scene.Transform.World ) )
		{
			Gizmo.Draw.Color = Index == 0 ? Gizmo.Colors.Green : Gizmo.Colors.Blue;
			Gizmo.Draw.LineSphere( Vector3.Zero, 16 );

			Gizmo.Draw.Color = Color.White;
			Gizmo.Draw.WorldText( $"Index: {Index}", new Transform( Vector3.Up * 24 ).WithRotation( GizmoUtils.FaceGizmoCamera ) );
		}

		using ( Gizmo.Scope( "worldTx", Scene.Transform.World ) )
		{
			if ( Gizmo.Control.Position( "pTransform", Transform.LocalPosition, out var newP1 ) )
			{
				Transform.LocalPosition = newP1;
			}
		}
	}
}
