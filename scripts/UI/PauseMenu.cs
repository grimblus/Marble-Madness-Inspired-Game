using Godot;

// Pause screen (scenes/ui/menus/PauseMenu.tscn). 
// Esc pauses and resumes. The scene runs with Process Mode = Always so it keeps working while the rest of the game is
// frozen, which also freezes the LevelTimer. The level stays visible behind
// a dimmed overlay.
// Pausing is blocked once the level has been won or lost.

public partial class PauseMenu : CanvasLayer
{
	// Scene the Main Menu button loads.
	[Export(PropertyHint.File, "*.tscn")]
	public string MainMenuScenePath { get; set; } = "res://scenes/ui/menus/startMenu.tscn";

	private Button _resumeButton;
	private Button _restartButton;
	private Button _menuButton;
	private GameStateManager _gameState;

	private bool _levelOver;
	private Input.MouseModeEnum _previousMouseMode = Input.MouseModeEnum.Visible;

	public override void _Ready()
	{
		Visible = false;

		_resumeButton = GetNode<Button>("%ResumeButton");
		_restartButton = GetNode<Button>("%RestartButton");
		_menuButton = GetNode<Button>("%MenuButton");

		_resumeButton.Pressed += Resume;
		_restartButton.Pressed += OnRestartPressed;
		_menuButton.Pressed += OnMenuPressed;

		// Don't allow pausing over the win / game over screens.
		_gameState = GetNodeOrNull<GameStateManager>("/root/GameStateManager");
		if (_gameState != null)
		{
			_gameState.LevelWon += OnLevelEnded;
			_gameState.LevelLost += OnLevelEnded;
		}
	}

	// Autoloads outlive a scene reload, so drop the subscriptions with this node.
	public override void _ExitTree()
	{
		if (_gameState != null)
		{
			_gameState.LevelWon -= OnLevelEnded;
			_gameState.LevelLost -= OnLevelEnded;
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Esc by default. If you add a "pause" action in the Input Map, that works too.
		bool pressed = @event.IsActionPressed("ui_cancel")
			|| (InputMap.HasAction("pause") && @event.IsActionPressed("pause"));

		if (!pressed || _levelOver)
		{
			return;
		}

		if (Visible)
		{
			Resume();
		}
		else if (!GetTree().Paused)
		{
			Pause();
		}

		GetViewport().SetInputAsHandled();
	}

	private void Pause()
	{
		// mouse detection
		_previousMouseMode = Input.MouseMode;
		Input.MouseMode = Input.MouseModeEnum.Visible;

		GetTree().Paused = true;
		Visible = true;
		_resumeButton.GrabFocus();
	}

	private void Resume()
	{
		GetTree().Paused = false;
		Visible = false;
		Input.MouseMode = _previousMouseMode;
	}

	private void OnRestartPressed()
	{
		Resume();
		GetTree().ReloadCurrentScene();
	}

	private void OnMenuPressed()
	{
		Resume();
		var error = GetTree().ChangeSceneToFile(MainMenuScenePath);
		if (error != Error.Ok)
		{
			GD.PushError($"PauseMenu: could not load '{MainMenuScenePath}' ({error}). " +
				"Set Main Menu Scene Path on the PauseMenu node.");
		}
	}

	private void OnLevelEnded()
	{
		_levelOver = true;
	}
}
