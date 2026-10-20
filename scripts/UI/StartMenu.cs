using Godot;

// Start menu (scenes/ui/menus/startMenu.tscn).
// - Start Game / Level Select / How to Play / Settings / Quit
// - Slowly crossfades through the images in Backgrounds, if any are set
//   (one screenshot per level); otherwise the cream fallback colour shows.

public partial class StartMenu : Control
{
	// Scene the Start Game button loads.
	[Export(PropertyHint.File, "*.tscn")]
	public string GameScenePath { get; set; } = "res://scenes/main/Main.tscn";

	// Scene the Level Select button opens.
	[Export(PropertyHint.File, "*.tscn")]
	public string LevelSelectScenePath { get; set; } = "res://scenes/ui/menus/LevelSelect.tscn";

	// Tick this to wipe saved stars and times each time the game launches (for testing).
	[Export] public bool ResetProgressOnLaunch { get; set; } = false;

	[ExportGroup("Level Backgrounds")]
	// Drag one screenshot per level into this array in the Inspector.
	[Export] public Texture2D[] Backgrounds { get; set; }
	// How long each background stays up before fading to the next.
	[Export] public float SecondsPerBackground { get; set; } = 6.0f;
	// Length of the crossfade between backgrounds.
	[Export] public float FadeSeconds { get; set; } = 1.5f;
	// Slow zoom applied to each background (0 = none, 0.06 = 6%).
	[Export] public float ZoomAmount { get; set; } = 0.06f;

	private TextureRect _bgA;
	private TextureRect _bgB;
	private PanelContainer _mainPanel;
	private PanelContainer _howToPanel;
	private PanelContainer _settingsPanel;
	private Button _startButton;
	private Button _levelSelectButton;
	private Button _howToButton;
	private Button _settingsButton;
	private Button _quitButton;
	private Button _howToBackButton;
	private Button _settingsBackButton;
	private HSlider _volumeSlider;

	// B sits on top of A. When true, B is the visible layer.
	private bool _bTop;
	private int _bgIndex;

	public override void _Ready()
	{
		// Wipes saved progress once per game launch (not every time the menu loads).
		if (ResetProgressOnLaunch)
		{
			ProgressStore.ResetOncePerLaunch();
		}

		// Returning from a win/lose screen: make sure nothing is left paused
		// and the cursor is usable.
		GetTree().Paused = false;
		Input.MouseMode = Input.MouseModeEnum.Visible;

		_bgA = GetNode<TextureRect>("%BackgroundA");
		_bgB = GetNode<TextureRect>("%BackgroundB");
		_mainPanel = GetNode<PanelContainer>("%MainPanel");
		_howToPanel = GetNode<PanelContainer>("%HowToPanel");
		_settingsPanel = GetNode<PanelContainer>("%SettingsPanel");
		_startButton = GetNode<Button>("%StartButton");
		_levelSelectButton = GetNode<Button>("%LevelSelectButton");
		_howToButton = GetNode<Button>("%HowToButton");
		_settingsButton = GetNode<Button>("%SettingsButton");
		_quitButton = GetNode<Button>("%QuitButton");
		_howToBackButton = GetNode<Button>("%HowToBackButton");
		_settingsBackButton = GetNode<Button>("%SettingsBackButton");
		_volumeSlider = GetNode<HSlider>("%VolumeSlider");

		_startButton.Pressed += OnStartPressed;
		_levelSelectButton.Pressed += OnLevelSelectPressed;
		_quitButton.Pressed += () => GetTree().Quit();
		_howToButton.Pressed += () => ShowPanel(_howToPanel, _howToBackButton);
		_settingsButton.Pressed += () => ShowPanel(_settingsPanel, _settingsBackButton);
		_howToBackButton.Pressed += () => ShowPanel(_mainPanel, _howToButton);
		_settingsBackButton.Pressed += () => ShowPanel(_mainPanel, _settingsButton);

		InitSettings();
		InitBackgrounds();

		_startButton.GrabFocus();
	}

