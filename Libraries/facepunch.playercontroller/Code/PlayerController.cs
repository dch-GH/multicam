using Sandbox.Citizen;

[Group("Walker")]
[Title("Walker - Player Controller")]
public sealed class PlayerController : Component
{
    [Property] public CharacterController CharacterController { get; set; }
    [Property] public float CrouchMoveSpeed { get; set; } = 64.0f;
    [Property] public float WalkMoveSpeed { get; set; } = 190.0f;
    [Property] public float RunMoveSpeed { get; set; } = 190.0f;
    [Property] public float SprintMoveSpeed { get; set; } = 320.0f;

    public float MaxStamina { get; set; } = 100f;
    public float CurrentStamina { get; set; } = 100f;
    public float StaminaRegenRate { get; set; } = 10f; // Stamina per second
    public float SprintStaminaDrainRate { get; set; } = 20f; // Stamina drain per second while sprinting
    public bool IsSprinting { get; set; } = false;


    [Property] public CitizenAnimationHelper AnimationHelper { get; set; }

    [Sync] public bool Crouching { get; set; }
    [Sync] public Angles EyeAngles { get; set; }
    [Sync] public Vector3 WishVelocity { get; set; }

    public bool WishCrouch;
    public float EyeHeight = 64;

    public bool isSliding = false;
    private float slideDuration = 0.75f; // Slide duration in seconds
    private float slideSpeed = 400.0f; // Speed during the slide
    private float slideTime = 0.0f; // Timer to track slide duration
    private Vector3 slideDirection;  // Store the direction of the slide
    private float cameraRollAngle = 10.0f;  // Angle of the camera roll during the slide
    private float currentCameraRoll = 0.0f;  // The current roll angle of the camera

    protected override void OnUpdate()
    {
        if (!IsProxy)
        {
            MouseInput();
            Transform.Rotation = new Angles(0, EyeAngles.yaw, 0);
        }

        UpdateAnimation();
    }

    protected override void OnFixedUpdate()
    {
        if (IsProxy)
            return;

        CrouchingInput(); // Handle crouch/stand-up first
        MovementInput();  // Then handle movement
    }

    private void MouseInput()
    {
        var e = EyeAngles;
        e += Input.AnalogLook;
        e.pitch = e.pitch.Clamp(-90, 90);
        e.roll = 0.0f;
        EyeAngles = e;
    }

    float CurrentMoveSpeed
    {
        get
        {
            if (Crouching) return CrouchMoveSpeed;
            if (Input.Down("run")) return SprintMoveSpeed;
            if (Input.Down("walk")) return WalkMoveSpeed;

            return RunMoveSpeed;
        }
    }

    RealTimeSince lastGrounded;
    RealTimeSince lastUngrounded;
    RealTimeSince lastJump;

    float GetFriction()
    {
        if (CharacterController.IsOnGround) return 6.0f;

        // air friction
        return 0.2f;
    }

