using Godot;

// Minimal placeholder game-over screen (scenes/ui/menus/GameOverMessage.tscn).
// Shows itself when the level is lost, with a retry button. Stand-in for
// the real game-over screen (requirements.md §3.7); it only needs
// GameStateManager's LevelLost signal and Retry(), so it can be swapped
// out without touching how the loss is triggered.
public partial class GameOverMessage : CanvasLayer
{
	private Button _retryButton;

	public override void _Ready()
	{
		Visible = false;

		_retryButton = GetNode<Button>("Control/RetryButton");
		_retryButton.Pressed += OnRetryPressed;

		GetNode<GameStateManager>("/root/GameStateManager").LevelLost += OnLevelLost;
	}

	// Autoloads outlive a scene reload (Retry), so drop this subscription
	// when the screen goes away rather than leaving a stale handler behind.
	public override void _ExitTree()
	{
		var gameState = GetNodeOrNull<GameStateManager>("/root/GameStateManager");
		if (gameState != null)
		{
			gameState.LevelLost -= OnLevelLost;
		}
	}

	private void OnLevelLost()
	{
		Visible = true;

		// Focus the button so Enter/Space also retries, not just the mouse.
		_retryButton.GrabFocus();
	}

	private void OnRetryPressed()
	{
		GetNode<GameStateManager>("/root/GameStateManager").Retry();
	}
}
