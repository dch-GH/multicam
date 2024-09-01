namespace Ironsim;

public sealed class Player : Component
{
	[RequireComponent]
	public LightDetector LightDetector { get; private set; }
}
