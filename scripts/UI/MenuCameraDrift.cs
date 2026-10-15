using Godot;

// Gives the menu's background camera a slow, gentle sway so the 3D backdrop
// feels alive. Place and aim the Camera3D yourself in the editor; this only
// moves it a little around where you put it.

public partial class MenuCameraDrift : Camera3D
{
	// How far the camera drifts sideways, in metres. 0 = completely still.
	[Export] public float SwayDistance { get; set; } = 0.6f;

	// Seconds for one full back-and-forth sway.
	[Export] public float SwaySeconds { get; set; } = 14.0f;

	private Vector3 _basePosition;
	private float _time;

	public override void _Ready()
	{
		_basePosition = Position;
	}

	public override void _Process(double delta)
	{
		if (SwayDistance <= 0.0f || SwaySeconds <= 0.0f)
		{
			return;
		}

		_time += (float)delta;
		float phase = _time / SwaySeconds * Mathf.Tau;

		// Mostly sideways, with a little up and down.
		Position = _basePosition + new Vector3(
			Mathf.Sin(phase) * SwayDistance,
			Mathf.Sin(phase * 0.5f) * SwayDistance * 0.25f,
			0.0f);
	}
}
