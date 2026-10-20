using Godot;

// In-game HUD (scenes/ui/hud/GameHud.tscn): level label top-left, timer
// top-right and objective bottom-centre. It only listens to LevelTimer's
// TimeChanged signal and never touches the timer's logic. Set TimerPath to
// the LevelTimer node.
public partial class GameHud : CanvasLayer
{
	// Defaults to the LevelTimer sibling in Main.tscn.
	[Export] public NodePath TimerPath;

	// Text shown top-left. Change per level.
	[Export] public string LevelLabel { get; set; } = "GALLERY 01";

	// Text shown at the bottom of the screen.
	[Export] public string ObjectiveText { get; set; } = "REACH THE GOAL";

	// How long the objective stays up before fading out. 0 = never fade.
	[Export] public float ObjectiveVisibleSeconds { get; set; } = 6.0f;

	private Label _levelLabel;
	private Label _timeLabel;
	private Label _objectiveLabel;
	private Control _objective;
	private LevelTimer _timer;

	public override void _Ready()
	{
		_levelLabel = GetNode<Label>("%LevelLabel");
		_timeLabel = GetNode<Label>("%TimeLabel");
		_objectiveLabel = GetNode<Label>("%ObjectiveLabel");
		_objective = GetNode<Control>("%Objective");

		_levelLabel.Text = LevelLabel;
		_objectiveLabel.Text = ObjectiveText;
		_timeLabel.Text = TimeFormat.Clock(0.0f);

		_timer = GetNodeOrNull<LevelTimer>(TimerPath);
		if (_timer != null)
		{
			_timer.TimeChanged += OnTimeChanged;
		}

		// Keep the HUD quiet: fade the objective out after a few seconds.
		if (ObjectiveVisibleSeconds > 0.0f)
		{
			var tween = CreateTween();
			tween.TweenInterval(ObjectiveVisibleSeconds);
			tween.TweenProperty(_objective, "modulate:a", 0.0f, 1.0f);
		}
	}

	public override void _ExitTree()
	{
		if (_timer != null)
		{
			_timer.TimeChanged -= OnTimeChanged;
		}
	}

	private void OnTimeChanged(float seconds)
	{
		_timeLabel.Text = TimeFormat.Clock(seconds);
	}
}
