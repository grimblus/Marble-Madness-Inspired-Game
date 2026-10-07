using Godot;

// HUD readout for the level countdown (scenes/ui/hud/TimerDisplay.tscn).
// Set TimerPath to the LevelTimer node in the scene; it listens to that
// timer's TimeChanged signal and never touches the timer's logic.
public partial class TimerDisplay : CanvasLayer
{
	[Export] public NodePath TimerPath;

	private Label _label;

	public override void _Ready()
	{
		_label = GetNode<Label>("Label");

		var timer = GetNodeOrNull<LevelTimer>(TimerPath);
		if (timer != null)
		{
			timer.TimeChanged += OnTimeChanged;
		}
	}

	private void OnTimeChanged(float secondsElapsed)
	{
		// round down so the display shows whole seconds that have fully passed
		int totalSeconds = Mathf.FloorToInt(secondsElapsed);
		_label.Text = $"{totalSeconds / 60}:{totalSeconds % 60:00}";
	}
}
