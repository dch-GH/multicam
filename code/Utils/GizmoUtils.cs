namespace Ironsim;

public static class GizmoUtils
{
	public static Rotation FaceGizmoCamera => Rotation.LookAt( Gizmo.Camera.Rotation.Right, Gizmo.Camera.Rotation.Backward );
}