	// Escape / B button goes back from How to Play and Settings.
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel") && !_mainPanel.Visible)
		{
			ShowPanel(_mainPanel, _startButton);
			GetViewport().SetInputAsHandled();
		}
	}

	// navigation

	private void OnStartPressed()
	{
		GameSession.Clear();
		var error = GetTree().ChangeSceneToFile(GameScenePath);
		if (error != Error.Ok)
		{
			GD.PushError($"StartMenu: could not load '{GameScenePath}' ({error}). " +
				"Set Game Scene Path on the StartMenu node.");
		}
	}

	private void OnLevelSelectPressed()
	{
		var error = GetTree().ChangeSceneToFile(LevelSelectScenePath);
		if (error != Error.Ok)
		{
			GD.PushError($"StartMenu: could not load '{LevelSelectScenePath}' ({error}). " +
				"Set Level Select Scene Path on the StartMenu node.");
		}
	}

	private void ShowPanel(PanelContainer panel, Button focusTarget)
	{
		_mainPanel.Visible = panel == _mainPanel;
		_howToPanel.Visible = panel == _howToPanel;
		_settingsPanel.Visible = panel == _settingsPanel;
		focusTarget.GrabFocus();
	}

	// Settings area

	private void InitSettings()
	{
		// Set the starting value first, then connect, so setup doesn't fire the handler.
		_volumeSlider.Value = Mathf.Sqrt(Mathf.DbToLinear(AudioServer.GetBusVolumeDb(0)));
		_volumeSlider.ValueChanged += OnVolumeChanged;
	}

	// The slider is squared before it becomes a volume, because loudness isn't
	// linear: with the raw value, the top half of the slider only spanned -6 dB
	// and sounded like it did nothing. Squared, halfway is about -12 dB.
	private void OnVolumeChanged(double value)
	{
		float v = (float)value;
		AudioServer.SetBusMute(0, v <= 0.001f);
		AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(Mathf.Max(v * v, 0.000001f)));
	}

	// Levels and background slideshow

	private void InitBackgrounds()
	{
		// Zoom from the middle of the screen, and keep the pivot right if the window resizes.
		_bgA.Resized += () => _bgA.PivotOffset = _bgA.Size / 2;
		_bgB.Resized += () => _bgB.PivotOffset = _bgB.Size / 2;
		_bgA.PivotOffset = _bgA.Size / 2;
		_bgB.PivotOffset = _bgB.Size / 2;

		// No images assigned: the cream Fallback colour shows instead.
		if (Backgrounds == null || Backgrounds.Length == 0)
		{
			return;
		}

		_bgA.Texture = Backgrounds[0];
		ZoomIn(_bgA);

		if (Backgrounds.Length < 2)
		{
			return;
		}

		var timer = new Timer
		{
			WaitTime = SecondsPerBackground,
			OneShot = false,
			Autostart = true
		};
		timer.Timeout += NextBackground;
		AddChild(timer);
	}

	private void NextBackground()
	{
		_bgIndex = (_bgIndex + 1) % Backgrounds.Length;
		var fade = CreateTween();

		if (_bTop)
		{
			// B is showing: load the next image on A (hidden underneath), fade B away.
			_bgA.Texture = Backgrounds[_bgIndex];
			ZoomIn(_bgA);
			fade.TweenProperty(_bgB, "modulate:a", 0.0f, FadeSeconds);
		}
		else
		{
			// A is showing: load the next image on B and fade it in on top.
			_bgB.Texture = Backgrounds[_bgIndex];
			ZoomIn(_bgB);
			fade.TweenProperty(_bgB, "modulate:a", 1.0f, FadeSeconds);
		}

		_bTop = !_bTop;
	}

	private void ZoomIn(TextureRect rect)
	{
		rect.Scale = Vector2.One;
		if (ZoomAmount <= 0.0f)
		{
			return;
		}

		CreateTween().TweenProperty(
			rect, "scale",
			Vector2.One * (1.0f + ZoomAmount),
			SecondsPerBackground + FadeSeconds * 2.0f);
	}
}
