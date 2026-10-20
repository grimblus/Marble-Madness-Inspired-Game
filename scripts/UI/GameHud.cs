using Godot;

// In-game HUD (scenes/ui/hud/GameHud.tscn): level label top-left, timer
// top-right and a message strip bottom-centre. The strip shows the
// objective when the level starts and "Checkpoint reached" whenever a new
// checkpoint is set. The HUD only listens to signals (LevelTimer's
// TimeChanged, CheckpointManager's CheckpointReached) and never touches the
// timer or checkpoint logic. Set TimerPath to the LevelTimer node.
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

	// Text shown in the same strip when a new checkpoint is reached.
	[Export] public string CheckpointText { get; set; } = "CHECKPOINT REACHED";

	// How long the checkpoint message stays up before fading out, in seconds.
	[Export] public float CheckpointVisibleSeconds { get; set; } = 2.5f;

	// Seconds to fade a message in or out. A message that replaces another
	// fades the old text out and the new text in, each this long.
	private const float SwapSeconds = 0.15f;
	private const float FadeOutSeconds = 1.0f;

	private Label _levelLabel;
	private Label _timeLabel;
	private Label _objectiveLabel;
	private Control _objective;
	private LevelTimer _timer;
	private CheckpointManager _checkpoints;
	private Tween _messageTween;

	public override void _Ready()
	{
		_levelLabel = GetNode<Label>("%LevelLabel");
		_timeLabel = GetNode<Label>("%TimeLabel");
		_objectiveLabel = GetNode<Label>("%ObjectiveLabel");
		_objective = GetNode<Control>("%Objective");

		_levelLabel.Text = LevelLabel;
		_timeLabel.Text = TimeFormat.Clock(0.0f);

		_timer = GetNodeOrNull<LevelTimer>(TimerPath);
		if (_timer != null)
		{
			_timer.TimeChanged += OnTimeChanged;
		}

		_checkpoints = GetNodeOrNull<CheckpointManager>("/root/CheckpointManager");
		if (_checkpoints != null)
		{
			_checkpoints.CheckpointReached += OnCheckpointReached;
		}

		// The objective is already showing when the level appears, so it
		// starts at full opacity rather than fading in.
		_objectiveLabel.Text = ObjectiveText;
		_objective.Modulate = Colors.White;
		ShowMessage(ObjectiveText, ObjectiveVisibleSeconds);
	}

	// Shows text in the bottom strip for `seconds`, then fades it out
	// (0 = keep it up). Only one message is ever shown: a new one takes over
	// the strip straight away, fading out whatever is there, and restarts
	// the timer, so two messages close together never overlap and an older
	// message's timer can't hide a newer one early.
	public void ShowMessage(string text, float seconds)
	{
		_messageTween?.Kill();
		_messageTween = CreateTween();

		bool showingOther = _objective.Modulate.A > 0.01f && _objectiveLabel.Text != text;
		if (showingOther)
		{
			_messageTween.TweenProperty(_objective, "modulate:a", 0.0f, SwapSeconds);
		}
		_messageTween.TweenCallback(Callable.From(() => _objectiveLabel.Text = text));
		_messageTween.TweenProperty(_objective, "modulate:a", 1.0f, SwapSeconds);

		if (seconds > 0.0f)
		{
			_messageTween.TweenInterval(seconds);
			_messageTween.TweenProperty(_objective, "modulate:a", 0.0f, FadeOutSeconds);
		}
	}

	// CheckpointManager is an autoload and outlives a scene reload (Retry),
	// so drop the subscription when the HUD goes away.
	public override void _ExitTree()
	{
		if (_timer != null)
		{
			_timer.TimeChanged -= OnTimeChanged;
		}
		if (_checkpoints != null)
		{
			_checkpoints.CheckpointReached -= OnCheckpointReached;
		}
	}

	private void OnCheckpointReached(Vector3 position)
	{
		ShowMessage(CheckpointText, CheckpointVisibleSeconds);
	}

	private void OnTimeChanged(float seconds)
	{
		_timeLabel.Text = TimeFormat.Clock(seconds);
	}
}
