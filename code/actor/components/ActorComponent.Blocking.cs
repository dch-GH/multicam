using Sandbox;

namespace Ironsim.Actor;

public partial class ActorComponent
{
	//[RequireComponent] public BlockManager BlockManager { get; set; }

	//public Handedness BlockingHand { get; set; }
	
	public bool IsBlocking { get; private set; }
	
	[Broadcast]
	public void SetBlockActive(bool b)
	{
		IsBlocking = b;
		//BlockManager.SetBlockActive(b);
	}
	
	public void InterruptBlock()
	{
		SetBlockActive(false);
		
		Body.Set("bBlockingRight", false);
		Body.Set("bBlockingLeft", false);
	}
}