    private void MovementInput()
    {
        if (CharacterController is null)
            return;

        var cc = CharacterController;
        Vector3 halfGravity = Scene.PhysicsWorld.Gravity * Time.Delta * 0.5f;

        WishVelocity = Input.AnalogMove;

        // Check for slide initiation (CTRL) while sprinting, and ensure the player isn't crouched
        if (Input.Pressed("slide") && Input.Down("run") && cc.IsOnGround && !isSliding && !Crouching)
        {
            StartSlide();
            return;
        }

        // Handle sliding movement
        if (isSliding)
        {
            HandleSlide();
            return;
        }

        // Handle sprinting with stamina
        if (Input.Down("run") && CurrentStamina > 0)
        {
            IsSprinting = true;
            CurrentStamina -= SprintStaminaDrainRate * Time.Delta; // Drain stamina while sprinting
            CurrentStamina = MathF.Max(CurrentStamina, 0); // Clamp to 0
        }
        else
        {
            IsSprinting = false;
        }

        // Regenerate stamina when not sprinting
        if (!IsSprinting && !isSliding)
        {
            CurrentStamina += StaminaRegenRate * Time.Delta;
            CurrentStamina = MathF.Min(CurrentStamina, MaxStamina); // Clamp to MaxStamina
        }

        // Handle jump and other movements
        if (lastGrounded < 0.2f && lastJump > 0.3f && Input.Pressed("jump"))
        {
            lastJump = 0;
            cc.Punch(Vector3.Up * 300);
            StandUp();
        }

        if (!WishVelocity.IsNearlyZero())
        {
            WishVelocity = new Angles(0, EyeAngles.yaw, 0).ToRotation() * WishVelocity;
            WishVelocity = WishVelocity.WithZ(0);
            WishVelocity = WishVelocity.ClampLength(1);
            WishVelocity *= CurrentMoveSpeed;

            if (!cc.IsOnGround)
            {
                WishVelocity = WishVelocity.ClampLength(50);
            }
        }

        cc.ApplyFriction(GetFriction());

        if (cc.IsOnGround)
        {
            cc.Accelerate(WishVelocity);
            cc.Velocity = CharacterController.Velocity.WithZ(0);
        }
        else
        {
            cc.Velocity += halfGravity;
            cc.Accelerate(WishVelocity);
        }

        // Handle movement interactions
        var pushVelocity = PlayerPusher.GetPushVector(Transform.Position + Vector3.Up * 40.0f, Scene, GameObject);
        if (!pushVelocity.IsNearlyZero())
        {
            var travelDot = cc.Velocity.Dot(pushVelocity.Normal);
            if (travelDot < 0)
            {
                cc.Velocity -= pushVelocity.Normal * travelDot * 0.6f;
            }

            cc.Velocity += pushVelocity * 128.0f;
        }

        cc.Move();

        if (!cc.IsOnGround)
        {
            cc.Velocity += halfGravity;
        }
        else
        {
            cc.Velocity = cc.Velocity.WithZ(0);
        }

        if (cc.IsOnGround)
        {
            lastGrounded = 0;
        }
        else
        {
            lastUngrounded = 0;
        }
    }

    float DuckHeight = (64 - 36);

    bool CanUncrouch()
    {
        if (!Crouching)
        {
            Log.Info("CanUncrouch: Not crouching, can stand up.");
            return true;
        }

        // Temporarily removed lastUngrounded condition
        var tr = CharacterController.TraceDirection(Vector3.Up * DuckHeight);
        bool canUncrouch = !tr.Hit;
        Log.Info($"CanUncrouch: TraceHit: {tr.Hit}, CanUncrouch: {canUncrouch}");
        return canUncrouch; // hit nothing - we can!
    }

    public void CrouchingInput()
    {
        // If the player is crouching and presses the sprint key, stand up and start sprinting
        if (Crouching && Input.Pressed("run"))
        {
            Log.Info("CrouchingInput: Sprint key pressed while crouching. Standing up.");
            StandUp();
            return;
        }

        // Toggle crouch state with C key
        if (Input.Pressed("duck"))
        {
            ToggleCrouch();
        }
    }

    private void ToggleCrouch()
    {
        if (isSliding)
        {
            Log.Info("ToggleCrouch: Cannot toggle crouch while sliding.");
            return; // Prevent toggling crouch while sliding
        }

        if (!Crouching)
        {
            Log.Info("ToggleCrouch: Initiating Crouch.");
            Crouch();
        }
        else
        {
            Log.Info("ToggleCrouch: Initiating StandUp.");
            StandUp();
        }
    }

    private void Crouch()
    {
        Crouching = true;
        CharacterController.Height = 36;
        Log.Info($"Crouch: Character is now crouching. Height: {CharacterController.Height}, EyeHeight: {EyeHeight}");
    }

    private void StandUp()
    {
        if (!Crouching)
        {
            Log.Info("StandUp: Character is already standing.");
            return;
        }

        if (CanUncrouch())
        {
            Crouching = false;
            CharacterController.Height = 64;
            Log.Info($"StandUp: Character is now standing. Height: {CharacterController.Height}, EyeHeight: {EyeHeight}");
        }
        else
        {
            Log.Info("StandUp: Cannot stand up. Trace detected obstruction.");
        }
    }

