namespace Ironsim;

public sealed class MovingLightComponent : Component
{
	private float minTarget;
	private float maxTarget;

	protected override void OnStart()
	{
		base.OnStart();
		minTarget = Transform.Position.x - 8;
		maxTarget = Transform.Position.x + 8;
	}

	protected override void OnUpdate()
	{
		var sin = MathF.Sin( Time.Now );
		var x = Transform.Position.x;
		Transform.Position = Transform.Position.WithX( x + sin * Time.Delta * 128 );
	}
}
