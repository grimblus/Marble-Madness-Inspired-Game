using Godot;

// Trigger volume for a checkpoint (scenes/triggers/Checkpoint.tscn).
// When the marble passes through, its respawn point updates to this
// node's position. Place these along a level's route.
public partial class Checkpoint : Area3D
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

		GetNode<CheckpointManager>("/root/CheckpointManager").SetCheckpoint(GlobalPosition);
	}
}
