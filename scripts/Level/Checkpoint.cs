using Godot;

// Trigger volume for a checkpoint (scenes/triggers/Checkpoint.tscn).
// When the marble passes through, its respawn point updates to this
// node's position. Place these along a level's route.
//
// Resize a checkpoint with Size, not by scaling the node: scaling an Area3D
// unevenly stretches its collision shape, which Godot warns about and
// physics doesn't handle reliably. Runs in the editor too ([Tool]), so the
// box you see while placing a checkpoint matches the one used in game.
[Tool]
public partial class Checkpoint : Area3D
{
	private Vector3 _size = new Vector3(2.0f, 3.0f, 0.5f);

	// Width, height and depth of the trigger box (and its marker), in metres.
	[Export]
	public Vector3 Size
	{
		get => _size;
		set
		{
			_size = value;
			ApplySize();
		}
	}

	public override void _Ready()
	{
		ApplySize();

		if (Engine.IsEditorHint())
		{
			return;
		}

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

	// Gives this checkpoint its own box at Size, for both the trigger shape
	// and the marker. Copies are needed because every checkpoint instance
	// would otherwise share one shape resource from Checkpoint.tscn.
	private void ApplySize()
	{
		if (!IsInsideTree())
		{
			return;
		}

		var shape = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
		if (shape != null && (shape.Shape is not BoxShape3D box || box.Size != _size))
		{
			shape.Shape = new BoxShape3D { Size = _size };
		}

		var marker = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
		if (marker?.Mesh is BoxMesh mesh && mesh.Size != _size)
		{
			var copy = (BoxMesh)mesh.Duplicate();
			copy.Size = _size;
			marker.Mesh = copy;
		}
	}
}
