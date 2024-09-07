namespace Ironsim;

public sealed class PlayerComponent : Component
{
	[RequireComponent]
	public LightDetectorComponent LightDetector { get; private set; }
}
