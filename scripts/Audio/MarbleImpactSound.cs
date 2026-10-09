using Godot;

// "Clunk" when the marble lands. Lives as a child of the marble and only
// reads its parent's physics state, like MarbleRollSound. A landing is the
// moment the marble goes from airborne to touching the ground, and its
// loudness comes from how fast the marble was moving *into* the surface.
// Rolling over ramp edges or tile seams never leaves the ground, so it
// stays silent; each bounce after a drop plays its own, quieter clunk.
public partial class MarbleImpactSound : AudioStreamPlayer
{
	// Landings slower than this (m/s into the ground) are silent, so tiny
	// hops over seams don't click.
	[Export] public float MinImpactSpeed = 1.0f;

	// Impact speed at which the clunk reaches MaxVolume.
	[Export] public float FullImpactSpeed = 6.0f;

	// Linear volume of the hardest landings (1 = the clip's own level).
	[Export] public float MaxVolume = 1.0f;

	// Random pitch spread per clunk (0.08 = +/-8%), so repeated bounces
	// don't sound identical.
	[Export] public float PitchVariation = 0.08f;

	// Smaller than the rolling sound's margin so the clunk lines up with
	// the actual moment of contact rather than just before it.
	private const float GroundCheckMargin = 0.05f;

	private RigidBody3D _marble;
	private float _radius;
	private bool _wasOnGround = true;
	private Vector3 _previousVelocity;

	public override void _Ready()
	{
		_marble = GetParent<RigidBody3D>();
		_radius = MarbleGroundCheck.GetRadius(_marble);

		// A respawn zeroes the marble's velocity in one step, which would
		// otherwise read as a hard landing at the checkpoint.
		if (_marble.HasSignal("Died"))
		{
			_marble.Connect("Died", Callable.From(() => _previousVelocity = Vector3.Zero));
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		bool onGround = MarbleGroundCheck.TryGetGroundNormal(_marble, _radius + GroundCheckMargin, out Vector3 normal);

		if (onGround && !_wasOnGround)
		{
			float impactSpeed = -_previousVelocity.Dot(normal);
			if (impactSpeed >= MinImpactSpeed)
			{
				PlayImpact(impactSpeed);
			}
		}

		_wasOnGround = onGround;
		_previousVelocity = _marble.LinearVelocity;
	}

	private void PlayImpact(float impactSpeed)
	{
		VolumeLinear = Mathf.Clamp(impactSpeed / FullImpactSpeed, 0.0f, 1.0f) * MaxVolume;
		PitchScale = 1.0f + (float)GD.RandRange(-PitchVariation, PitchVariation);
		Play();
	}
}
