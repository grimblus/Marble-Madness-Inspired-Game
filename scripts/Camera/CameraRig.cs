using Godot;

// Follows a target (the marble) at a fixed angle and distance, so the view
// stays isometric/fixed-angle while still centering on the marble.
// The rig only ever moves its *position* toward the target - it never rotates -
// so the Camera3D child keeps a constant viewing angle.
// The player can step through the zoom levels (three zoomed in, regular,
// zoomed out) with the zoom_in / zoom_out actions; the camera eases between them.
public partial class CameraRig : Node3D
{
	[Export] public NodePath TargetPath;

	// Angle looking down at the marble. -35.264 is the "true" isometric angle;
	// -30 reads as the flatter, classic isometric-game look.
	[Export] public float PitchDegrees = -35.264f;

	// Which side the camera views from, rotated around the vertical axis.
	[Export] public float YawDegrees = 45.0f;

	// How far back the camera sits from the tracked point at the regular
	// zoom level. Under Perspective projection this also controls apparent
	// zoom; under Orthogonal it only affects near/far clipping - use
	// OrthographicSize for zoom in that mode instead.
	[Export] public float Distance = 12.0f;

	// Vertical extent of the view, in world units, at the regular zoom level
	// when the child Camera3D's Projection is set to Orthogonal. Smaller =
	// more zoomed in. Has no effect under Perspective projection.
	[Export] public float OrthographicSize = 10.0f;

	// Orthographic sizes for the zoomed in levels, ordered from the first
	// step in (closest to OrthographicSize) to the closest zoom.
	[Export] public float[] ZoomedInSizes = { 10.0f, 7.0f, 5.0f };

	// Orthographic size for the single zoomed out level.
	[Export] public float ZoomedOutSize = 20.0f;

	// How quickly the camera eases to a new zoom level. Same smoothing as
	// FollowSpeed: higher = snappier.
	[Export] public float ZoomSpeed = 10.0f;

	// Higher = snappier follow, lower = smoother/laggier. This is
	// framerate-independent exponential smoothing, not a plain per-frame lerp.
	[Export] public float FollowSpeed = 8.0f;

	private Node3D _target;
	private Camera3D _camera;
	// All zoom levels, most zoomed in first: the zoomed in sizes (reversed),
	// then the regular size, then the zoomed out size.
	private float[] _zoomSizes;
	private int _zoomIndex;
	private float _currentSize;

	public override void _Ready()
	{
		_target = GetNodeOrNull<Node3D>(TargetPath);
		_camera = GetNode<Camera3D>("Camera3D");

		if (_target != null)
		{
			GlobalPosition = _target.GlobalPosition;
		}

		int zoomedInCount = ZoomedInSizes.Length;
		_zoomSizes = new float[zoomedInCount + 2];
		for (int i = 0; i < zoomedInCount; i++)
		{
			_zoomSizes[i] = ZoomedInSizes[zoomedInCount - 1 - i];
		}
		_zoomSizes[zoomedInCount] = OrthographicSize;
		_zoomSizes[zoomedInCount + 1] = ZoomedOutSize;

		_zoomIndex = zoomedInCount; // start at the regular level
		_currentSize = OrthographicSize;
		UpdateCameraOffset();
	}

	// Recomputes the camera's local offset/rotation from Pitch/Yaw/Distance
	// and the current zoom. The distance scales with the zoom so the framing
	// keeps the same proportions at every level: zoomed out, the camera pulls
	// back far enough that the near edge of the screen isn't clipped; zoomed
	// in, it moves closer, keeping directional shadows sharp.
	private void UpdateCameraOffset()
	{
		Vector3 direction = new Vector3(0, 0, 1)
			.Rotated(Vector3.Right, Mathf.DegToRad(PitchDegrees))
			.Rotated(Vector3.Up, Mathf.DegToRad(YawDegrees));

		float zoomScale = _currentSize / OrthographicSize;

		_camera.Position = direction * Distance * zoomScale;
		_camera.LookAt(GlobalPosition, Vector3.Up);
		_camera.Size = _currentSize; // no-op when Projection is Perspective
	}

	// Unhandled so scrolling over UI (menus, buttons) doesn't also zoom.
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("zoom_in"))
		{
			_zoomIndex = Mathf.Max(_zoomIndex - 1, 0);
		}
		else if (@event.IsActionPressed("zoom_out"))
		{
			_zoomIndex = Mathf.Min(_zoomIndex + 1, _zoomSizes.Length - 1);
		}
	}

	public override void _Process(double delta)
	{
		float targetSize = _zoomSizes[_zoomIndex];
		if (Mathf.IsEqualApprox(_currentSize, targetSize))
		{
			return;
		}

		float t = 1.0f - Mathf.Exp(-ZoomSpeed * (float)delta);
		_currentSize = Mathf.Lerp(_currentSize, targetSize, t);
		if (Mathf.Abs(_currentSize - targetSize) < 0.01f)
		{
			_currentSize = targetSize;
		}

		UpdateCameraOffset();
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
