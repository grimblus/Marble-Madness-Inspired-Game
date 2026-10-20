using Godot;

// Impact sound for a loose physics prop: the vases (scenes/props/) and the
// wet-floor sign. Add it as a child of the prop's RigidBody3D and give it a
// stream - an AudioStreamRandomizer such as GlassImpacts.tres, so repeated
// hits vary in clip and pitch.
//
// It plays whenever the prop's motion changes sharply between physics steps:
// being hit by the marble, hitting the floor as it topples, or bumping
// another prop. Gravity and damping only change velocity a little each step,
// so they stay below the threshold. Louder jolts play louder. Like
// MarbleImpactSound, it only reads its parent's physics state, so the prop
// itself needs no audio code.
//
// Non-positional for the same reason as the marble's sounds: the camera
// follows the marble, and a 3D player would get quieter as the camera zooms out.
public partial class PropImpactSound : AudioStreamPlayer
{
	// Jolts smaller than this (change in speed per physics step, m/s) are
	// silent, so a settled prop resting on a tile never clicks.
	[Export] public float MinImpact = 0.6f;

	// Jolt at which the sound reaches MaxVolume.
	[Export] public float FullImpact = 4.0f;

	// Linear volume of the hardest impacts (1 = the clip's own level).
	[Export] public float MaxVolume = 1.0f;

	// Minimum seconds between sounds, so a prop rattling across the floor
	// doesn't trigger a burst of overlapping clips.
	[Export] public float MinInterval = 0.12f;

	private RigidBody3D _prop;
	private Vector3 _previousLinear;
	private Vector3 _previousAngular;
	private float _leverArm;
	private float _cooldown;

	// Props are placed slightly above the floor and drop onto it when the
	// level loads. Stay silent until the prop has come to rest once, or a
	// room full of vases would all clink at the start of every level.
	private bool _armed;

	public override void _Ready()
	{
		_prop = GetParent<RigidBody3D>();
		_leverArm = GetLeverArm();
	}

	public override void _PhysicsProcess(double delta)
	{
		_cooldown -= (float)delta;

		// A sleeping prop isn't moving, so there's nothing to measure. Most
		// props spend most of a level asleep, so this skips nearly all of the
		// work. The first step after a hit wakes it compares against zero,
		// which is the hit itself.
		if (_prop.Sleeping)
		{
			_previousLinear = Vector3.Zero;
			_previousAngular = Vector3.Zero;
			_armed = true;
			return;
		}

		// A tipping prop barely changes its linear speed when its top hits
		// the floor, but its spin stops abruptly. Spin is converted to the
		// speed of the prop's outer edge so both are measured in m/s.
		Vector3 linear = _prop.LinearVelocity;
		Vector3 angular = _prop.AngularVelocity;
		float jolt = Mathf.Max(
			(linear - _previousLinear).Length(),
			(angular - _previousAngular).Length() * _leverArm);

		_previousLinear = linear;
		_previousAngular = angular;

		if (!_armed)
		{
			_armed = _prop.Sleeping;
			return;
		}

		if (jolt < MinImpact || _cooldown > 0.0f)
		{
			return;
		}

		_cooldown = MinInterval;
		VolumeLinear = Mathf.Clamp(jolt / FullImpact, 0.0f, 1.0f) * MaxVolume;
		Play();
	}

	// Half the size of the prop's collision shapes: roughly how far its
	// edges sit from the middle, used to turn spin into edge speed.
	private float GetLeverArm()
	{
		Aabb? bounds = null;
		foreach (Node child in _prop.GetChildren())
		{
			if (child is CollisionShape3D { Shape: not null } shape)
			{
				Aabb box = shape.Transform * shape.Shape.GetDebugMesh().GetAabb();
				bounds = bounds?.Merge(box) ?? box;
			}
		}

		return bounds.HasValue ? bounds.Value.Size.Length() * 0.5f : 0.5f;
	}
}
