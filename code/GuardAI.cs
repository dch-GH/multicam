using Sandbox;
using System;
using System.Linq;

public partial class GuardAI : Component
{
    public Vector3[] PatrolPoints { get; set; }
    public float PatrolSpeed { get; set; } = 200.0f;
    public float ChaseSpeed { get; set; } = 300.0f;
    public float DetectionRange { get; set; } = 500.0f;
    public float DetectionAngle { get; set; } = 45.0f;

    private int currentPatrolIndex = 0;
    private Vector3 lastKnownPlayerPosition;
    private float searchDuration = 5.0f;
    private float searchStartTime;

    private enum GuardState { Patrolling, Searching, Chasing }
    private GuardState currentState = GuardState.Patrolling;

    // Constructor or Initialization
    public GuardAI()
    {
        // Initialize patrol points, or set them externally.
        if (Entity != null)
        {
            Entity.SetModel("models/citizen/citizen.vmdl");
            Entity.SetupPhysicsFromModel(PhysicsMotionType.Dynamic);

            PatrolPoints = new Vector3[]
            {
                Entity.Position,
                Entity.Position + new Vector3(500, 0, 0),
                Entity.Position + new Vector3(500, 500, 0),
                Entity.Position + new Vector3(0, 500, 0)
            };
        }
    }

    [Event.Tick.Server]
    public void Tick()
    {
        if (Entity == null) return;

        switch (currentState)
        {
            case GuardState.Patrolling:
                Patrol();
                break;
            case GuardState.Searching:
                Search();
                break;
            case GuardState.Chasing:
                Chase();
                break;
        }
    }

    private void Patrol()
    {
        if (PatrolPoints == null || PatrolPoints.Length == 0) return;

        var destination = PatrolPoints[currentPatrolIndex];
        var direction = (destination - Entity.Position).Normal;
        Entity.Position += direction * PatrolSpeed * Time.Delta;
        Entity.Rotation = Rotation.LookAt(direction);

        if (Vector3.Distance(Entity.Position, destination) < 10.0f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % PatrolPoints.Length;
        }

        DetectPlayer();
    }

    private void Search()
    {
        var direction = (lastKnownPlayerPosition - Entity.Position).Normal;
        Entity.Position += direction * PatrolSpeed * Time.Delta;
        Entity.Rotation = Rotation.LookAt(direction);

        var distanceToLastKnown = Vector3.Distance(Entity.Position, lastKnownPlayerPosition);

        if (distanceToLastKnown < 10.0f || Time.Now - searchStartTime > searchDuration)
        {
            currentState = GuardState.Patrolling;
        }

        DetectPlayer();
    }

    private void Chase()
    {
        var player = Entity.All.OfType<Player>().FirstOrDefault();

        if (player != null)
        {
            var direction = (player.Position - Entity.Position).Normal;
            Entity.Position += direction * ChaseSpeed * Time.Delta;
            Entity.Rotation = Rotation.LookAt(direction);

            var distanceToPlayer = Vector3.Distance(Entity.Position, player.Position);

            if (distanceToPlayer > DetectionRange)
            {
                currentState = GuardState.Searching;
                searchStartTime = Time.Now;
            }
            else
            {
                lastKnownPlayerPosition = player.Position;
            }
        }
        else
        {
            currentState = GuardState.Searching;
            searchStartTime = Time.Now;
        }
    }

    private void DetectPlayer()
    {
        var player = Entity.All.OfType<Player>().FirstOrDefault();

        if (player != null)
        {
            var directionToPlayer = (player.Position - Entity.Position).Normal;
            var distanceToPlayer = Vector3.Distance(Entity.Position, player.Position);
            var forward = Entity.Rotation.Forward;
            var angle = MathF.Acos(Vector3.Dot(forward, directionToPlayer)) * (180 / MathF.PI);

            if (distanceToPlayer <= DetectionRange && angle <= DetectionAngle)
            {
                currentState = GuardState.Chasing;
                lastKnownPlayerPosition = player.Position;
            }
        }
    }
}
