using Godot;

public partial class StartMenu : Control
{
	// scene that the play button goes to
	[Export(PropertyHint.File, "*.tscn")]
	public string GameScenePath { get; set; } = "res://scenes/main/main.tscn";

	private Button _playButton;
	private Button _quitButton;

	public override void _Ready()
	{
		// Coming back from a win/lose screen
		// and the cursor is usable on the menu.
		GetTree().Paused = false;
		Input.MouseMode = Input.MouseModeEnum.Visible;

		_playButton = GetNode<Button>("%PlayButton");
		_quitButton = GetNode<Button>("%QuitButton");

		_playButton.Pressed += OnPlayPressed;
		_quitButton.Pressed += OnQuitPressed;

		// Keyboard / gamepad friendly
		_playButton.GrabFocus();
	}

	private void OnPlayPressed()
	{
		var error = GetTree().ChangeSceneToFile(GameScenePath);
		if (error != Error.Ok)
		{
			GD.PushError($"StartMenu: could not load '{GameScenePath}' ({error}). " +
				"Set GameScenePath on the StartMenu node.");
		}
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
