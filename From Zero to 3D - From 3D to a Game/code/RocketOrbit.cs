using Godot;

namespace VisA.OrbitWorkshop;

/// <summary>Moves a rocket in the XZ plane using a fixed central gravitational field.</summary>
public partial class RocketOrbit : CharacterBody3D
{
    [ExportGroup("Scene references")]
    [Export] private StaticBody3D _planet = null!;
    [Export] private Label _status = null!;

    [ExportGroup("Experiment")]
    [Export] private bool _useGravity = true;
    [Export] private bool _allowThrust = true;
    [Export(PropertyHint.Range, "1,500,1")] private float _mu = 96.0f;
    [Export(PropertyHint.Range, "1,8,1")] private int _substeps = 4;
    [Export] private Vector3 _initialOffset = new Vector3(6.0f, 0.0f, 0.0f);
    [Export(PropertyHint.Range, "0,8,0.1")] private float _initialSpeed = 4.0f;
    [Export] private float _initialHeadingDegrees;

    [ExportGroup("Controls")]
    [Export] private float _placementSpeed = 3.0f;
    [Export] private float _turnSpeedDegrees = 90.0f;
    [Export] private float _thrustAcceleration = 2.0f;

    private enum FlightState { Placement, Flying, Paused, Crashed, OutsideArea }
    private FlightState _state;
    private Vector3 _launchOffset;
    private float _launchSpeed;
    private float _heading;

    // Fixed workshop geometry: planet radius 2, rocket bounding radius 0.65.
    private const float MinimumLaunchRadius = 3.0f;
    private const float MaximumLaunchRadius = 9.0f;
    private const float DemoRadius = 10.0f;

    public bool IsFlying => _state == FlightState.Flying;
    public int ResetVersion { get; private set; }

    public override void _Ready()
    {
        if (_planet == null || _status == null)
        {
            GD.PushError("Assign Planet and Status on the Rocket instance in Main.tscn.");
            SetPhysicsProcess(false);
            return;
        }

        ResetExperiment();
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        if (Input.IsActionJustPressed("reset"))
        {
            ResetExperiment();
            return;
        }

        if (Input.IsActionJustPressed("pause"))
        {
            if (_state == FlightState.Flying)
            {
                _state = FlightState.Paused;
            }
            else if (_state == FlightState.Paused)
            {
                _state = FlightState.Flying;
            }
        }

        if (_state == FlightState.Placement)
        {
            UpdatePlacement(dt);
        }
        else if (_state == FlightState.Flying)
        {
            Simulate(dt);
        }

        UpdateStatus();
    }

    private void ResetExperiment()
    {
        _state = FlightState.Placement;
        _launchOffset = ClampLaunchOffset(_initialOffset);
        _launchSpeed = Mathf.Clamp(_initialSpeed, 0.0f, 8.0f);
        _heading = Mathf.DegToRad(_initialHeadingDegrees);
        GlobalPosition = _planet.GlobalPosition + _launchOffset;
        Rotation = new Vector3(0.0f, _heading, 0.0f);
        Velocity = Vector3.Zero;
        ResetVersion++;
        UpdateStatus();
    }

    private static Vector3 ClampLaunchOffset(Vector3 offset)
    {
        offset.Y = 0.0f;
        float radius = offset.Length();
        if (radius < 0.001f) return Vector3.Right * MinimumLaunchRadius;
        return offset / radius * Mathf.Clamp(radius, MinimumLaunchRadius, MaximumLaunchRadius);
    }

    private void UpdatePlacement(float dt)
    {
        Vector2 input = Input.GetVector("place_left", "place_right", "place_forward", "place_back");
        Vector3 movement = new Vector3(input.X, 0.0f, input.Y) * _placementSpeed * dt;
        _launchOffset = ClampLaunchOffset(_launchOffset + movement);
        GlobalPosition = _planet.GlobalPosition + _launchOffset;

        _heading += Input.GetAxis("turn_right", "turn_left") * Mathf.DegToRad(_turnSpeedDegrees) * dt;
        _launchSpeed = Mathf.Clamp(_launchSpeed + Input.GetAxis("speed_down", "speed_up") * 2.0f * dt, 0.0f, 8.0f);
        Rotation = new Vector3(0.0f, _heading, 0.0f);

        if (Input.IsActionJustPressed("launch"))
        {
            Velocity = -GlobalBasis.Z * _launchSpeed;
            _state = FlightState.Flying;
        }
    }

    private void Simulate(float dt)
    {
        if (_allowThrust)
        {
            _heading += Input.GetAxis("turn_right", "turn_left") * Mathf.DegToRad(_turnSpeedDegrees) * dt;
            Rotation = new Vector3(0.0f, _heading, 0.0f);
        }

        int steps = Mathf.Max(_substeps, 1);
        float h = dt / steps;
        for (int i = 0; i < steps; i++)
        {
            Vector3 acceleration = Vector3.Zero;
            if (_useGravity)
            {
                Vector3 toPlanet = _planet.GlobalPosition - GlobalPosition;
                toPlanet.Y = 0.0f;
                float distanceSquared = Mathf.Max(toPlanet.LengthSquared(), 0.01f);
                acceleration = toPlanet.Normalized() * (_mu / distanceSquared);
            }

            if (_allowThrust && Input.IsActionPressed("thrust"))
            {
                acceleration += -GlobalBasis.Z * _thrustAcceleration;
            }

            // Semi-implicit Euler: update velocity, then sweep the proposed displacement.
            Velocity += acceleration * h;
            KinematicCollision3D? collision = MoveAndCollide(Velocity * h);
            if (collision != null)
            {
                Velocity = Vector3.Zero;
                _state = FlightState.Crashed;
                GD.Print("Collision: experiment finished. Press R to reset.");
                break;
            }

            if (GlobalPosition.DistanceTo(_planet.GlobalPosition) > DemoRadius)
            {
                _state = FlightState.OutsideArea;
                break;
            }
        }
    }

    private void UpdateStatus()
    {
        Vector3 displayedVelocity = _state == FlightState.Placement ? -GlobalBasis.Z * _launchSpeed : Velocity;
        float radius = GlobalPosition.DistanceTo(_planet.GlobalPosition);
        float circularSpeed = Mathf.Sqrt(_mu / Mathf.Max(radius, 0.001f));
        string stateText = _state switch
        {
            FlightState.Placement => "PLACE: arrows move | A/D aim | Z/X speed | Space launch",
            FlightState.Flying => "FLYING: A/D turn | W thrust | P pause | R reset",
            FlightState.Paused => "PAUSED: P resume | R reset",
            FlightState.Crashed => "COLLISION: R reset",
            _ => "OUTSIDE DEMO AREA: R reset (this alone does not prove escape)"
        };

        _status.Text = stateText + $"\nRadius: {radius:F2}   Speed: {displayedVelocity.Length():F2}   Circular speed: {circularSpeed:F2}";
        _status.Text += $"\nVelocity: ({displayedVelocity.X:F2}, {displayedVelocity.Y:F2}, {displayedVelocity.Z:F2})";
        _status.Text += $"\nGravity: {_useGravity}   Thrust enabled: {_allowThrust}";
    }
}
