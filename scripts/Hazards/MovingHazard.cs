using Godot;

// Patrolling caryatid hazard (scenes/hazards/moving_hazard.tscn). The statue
// walks along a patrol path at a steady speed and loops back to the start at
// the end; touching it knocks the marble back (MarbleController.GetHitPhys).
//
// It's a frozen kinematic RigidBody3D moved by position each physics step:
// kinematic bodies are meant to be moved directly, and they still push the
// marble aside physically as they pass.
public partial class MovingHazard : RigidBody3D
{
	// Patrol shape, used only when PatternPath isn't set. Points are relative
	// to the hazard's starting position.
	[Export] public Curve3D Pattern;

	// Walking speed along the patrol, in metres per second.
	[Export] public float PatternSpeed = 0.75f;

	// Path3D whose curve is the patrol. Normally the hazard's own child, so the
	// patrol scales and rotates with the hazard when it's placed in a level.
	[Export] public Path3D PatternPath;

	private float _distanceTravelled;
	private float _patternLength;

	// The patrol path's transform, captured once at the start. The Path3D is a
	// child of the hazard, so reading it live would move the path along with
	// the statue it's guiding.
	private Transform3D _pathTransform;

	// Shifts the whole path so it starts exactly where the hazard was placed
	// in the level. The path's first point doesn't sit on the Path3D's
	// origin, so following the raw path made every hazard jump up off the
	// floor when the level started.
	private Vector3 _pathStartOffset;

	public override void _Ready()
	{
		Freeze = true;
		FreezeMode = FreezeModeEnum.Kinematic;

		// A RigidBody3D only reports body_entered with contact monitoring on.
		ContactMonitor = true;
		MaxContactsReported = 4;

		if (PatternPath?.Curve != null)
		{
			Pattern = PatternPath.Curve;
			_pathTransform = PatternPath.GlobalTransform;
		}
		else
		{
			_pathTransform = GlobalTransform;
		}

		if (Pattern == null)
		{
			return;
		}

		_patternLength = Pattern.GetBakedLength();
		_pathStartOffset = GlobalPosition - _pathTransform * Pattern.SampleBaked(0.0f);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_patternLength <= 0.0f)
		{
			return;
		}

		// Wrap rather than reset to zero, so looping back to the start doesn't
		// lose the distance covered in this step.
		_distanceTravelled = Mathf.PosMod(_distanceTravelled + PatternSpeed * (float)delta, _patternLength);
		GlobalPosition = _pathTransform * Pattern.SampleBaked(_distanceTravelled) + _pathStartOffset;
	}

	// Connected to this body's own body_entered signal in the scene.
	private void OnBodyEntered(Node3D body)
	{
		if (body is MarbleController marble)
		{
			marble.GetHitPhys(GlobalPosition);
		}
	}
}
