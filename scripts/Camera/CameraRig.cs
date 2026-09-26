using Godot;

// Follows a target (the marble) at a fixed angle and distance, so the view
// stays isometric/fixed-angle while still centering on the marble.
// The rig only ever moves its *position* toward the target - it never rotates -
// so the Camera3D child, offset once in _Ready, keeps a constant viewing angle.
public partial class CameraRig : Node3D
{
	[Export] public NodePath TargetPath;

	// Angle looking down at the marble. -35.264 is the "true" isometric angle;
	// -30 reads as the flatter, classic isometric-game look.
	[Export] public float PitchDegrees = -35.264f;

	// Which side the camera views from, rotated around the vertical axis.
	[Export] public float YawDegrees = 45.0f;

	// How far back the camera sits from the tracked point.
	// Under Perspective projection this also controls apparent zoom;
	// under Orthogonal it only affects near/far clipping - use
	// OrthographicSize for zoom in that mode instead.
	[Export] public float Distance = 12.0f;

	// Vertical extent of the view, in world units, when the child Camera3D's
	// Projection is set to Orthogonal. Smaller = more zoomed in. Has no
	// effect under Perspective projection.
	[Export] public float OrthographicSize = 10.0f;

	// Higher = snappier follow, lower = smoother/laggier. This is
	// framerate-independent exponential smoothing, not a plain per-frame lerp.
	[Export] public float FollowSpeed = 8.0f;

	private Node3D _target;
	private Camera3D _camera;

	public override void _Ready()
	{
		_target = GetNodeOrNull<Node3D>(TargetPath);
		_camera = GetNode<Camera3D>("Camera3D");

		if (_target != null)
		{
			GlobalPosition = _target.GlobalPosition;
		}

		UpdateCameraOffset();
	}

	// Recomputes the camera's local offset/rotation from Pitch/Yaw/Distance.
	// Called once in _Ready; call again at runtime if you expose these as
	// live-tunable (e.g. from a debug menu).
	private void UpdateCameraOffset()
	{
		Vector3 direction = new Vector3(0, 0, 1)
			.Rotated(Vector3.Right, Mathf.DegToRad(PitchDegrees))
			.Rotated(Vector3.Up, Mathf.DegToRad(YawDegrees));

		_camera.Position = direction * Distance;
		_camera.LookAt(GlobalPosition, Vector3.Up);
		_camera.Size = OrthographicSize; // no-op when Projection is Perspective
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_target == null)
		{
			return;
		}

		// Exponential smoothing: frame-rate independent, unlike Lerp(pos, target, fixedAlpha).
		float t = 1.0f - Mathf.Exp(-FollowSpeed * (float)delta);
		GlobalPosition = GlobalPosition.Lerp(_target.GlobalPosition, t);
	}
}
