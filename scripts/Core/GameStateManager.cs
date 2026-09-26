using Godot;

// Autoloaded singleton (see project.godot [autoload]) for whole-level
// state that isn't owned by any one system - currently just win/lose.
// Anything that can end the level (a goal trigger today, a timer or a
// hazard later) calls Win()/Lose() here; listeners (UI, marble controls,
// audio) subscribe to the signals instead of being called directly, so
// none of these systems need to know about each other.
public partial class GameStateManager : Node
{
	[Signal] public delegate void LevelWonEventHandler();

	// Not raised by anything yet - reserved for the timer's lose condition
	// (requirements.md §3.5) so that system can plug in without touching
	// this class again.
	[Signal] public delegate void LevelLostEventHandler();

	private bool _hasEnded = false;

	public void Win()
	{
		if (_hasEnded)
		{
			return;
		}

		_hasEnded = true;
		EmitSignal(SignalName.LevelWon);
	}

	public void Lose()
	{
		if (_hasEnded)
		{
			return;
		}

		_hasEnded = true;
		EmitSignal(SignalName.LevelLost);
	}

	// Levels aren't reloaded/transitioned yet; once they are, whatever
	// loads a fresh attempt should call this so it can end again.
	public void Reset()
	{
		_hasEnded = false;
	}
}
