using Sandbox;

namespace Ironsim.Actor;

public partial class ActorComponent
{
	public BodyComponent Body { get; set; }

	[Property, ToggleGroup("Body")]
	private bool EnableKnockBack { get; set; } = true;

	[Property, ToggleGroup("Body")]
	private bool ShouldRagdollOnDeath { get; set; } = true;
}
