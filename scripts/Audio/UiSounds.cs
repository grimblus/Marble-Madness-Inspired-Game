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

	private AudioStreamPlayer _click;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		_click = new AudioStreamPlayer
		{
			Stream = GD.Load<AudioStream>(ClickPath),
			MaxPolyphony = 2,
		};
		AddChild(_click);

		GetTree().NodeAdded += OnNodeAdded;
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
		// The connection is freed along with the button, so nothing leaks.
		if (node is BaseButton button)
		{
			button.Pressed += PlayClick;
		}
	}
}
