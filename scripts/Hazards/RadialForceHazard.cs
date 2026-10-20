using Godot;

// Shared behaviour of the wind hazards, BlowerHazard and VacuumHazard. While
// the marble is inside the hazard's cone-shaped wind area, it is pushed
// horizontally away from the hazard (blower) or pulled towards it (vacuum),
// harder the closer it is. The scene connects the area's body_entered and
// body_exited signals to the subclass, which passes them to Track/Untrack.
//
// The force only applies between those two signals, so the marble is never
// pushed from outside the visible wind, for example from behind the well or
// from a floor above or below it.
public partial class RadialForceHazard : RigidBody3D
{
	private RigidBody3D _marble;

	// Strength of the push at the hazard itself, in newtons.
	protected virtual float Force => 0.0f;

	// Horizontal distance, in metres, at which the push fades to nothing.
	protected virtual float Radius => 1.0f;

	// +1 pushes the marble away from the hazard, -1 pulls it in.
	protected virtual float Direction => 1.0f;

	public override void _Ready()
	{
		// The well itself never moves; it only exists to be collided with.
		Freeze = true;
		FreezeMode = FreezeModeEnum.Static;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_marble == null)
		{
			return;
		}

		Vector3 away = _marble.GlobalPosition - GlobalPosition;
		away.Y = 0.0f;
		float distance = away.Length();
		if (distance < 0.01f)
		{
			return;
		}

		float strength = Mathf.Clamp(1.0f - distance / Radius, 0.0f, 1.0f);

		// A resting marble is asleep and would ignore the force.
		_marble.Sleeping = false;
		_marble.ApplyCentralForce(away / distance * Direction * Force * strength);
	}

	protected void Track(Node3D body)
	{
		if (body is RigidBody3D marble && marble.IsInGroup("marble"))
		{
			_marble = marble;
		}
	}

	protected void Untrack(Node3D body)
	{
		if (body == _marble)
		{
			_marble = null;
		}
	}
}
