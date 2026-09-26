using Godot;

// Trigger volume for a level's goal (scenes/triggers/Goal.tscn). Reaching
// it wins the level - same Area3D-on-the-Triggers-layer pattern as
// Checkpoint and KillZone.
public partial class Goal : Area3D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (!body.IsInGroup("marble"))
		{
			return;
		}

		GetNode<GameStateManager>("/root/GameStateManager").Win();
	}
}
