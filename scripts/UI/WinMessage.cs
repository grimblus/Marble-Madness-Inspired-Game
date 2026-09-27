using Godot;

// Minimal placeholder "you win" message (scenes/ui/menus/WinMessage.tscn).
// Shows itself when the level is won. This is a stand-in for the real
// victory screen (stats, next-level/retry buttons - requirements.md §3.7);
// it only needs GameStateManager's LevelWon signal, so it can be swapped
// out without touching how the win is triggered.
public partial class WinMessage : CanvasLayer
{
	public override void _Ready()
	{
		Visible = false;
		GetNode<GameStateManager>("/root/GameStateManager").LevelWon += OnLevelWon;
	}

	// Autoloads outlive a scene reload (Retry), so drop this subscription
	// when the message goes away rather than leaving a stale handler behind.
	public override void _ExitTree()
	{
		var gameState = GetNodeOrNull<GameStateManager>("/root/GameStateManager");
		if (gameState != null)
		{
			gameState.LevelWon -= OnLevelWon;
		}
	}

	private void OnLevelWon()
	{
		Visible = true;
	}
}
