namespace Ironsim.AI;

public sealed class PatrolPathComponent : Component
{
	[Property] public bool IsLoop { get; private set; }
	public List<PatrolNode> Path { get; private set; }

	// EDITOR ONLY
	public bool IsBeingEdited = false;

	[Button( "Edit Mode" )]
	public void EnterEditMode()
	{
		IsBeingEdited = !IsBeingEdited;
	}
}
