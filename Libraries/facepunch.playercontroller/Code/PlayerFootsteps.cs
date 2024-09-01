public sealed class PlayerFootsteps : Component
{
    [Property] SkinnedModelRenderer Source { get; set; }

    private PlayerController playerController;

    protected override void OnEnabled()
    {
        if (Source is null)
            return;

        Source.OnFootstepEvent += OnEvent;

        // Get the PlayerController reference
        playerController = this.Components.Get<PlayerController>();
    }

    protected override void OnDisabled()
    {
        if (Source is null)
            return;

        Source.OnFootstepEvent -= OnEvent;
    }

    TimeSince timeSinceStep;

    private void OnEvent(SceneModel.FootstepEvent e)
    {
        // If the player is sliding, don't play footstep sounds
        if (playerController != null && playerController.isSliding)
        {
            // Play slide sound only once when the slide starts
            if (timeSinceStep > 1.0f)
            {
                PlaySlideSound();
                timeSinceStep = 0;
            }
            return;
        }

        if (timeSinceStep < 0.2f)
            return;

        var tr = Scene.Trace
            .Ray(e.Transform.Position + Vector3.Up * 20, e.Transform.Position + Vector3.Up * -20)
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
        handle.Update();
    }

    private void PlaySlideSound()
    {
        Sound.Play("sound/slide.mp3");
    }
}
