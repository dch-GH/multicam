namespace Ironsim.AI;

// NOTE: (duston) These will not be components (hopefully) down the line, 
// just need to figure out if s&box's serialization stuff works. Making it a Component makes it super easy.
public class PatrolNodeComponent : Component
{
	[Property] public int Index;
	[Property] public bool Visited;
	[Property] public Vector3 Position;
}
