using Sandbox;

namespace Ironsim.Actor;

public partial class ActorComponent
{
	[Property, ToggleGroup("SFX")] 
	public Sounds Sounds { get; set; }

	[Property, ToggleGroup("SFX")]
	private bool EnablePainSounds { get; set; } = true;

	[Property, ToggleGroup("SFX")]
	private bool EnableDeathSounds { get; set; } = true;

	[Property, ToggleGroup("SFX")]
	private bool EnableImpactSounds { get; set; } = true;
}
