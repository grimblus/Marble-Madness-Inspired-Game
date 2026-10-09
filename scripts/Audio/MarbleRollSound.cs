using Godot;

// Looping rolling sound for the marble. Lives as a child of the marble and
// only reads its parent's physics state, so MarbleController knows nothing
// about audio. The loop plays continuously; only its volume and pitch
// change - louder and higher the faster the marble rolls, silent while
// airborne or still - which avoids the clicks of starting/stopping it.
// Non-positional on purpose: the camera always follows the marble, and a
// 3D player would get quieter when the camera zooms out.
public partial class MarbleRollSound : AudioStreamPlayer
{
	// Speed (m/s) at which the roll reaches full volume and MaxPitch.
	// Matches MarbleController.MaxSpeed by default.
	[Export] public float FullVolumeSpeed = 8.0f;

	// Linear volume at full speed. Above 1 boosts a quietly recorded clip
	// (2 = +6 dB); watch for clipping on loud peaks.
	[Export] public float MaxVolume = 1.0f;
	[Export] public float MinPitch = 0.8f;
	[Export] public float MaxPitch = 1.3f;

	// How quickly volume/pitch follow the marble. High enough to cut out
	// promptly on a bounce, low enough not to flutter on small bumps.
	[Export] public float Responsiveness = 15.0f;

	// Below this fraction of FullVolumeSpeed the marble counts as still.
	private const float SilentBelow = 0.03f;

	// Extra reach of the ground check beyond the marble's radius, so slopes
	// and tiny hops still count as rolling.
	private const float GroundCheckMargin = 0.15f;

	private RigidBody3D _marble;
	private float _radius;
	private float _volume;

	public override void _Ready()
	{
		_marble = GetParent<RigidBody3D>();
		_radius = MarbleGroundCheck.GetRadius(_marble);

		VolumeLinear = 0.0f;
		Play();
	}

	public override void _PhysicsProcess(double delta)
	{
		// Only the speed along the surface counts as rolling: the downward
		// speed of a landing shouldn't make a burst of rolling sound, but
		// rolling down a ramp still should.
		float speed = 0.0f;
		if (MarbleGroundCheck.TryGetGroundNormal(_marble, _radius + GroundCheckMargin, out Vector3 normal))
		{
			Vector3 velocity = _marble.LinearVelocity;
			speed = (velocity - normal * velocity.Dot(normal)).Length();
		}

		float speedFraction = Mathf.Clamp(speed / FullVolumeSpeed, 0.0f, 1.0f);
		if (speedFraction < SilentBelow)
		{
			speedFraction = 0.0f;
		}

		// Same frame-rate-independent smoothing as the camera follow.
		float t = 1.0f - Mathf.Exp(-Responsiveness * (float)delta);
		_volume = Mathf.Lerp(_volume, speedFraction * MaxVolume, t);

		VolumeLinear = _volume;
		PitchScale = Mathf.Lerp(MinPitch, MaxPitch, speedFraction);
	}

}
