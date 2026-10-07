using Godot;

// Per-level stopwatch. Counts up from 0 and reports the elapsed time through
// TimeChanged. It only owns the time; anything that wants to display it (the
// HUD) subscribes to TimeChanged instead of this node knowing about the UI.
// It stops on win or lose, so the clock freezes when the level ends.
// Optional: set TimeLimitSeconds above 0 to call GameStateManager.Lose()
// when that time is reached (0 = no limit).

public partial class LevelTimer : Node
{
	[Signal] public delegate void TimeChangedEventHandler(float secondsElapsed);

	[Export] public float TimeLimitSeconds = 0.0f;

	private float _elapsed;
	private bool _running = true;

	public override void _Ready()
	{
		_elapsed = 0.0f;
		EmitSignal(SignalName.TimeChanged, _elapsed);

		var gameState = GetNode<GameStateManager>("/root/GameStateManager");
		gameState.LevelWon += Stop;
		gameState.LevelLost += Stop;
	}

	// Autoloads outlive a scene reload (Retry), so drop these subscriptions
	// when the timer goes away rather than leaving stale handlers behind.
	public override void _ExitTree()
	{
		var gameState = GetNodeOrNull<GameStateManager>("/root/GameStateManager");
		if (gameState != null)
		{
			gameState.LevelWon -= Stop;
			gameState.LevelLost -= Stop;
		}
	}

	public override void _Process(double delta)
	{
		if (!_running)
		{
			return;
		}

		_elapsed += (float)delta;
		EmitSignal(SignalName.TimeChanged, _elapsed);

		if (TimeLimitSeconds > 0.0f && _elapsed >= TimeLimitSeconds)
		{
			_running = false;
			GetNode<GameStateManager>("/root/GameStateManager").Lose();
		}
	}

	private void Stop()
	{
		_running = false;
	}
}
