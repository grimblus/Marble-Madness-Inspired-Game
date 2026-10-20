using Godot;

// Win and game over screen (scenes/ui/menus/ResultScreen.tscn).
//
// It listens to GameStateManager.LevelWon / LevelLost and to the LevelTimer's
// TimeChanged signal (so it knows the final time), then shows one of two panels:
//   - Level Complete: 3 stars under ThreeStarSeconds, 2 under TwoStarSeconds,
//     1 under OneStarSeconds.
//   - Game Over: finishing at or after OneStarSeconds, the timer limit running
//     out, or the marble falling.
// The game is paused while the screen is up (it runs with Process Mode = Always).

public partial class ResultScreen : CanvasLayer
{
	// Defaults to the LevelTimer sibling in Main.tscn.
	[Export] public NodePath TimerPath;

	// Name shown at the top of the card. Change per level.
	[Export] public string LevelLabel { get; set; } = "GALLERY 01";

	// Key used to save the best time and stars for this level (ProgressStore).
	[Export] public string LevelId { get; set; } = "level_01";

	[ExportGroup("Star times (seconds)")]
	// Finishing under this time earns three stars.
	[Export] public float ThreeStarSeconds { get; set; } = 30.0f;
	// Finishing under this time earns two stars.
	[Export] public float TwoStarSeconds { get; set; } = 60.0f;
	// Finishing under this time earns one star; at or after it is a game over.
	[Export] public float OneStarSeconds { get; set; } = 300.0f;

	[ExportGroup("Scenes")]
	// Scene the Main Menu buttons load.
	[Export(PropertyHint.File, "*.tscn")]
	public string MainMenuScenePath { get; set; } = "res://scenes/ui/menus/startMenu.tscn";

	// Scene the Level Select button opens; the button stays disabled while this is empty.
	// Next Level and Previous don't need a path: they use the order in LevelCatalog.
	[Export(PropertyHint.File, "*.tscn")] public string LevelSelectScenePath { get; set; } = "";

	private Control _root;
	private PanelContainer _winPanel;
	private PanelContainer _losePanel;

	private Label _winLevelLabel;
	private StarIcon[] _stars;
	private Label _winMessageLabel;
	private Label _winTimeLabel;
	private Label _winBestLabel;
	private PanelContainer[] _chips;   // index 0 = 1 star, 1 = 2 stars, 2 = 3 stars
	private Label[] _chipLabels;
	private Button _nextButton;
	private Button _previousButton;
	private Button _retryButton;
	private Button _levelSelectButton;
	private Button _winMenuButton;

	private Label _loseLevelLabel;
	private Label _reasonLabel;
	private Label _loseMessageLabel;
	private Label _loseTimeLabel;
	private Label _loseBestLabel;
	private Button _tryAgainButton;
	private Button _loseLevelSelectButton;
	private Button _loseMenuButton;

	private LevelTimer _timer;
	private GameStateManager _gameState;
	private float _lastTime;
	private bool _shown;