    private void StartSlide()
    {
        if (isSliding || Crouching)
        {
            Log.Info("StartSlide: Cannot start sliding. Either already sliding or crouched.");
            return;  // Prevent starting a new slide while sliding or crouched
        }

        isSliding = true;
        slideTime = 0.0f;
        CharacterController.Height = 36; // Reduce height for the slide
        Log.Info($"StartSlide: Sliding started. Height set to {CharacterController.Height}");

        // Lock the slide direction based on the player's current look direction
        slideDirection = new Angles(0, EyeAngles.yaw, 0).ToRotation().Forward;
    }

    private void HandleSlide()
    {
        slideTime += Time.Delta;

        if (slideTime >= slideDuration || !Input.Down("slide"))
        {
            EndSlide();
            return;
        }

        // Apply slide velocity in the locked slide direction
        CharacterController.Velocity = slideDirection * slideSpeed;

        // Reduce friction during the slide for a smooth motion
        CharacterController.ApplyFriction(0.5f);
        CharacterController.Move();
    }

    private void EndSlide()
    {
        isSliding = false;
        Crouching = true;  // Player is now crouched after sliding
        CharacterController.Height = 36; // Maintain crouched height
        Log.Info($"EndSlide: Sliding ended. Crouching: {Crouching}, Height: {CharacterController.Height}");
    }

    private void UpdateCamera()
    {
        var camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault(x => x.IsMainCamera);
        if (camera is null) return;

        var targetEyeHeight = isSliding ? 28 : (Crouching ? 28 : 64);
        EyeHeight = EyeHeight.LerpTo(targetEyeHeight, RealTime.Delta * 10.0f);

        var targetCameraPos = Transform.Position + new Vector3(0, 0, EyeHeight);

        // Determine the target roll angle based on whether the player is sliding
        var targetRoll = isSliding ? cameraRollAngle : 0.0f;

        // Interpolate the current camera roll towards the target roll
        currentCameraRoll = currentCameraRoll.LerpTo(targetRoll, RealTime.Delta * 10.0f);

        // Smooth view z, so when going up and down stairs or ducking, it's smooth
        if (lastUngrounded > 0.2f)
        {
            targetCameraPos.z = camera.Transform.Position.z.LerpTo(targetCameraPos.z, RealTime.Delta * 25.0f);
        }

        camera.Transform.Position = targetCameraPos;
        camera.Transform.Rotation = EyeAngles.WithRoll(currentCameraRoll);
        camera.FieldOfView = Preferences.FieldOfView;
    }

    protected override void OnPreRender()
    {
        UpdateBodyVisibility();

        if (IsProxy)
            return;

        UpdateCamera();
    }

    private void UpdateAnimation()
    {
        if (AnimationHelper is null) return;

        var wv = WishVelocity.Length;

        AnimationHelper.WithWishVelocity(WishVelocity);
        AnimationHelper.WithVelocity(CharacterController.Velocity);
        AnimationHelper.IsGrounded = CharacterController.IsOnGround;
        AnimationHelper.DuckLevel = Crouching ? 1.0f : 0.0f;

        AnimationHelper.MoveStyle = wv < 160f ? CitizenAnimationHelper.MoveStyles.Walk : CitizenAnimationHelper.MoveStyles.Run;

        var lookDir = EyeAngles.ToRotation().Forward * 1024;
        AnimationHelper.WithLook(lookDir, 1, 0.5f, 0.25f);
    }

    private void UpdateBodyVisibility()
    {
        if (AnimationHelper is null)
            return;

        var renderMode = ModelRenderer.ShadowRenderType.On;
        if (!IsProxy) renderMode = ModelRenderer.ShadowRenderType.ShadowsOnly;

        AnimationHelper.Target.RenderType = renderMode;

        foreach (var clothing in AnimationHelper.Target.Components.GetAll<ModelRenderer>(FindMode.InChildren))
        {
            if (!clothing.Tags.Has("clothing"))
                continue;

            clothing.RenderType = renderMode;
        }
    }
}
