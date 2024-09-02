﻿using Sandbox;

namespace Ironsim.Actor;

public partial class BodyComponent
{

	TimeSince timeSinceStep;

	private void OnFootStepEvent(SceneModel.FootstepEvent e)
	{
		if (timeSinceStep < 0.2f)
			return;

		var tr = Scene.Trace
			.Ray(e.Transform.Position + Vector3.Up * 20, e.Transform.Position + Vector3.Up * -20)
			.IgnoreGameObjectHierarchy(Actor.GameObject)
			.Run();

		if (!tr.Hit)
			return;

		if (tr.Surface is null)
			return;

		timeSinceStep = 0;

		var sound = e.FootId == 0 ? tr.Surface.Sounds.FootLeft : tr.Surface.Sounds.FootRight;
		if (sound is null) return;

		var handle = Sound.Play(sound, tr.HitPosition + tr.Normal * 5);
		handle.Volume *= e.Volume;
	}
}
