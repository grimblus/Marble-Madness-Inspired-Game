using Godot;

// Per-level countdown. Ticks down from StartSeconds and calls
// GameStateManager.Lose() when it hits zero (requirements.md §3.5). It
// only owns the time; anything that wants to display it (the HUD)
// subscribes to TimeChanged instead of this node knowing about the UI.
// It stops on win or lose, so the clock freezes when the level ends.
public partial class LevelTimer : Node
{
	[Signal] public delegate void TimeChangedEventHandler(float secondsRemaining);

	[Export] public float StartSeconds = 60.0f;

	private float _remaining;
	private bool _running = true;

	public override void _Ready()
	{
		_remaining = StartSeconds;

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

		_remaining = Mathf.Max(0.0f, _remaining - (float)delta);
		EmitSignal(SignalName.TimeChanged, _remaining);

		if (_remaining <= 0.0f)
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
