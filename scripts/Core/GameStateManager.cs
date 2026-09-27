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

	// Raised by LevelTimer when time runs out (requirements.md §3.5); any
	// future lose condition (e.g. a hazard) can call Lose() the same way.
	[Signal] public delegate void LevelLostEventHandler();

	private bool _hasEnded = false;

	// True once the level has been won or lost. Systems that must stop
	// acting after the end (marble input, respawning) should check this
	// directly instead of caching their own copy from the signals, so they
	// can never be out of step with it.
	public bool IsEnded => _hasEnded;

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

	// Clears the ended state so a fresh attempt can win or lose again.
	public void Reset()
	{
		_hasEnded = false;
	}

	// Restarts the current level from scratch. Reloading the scene recreates
	// the marble (which resets its checkpoint), the timer and the HUD, so
	// this only has to clear the win/lose state first. Any screen offering a
	// retry (game over, later the win screen or a pause menu) calls this.
	public void Retry()
	{
		Reset();
		GetTree().ReloadCurrentScene();
	}
}
