using Sandbox;

using System;

namespace Ironsim.Actor;

[GameResource("Actor Spawn Data", "spawn", "Used to add actors to encounters.", Icon = "groups")]
public class ActorSpawnResource : GameResource
{
	[Property]
	public int Level { get; set; } = 1;
	public int Xp => Level * 10;

	[Property]
	[Range(0, 1)]
	public float Probability { get; set; } = 1;

	[Property]
	public GameObject Prefab { get; set; }
}