	public override void _Ready()
	{
		Visible = false;

		_root = GetNode<Control>("%Root");
		_winPanel = GetNode<PanelContainer>("%WinPanel");
		_losePanel = GetNode<PanelContainer>("%LosePanel");

		_winLevelLabel = GetNode<Label>("%WinLevelLabel");
		_stars = new[]
		{
			GetNode<StarIcon>("%Star1"),
			GetNode<StarIcon>("%Star2"),
			GetNode<StarIcon>("%Star3")
		};
		_winMessageLabel = GetNode<Label>("%WinMessageLabel");
		_winTimeLabel = GetNode<Label>("%WinTimeLabel");
		_winBestLabel = GetNode<Label>("%WinBestLabel");
		_chips = new[]
		{
			GetNode<PanelContainer>("%Chip1"),
			GetNode<PanelContainer>("%Chip2"),
			GetNode<PanelContainer>("%Chip3")
		};
		_chipLabels = new[]
		{
			GetNode<Label>("%Chip1Label"),
			GetNode<Label>("%Chip2Label"),
			GetNode<Label>("%Chip3Label")
		};
		_nextButton = GetNode<Button>("%NextButton");
		_previousButton = GetNode<Button>("%PreviousButton");
		_retryButton = GetNode<Button>("%RetryButton");
		_levelSelectButton = GetNode<Button>("%LevelSelectButton");
		_winMenuButton = GetNode<Button>("%WinMenuButton");

		_loseLevelLabel = GetNode<Label>("%LoseLevelLabel");
		_reasonLabel = GetNode<Label>("%ReasonLabel");
		_loseMessageLabel = GetNode<Label>("%LoseMessageLabel");
		_loseTimeLabel = GetNode<Label>("%LoseTimeLabel");
		_loseBestLabel = GetNode<Label>("%LoseBestLabel");
		_tryAgainButton = GetNode<Button>("%TryAgainButton");
		_loseLevelSelectButton = GetNode<Button>("%LoseLevelSelectButton");
		_loseMenuButton = GetNode<Button>("%LoseMenuButton");

		_winLevelLabel.Text = LevelLabel;
		_loseLevelLabel.Text = LevelLabel;

		_nextButton.Pressed += OnNextPressed;
		_previousButton.Pressed += OnPreviousPressed;
		_levelSelectButton.Pressed += () => GoTo(LevelSelectScenePath);
		_loseLevelSelectButton.Pressed += () => GoTo(LevelSelectScenePath);
		_retryButton.Pressed += OnRetryPressed;
		_tryAgainButton.Pressed += OnRetryPressed;
		_winMenuButton.Pressed += OnMenuPressed;
		_loseMenuButton.Pressed += OnMenuPressed;

		// LevelId is set by the LevelLoader before this runs, so the catalog can tell
		// which levels come before and after this one.
		_nextButton.Disabled = LevelCatalog.NextPlayable(LevelId) == null;
		_previousButton.Disabled = LevelCatalog.PreviousPlayable(LevelId) == null;
		_levelSelectButton.Disabled = string.IsNullOrEmpty(LevelSelectScenePath);
		_loseLevelSelectButton.Disabled = string.IsNullOrEmpty(LevelSelectScenePath);

		if (TimerPath != null && !TimerPath.IsEmpty)
		{
			_timer = GetNodeOrNull<LevelTimer>(TimerPath);
		}
		if (_timer != null)
		{
			_timer.TimeChanged += OnTimeChanged;
		}
		else
		{
			GD.PushWarning("ResultScreen: could not find a LevelTimer. Set Timer Path on the ResultScreen node.");
		}

		_gameState = GetNodeOrNull<GameStateManager>("/root/GameStateManager");
		if (_gameState != null)
		{
			_gameState.LevelWon += OnLevelWon;
			_gameState.LevelLost += OnLevelLost;
		}
	}

	// Autoloads outlive a scene reload (Retry), so drop the subscriptions with this node.
	public override void _ExitTree()
	{
		if (_timer != null)
		{
			_timer.TimeChanged -= OnTimeChanged;
		}
		if (_gameState != null)
		{
			_gameState.LevelWon -= OnLevelWon;
			_gameState.LevelLost -= OnLevelLost;
		}
	}

	private void OnTimeChanged(float seconds)
	{
		_lastTime = seconds;
	}

	// ---------- Outcomes ----------

	private void OnLevelWon()
	{
		if (_shown)
		{
			return;
		}

		// Too slow: finishing at or after the last star time counts as a game over.
		if (_lastTime >= OneStarSeconds)
		{
			ShowGameOver(true);
			return;
		}

		ShowWin();
	}

	private void OnLevelLost()
	{
		if (_shown)
		{
			return;
		}

		ShowGameOver(false);
	}

