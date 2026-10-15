using Godot;

// Level select screen (scenes/ui/menus/LevelSelect.tscn): one level card in the
// middle, switched with the < and > buttons, the dots, or the Left / Right keys.
//
// To add a level, add a line to Levels below. Leave the scene path empty for a
// placeholder: shows as "Coming soon" until we add a real scene.
// Best times and stars come from ProgressStore, using each level's Id (the same
// LevelId you set on the ResultScreen in that level).

public partial class LevelSelect : Control
{
	private sealed record LevelEntry(string Id, string Label, string ScenePath);

	private static readonly LevelEntry[] Levels =
	{
		// Each path is the level's own scene (tiles, goal, hazards), not Main.tscn.
		// Main.tscn is the shared scene the chosen level gets loaded into.
		new("level_01", "GALLERY 01", "res://scenes/levels/level_01/Level01.tscn"),
		// Check this path: right-click your playable level 2 scene > Copy Path.
		new("level_02", "GALLERY 02", "res://scenes/levels/level_02/Level02.tscn"),
		// Work in progress: add the scene path here when level 3 is ready.
		new("level_03", "GALLERY 03", "")
	};

	// Fake progress used while ShowSampleProgress is on, so you can check how
	// every state looks before the real levels exist.
	private static readonly int[] SampleStars = { 3, 2, 0 };
	private static readonly float?[] SampleTimes = { 28.40f, 52.10f, null };

	[Export(PropertyHint.File, "*.tscn")]
	public string MainMenuScenePath { get; set; } = "res://scenes/ui/menus/startMenu.tscn";

	// The shared scene every level is played in (marble, camera, HUD).
	[Export(PropertyHint.File, "*.tscn")]
	public string GameScenePath { get; set; } = "res://scenes/main/Main.tscn";

	// Off = show your real saved stars and times. Turn on to preview fake progress.
	[Export] public bool ShowSampleProgress { get; set; } = false;

	// Optional: one screenshot per level, in the same order as Levels.
	// Levels without one show the stylised placeholder picture.
	[Export] public Texture2D[] Screenshots { get; set; }

	private LevelPreview _preview;
	private PanelContainer _card;
	private Label _titleLabel;
	private Control _unlockedBox;
	private Control _lockedBox;
	private StarIcon[] _stars;
	private Label _bestLabel;
	private Label _requirementLabel;
	private Button _playButton;
	private Button _prevButton;
	private Button _nextButton;
	private Button _backButton;
	private HBoxContainer _dotsRow;
	private Label _counterLabel;
	private Panel[] _dots;

	private int _index;

	public override void _Ready()
	{
		GetTree().Paused = false;
		Input.MouseMode = Input.MouseModeEnum.Visible;

		_preview = GetNode<LevelPreview>("%Preview");
		_card = GetNode<PanelContainer>("%Card");
		_titleLabel = GetNode<Label>("%TitleLabel");
		_unlockedBox = GetNode<Control>("%UnlockedBox");
		_lockedBox = GetNode<Control>("%LockedBox");
		_stars = new[]
		{
			GetNode<StarIcon>("%StarA"),
			GetNode<StarIcon>("%StarB"),
			GetNode<StarIcon>("%StarC")
		};
		_bestLabel = GetNode<Label>("%BestLabel");
		_requirementLabel = GetNode<Label>("%RequirementLabel");
		_playButton = GetNode<Button>("%PlayButton");
		_prevButton = GetNode<Button>("%PrevButton");
		_nextButton = GetNode<Button>("%NextButton");
		_backButton = GetNode<Button>("%BackButton");
		_dotsRow = GetNode<HBoxContainer>("%Dots");
		_counterLabel = GetNode<Label>("%CounterLabel");

		_prevButton.Pressed += () => Select(_index - 1);
		_nextButton.Pressed += () => Select(_index + 1);
		_playButton.Pressed += OnPlayPressed;
		_backButton.Pressed += GoBack;

		BuildDots();
		Refresh(false);

		if (!_playButton.Disabled)
		{
			_playButton.GrabFocus();
		}
	}

	// Left / Right switch levels and Esc goes back. This uses _Input (not
	// _UnhandledInput) so the keys work even while a button has focus.
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_left"))
		{
			Select(_index - 1);
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_right"))
		{
			Select(_index + 1);
			GetViewport().SetInputAsHandled();
		}
		else if (@event.IsActionPressed("ui_cancel"))
		{
			GoBack();
			GetViewport().SetInputAsHandled();
		}
	}

	// progress bars

	private int GetStars(int index)
	{
		return ShowSampleProgress ? SampleStars[index] : ProgressStore.GetBestStars(Levels[index].Id);
	}

	private float? GetBestTime(int index)
	{
		return ShowSampleProgress ? SampleTimes[index] : ProgressStore.GetBestTime(Levels[index].Id);
	}

