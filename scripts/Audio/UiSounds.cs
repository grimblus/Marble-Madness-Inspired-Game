using Godot;

// Autoloaded singleton (see project.godot [autoload]) that gives every button
// in the game a click sound. Rather than each menu wiring up audio, it
// watches the scene tree and connects the click to any button as it's added -
// start menu, level select, settings, pause menu, results - including buttons
// added later. Living above the current scene means the click isn't cut off
// when a button changes scene, and processing Always keeps it working while
// the game is paused behind the pause menu.
public partial class UiSounds : Node
{
	private const string ClickPath = "res://assets/audio/sfx/click_003.ogg";

	// Linear volume of the click (1 = the clip's own level).
	private const float ClickVolume = 0.7f;

	// Marks a button that already has the click, so it's never connected twice.
	private static readonly StringName ConnectedMeta = "ui_click_connected";

	private AudioStreamPlayer _click;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		_click = new AudioStreamPlayer
		{
			Stream = GD.Load<AudioStream>(ClickPath),
			VolumeLinear = ClickVolume,
			MaxPolyphony = 2,
		};
		AddChild(_click);

		GetTree().NodeAdded += OnNodeAdded;

		// At launch the main scene (the start menu) enters the tree before
		// this autoload is ready, so its buttons were added before NodeAdded
		// was connected. Catch everything that's already there.
		foreach (Node node in GetTree().Root.FindChildren("*", "BaseButton", true, false))
		{
			ConnectClick((BaseButton)node);
		}
	}

	public override void _ExitTree()
	{
		GetTree().NodeAdded -= OnNodeAdded;
	}

	// Any other script can call this for a click on something that isn't a
	// button (for example a custom control).
	public void PlayClick()
	{
		_click.Play();
	}

	private void OnNodeAdded(Node node)
	{
		// BaseButton covers Button, CheckButton, TextureButton and the rest.
		if (node is BaseButton button)
		{
			ConnectClick(button);
		}
	}

	// The connection is freed along with the button, so nothing leaks.
	private void ConnectClick(BaseButton button)
	{
		if (button.HasMeta(ConnectedMeta))
		{
			return;
		}

		button.SetMeta(ConnectedMeta, true);
		button.Pressed += PlayClick;
	}
}