	private void ShowWin()
	{
		int stars = _lastTime < ThreeStarSeconds ? 3 : (_lastTime < TwoStarSeconds ? 2 : 1);

		float? previousBest = ProgressStore.GetBestTime(LevelId);
		bool newBest = !previousBest.HasValue || _lastTime < previousBest.Value;
		ProgressStore.Record(LevelId, _lastTime, stars);

		for (int i = 0; i < _stars.Length; i++)
		{
			_stars[i].Filled = i < stars;
		}

		_winMessageLabel.Text = stars == 3 ? "A flawless roll."
			: (stars == 2 ? "Nicely rolled." : "Gallery cleared.");
		_winTimeLabel.Text = TimeFormat.Clock(_lastTime);
		_winBestLabel.Text = newBest ? "NEW BEST" : "BEST " + TimeFormat.Clock(previousBest.Value);

		// Chips show the star times, with the earned tier highlighted.
		SetChip(0, "1 STAR", "UNDER " + TimeFormat.Short(OneStarSeconds), stars == 1);
		SetChip(1, "2 STARS", "UNDER " + TimeFormat.Short(TwoStarSeconds), stars == 2);
		SetChip(2, "3 STARS", "UNDER " + TimeFormat.Short(ThreeStarSeconds), stars == 3);

		Reveal(_winPanel);

		// Earned stars pop in one after another, then keep shining.
		for (int i = 0; i < stars; i++)
		{
			_stars[i].PlayShine(0.4f + 0.3f * i);
		}

		(_nextButton.Disabled ? _retryButton : _nextButton).GrabFocus();
	}

	private void ShowGameOver(bool finishedTooSlow)
	{
		// A time limit on the LevelTimer ending the run also counts as "time expired".
		float limit = _timer?.TimeLimitSeconds ?? 0.0f;
		bool expired = finishedTooSlow || (limit > 0.0f && _lastTime >= limit - 0.05f);

		float? best = ProgressStore.GetBestTime(LevelId);

		_reasonLabel.Text = expired ? "TIMER EXPIRES" : "MARBLE LOST";
		_loseMessageLabel.Text = expired
			? "The gallery closed before you reached the goal."
			: "The marble rolled off the edge.";
		_loseTimeLabel.Text = TimeFormat.Clock(_lastTime);
		_loseBestLabel.Text = best.HasValue ? "BEST " + TimeFormat.Clock(best.Value) : "BEST --:--.--";

		Reveal(_losePanel);
		_tryAgainButton.GrabFocus();
	}

	private void Reveal(PanelContainer panel)
	{
		_shown = true;
		_winPanel.Visible = panel == _winPanel;
		_losePanel.Visible = panel == _losePanel;

		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;
		Visible = true;

		// Soft fade-in. This tween runs while paused because the node is Process Mode = Always.
		_root.Modulate = new Color(1, 1, 1, 0);
		CreateTween().TweenProperty(_root, "modulate:a", 1.0f, 0.5f);
	}

	// ---------- Buttons ----------

	private void OnRetryPressed()
	{
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	private void OnNextPressed()
	{
		GoToLevel(LevelCatalog.NextPlayable(LevelId));
	}

	private void OnPreviousPressed()
	{
		GoToLevel(LevelCatalog.PreviousPlayable(LevelId));
	}

	// Picks the level, then reloads Main so the LevelLoader loads it.
	private void GoToLevel(LevelCatalog.LevelEntry level)
	{
		if (level == null)
		{
			return;
		}

		GameSession.Select(level.ScenePath, level.Id, level.Label);
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	private void OnMenuPressed()
	{
		GoTo(MainMenuScenePath);
	}

	private void GoTo(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}

		GetTree().Paused = false;
		var error = GetTree().ChangeSceneToFile(path);
		if (error != Error.Ok)
		{
			GD.PushError($"ResultScreen: could not load '{path}' ({error}).");
		}
	}

	// ---------- Helpers ----------

	private void SetChip(int index, string title, string sub, bool active)
	{
		_chipLabels[index].Text = title + "\n" + sub;

		var style = new StyleBoxFlat
		{
			BgColor = active ? new Color(0.72f, 0.6f, 0.39f, 0.18f) : new Color(1, 1, 1, 0.0f),
			BorderColor = active ? new Color("B89A63") : new Color("DDD5C5")
		};
		style.SetBorderWidthAll(1);
		style.SetCornerRadiusAll(10);
		style.SetContentMarginAll(8.0f);
		_chips[index].AddThemeStyleboxOverride("panel", style);
	}
}