	// The first level is always open; each later one opens when the one before it has at least 1 star.
	private bool IsUnlocked(int index)
	{
		return index == 0 || GetStars(index - 1) > 0;
	}

	// navigation

	private void Select(int index)
	{
		index = Mathf.Clamp(index, 0, Levels.Length - 1);
		if (index == _index)
		{
			return;
		}

		_index = index;
		Refresh(true);
	}

	private void OnPlayPressed()
	{
		LevelEntry level = Levels[_index];
		if (!IsUnlocked(_index) || string.IsNullOrEmpty(level.ScenePath))
		{
			return;
		}

		// Remember the choice, then open the shared scene. The LevelLoader in
		// Main.tscn reads GameSession and loads this level into it.
		GameSession.Select(level.ScenePath, level.Id, level.Label);
		var error = GetTree().ChangeSceneToFile(GameScenePath);
		if (error != Error.Ok)
		{
			GD.PushError($"LevelSelect: could not load '{GameScenePath}' ({error}).");
		}
	}

	private void GoBack()
	{
		var error = GetTree().ChangeSceneToFile(MainMenuScenePath);
		if (error != Error.Ok)
		{
			GD.PushError($"LevelSelect: could not load '{MainMenuScenePath}' ({error}).");
		}
	}

	// display

	private void Refresh(bool animate)
	{
		LevelEntry level = Levels[_index];
		bool unlocked = IsUnlocked(_index);
		bool playable = !string.IsNullOrEmpty(level.ScenePath);
		int stars = GetStars(_index);
		float? best = GetBestTime(_index);

		_titleLabel.Text = level.Label;

		_preview.Variant = _index;
		_preview.Locked = !unlocked;
		_preview.Screenshot = (Screenshots != null && _index < Screenshots.Length) ? Screenshots[_index] : null;

		_unlockedBox.Visible = unlocked;
		_lockedBox.Visible = !unlocked;

		for (int i = 0; i < _stars.Length; i++)
		{
			_stars[i].Filled = i < stars;
		}
		_bestLabel.Text = best.HasValue ? "BEST " + FormatTime(best.Value) : "BEST --:--.--";
		_requirementLabel.Text = _index > 0 ? "Complete " + Levels[_index - 1].Label + " to unlock" : "";

		if (!unlocked)
		{
			_playButton.Text = "LOCKED";
			_playButton.Disabled = true;
		}
		else if (!playable)
		{
			_playButton.Text = "COMING SOON";
			_playButton.Disabled = true;
		}
		else
		{
			_playButton.Text = stars > 0 ? "PLAY AGAIN" : "PLAY";
			_playButton.Disabled = false;
		}

		_prevButton.Disabled = _index == 0;
		_nextButton.Disabled = _index == Levels.Length - 1;

		UpdateDots();
		UpdateCounter();

		if (animate)
		{
			_card.Modulate = new Color(1, 1, 1, 0);
			CreateTween().TweenProperty(_card, "modulate:a", 1.0f, 0.22f);
		}
	}

	// One small clickable dot per level.
	private void BuildDots()
	{
		_dots = new Panel[Levels.Length];
		for (int i = 0; i < Levels.Length; i++)
		{
			int index = i;
			var dot = new Panel
			{
				CustomMinimumSize = new Vector2(8, 8),
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
				MouseDefaultCursorShape = Control.CursorShape.PointingHand,
				TooltipText = Levels[i].Label
			};
			dot.GuiInput += (InputEvent e) =>
			{
				if (e is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
				{
					Select(index);
				}
			};

			_dots[i] = dot;
			_dotsRow.AddChild(dot);
		}
	}

	private void UpdateDots()
	{
		for (int i = 0; i < _dots.Length; i++)
		{
			bool active = i == _index;

			var style = new StyleBoxFlat { BgColor = active ? new Color("B89A63") : new Color("C4BBA8") };
			style.SetCornerRadiusAll(4);

			_dots[i].AddThemeStyleboxOverride("panel", style);
			_dots[i].CustomMinimumSize = new Vector2(active ? 22 : 8, 8);
		}
	}

	private void UpdateCounter()
	{
		int total = 0;
		for (int i = 0; i < Levels.Length; i++)
		{
			total += GetStars(i);
		}
		_counterLabel.Text = total + " / " + (Levels.Length * 3);
	}

	// mm:ss.cc, e.g. 00:27.48
	private static string FormatTime(float seconds)
	{
		int centis = Mathf.FloorToInt(Mathf.Max(0.0f, seconds) * 100.0f);
		int minutes = centis / 6000;
		int secs = centis / 100 % 60;
		int hundredths = centis % 100;
		return $"{minutes:00}:{secs:00}.{hundredths:00}";
	}
}
