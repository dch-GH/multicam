namespace Ironsim;

public static class Blackboard
{
	public static IReadOnlyCollection<GameObject> Actors => actors;
	public static IReadOnlyCollection<TargetComponent> Targets => targets;

	private static readonly List<GameObject> actors = new();
	private static readonly List<TargetComponent> targets = new();

	public static void Register( GameObject actorComponent )
	{
		actors.Add( actorComponent );
	}

	public static void UnRegister( GameObject actorComponent )
	{
		actors.Remove( actorComponent );
	}

	public static void Register( TargetComponent targetComponent )
	{
		targets.Add( targetComponent );
	}

	public static void UnRegister( TargetComponent targetComponent )
	{
		targets.Remove( targetComponent );
	}
}
