using Godot;

// Autoloaded singleton (see project.godot [autoload]) tracking the
// marble's current respawn point for this level attempt. Checkpoints,
// kill zones and hazards all read/write this instead of holding direct
// references to each other or to the level.
public partial class CheckpointManager : Node
{
	[Signal] public delegate void CheckpointReachedEventHandler(Vector3 position);

	private Vector3 _respawnPosition = Vector3.Zero;
	private bool _hasCheckpoint = false;

	// Clears the checkpoint. Call this at the start of a level/retry
	// (MarbleController does this in _Ready) so a stale checkpoint from a
	// previous level never carries over.
	public void Reset()
	{
		_hasCheckpoint = false;
		_respawnPosition = Vector3.Zero;
	}

	// Sets the fallback respawn point, but only if no real checkpoint has
	// been reached yet - so falling before the first checkpoint sends the
	// marble back to wherever the level placed it, not to (0, 0, 0).
	public void SetStartPoint(Vector3 position)
	{
		if (!_hasCheckpoint)
		{
			_respawnPosition = position;
		}
	}

	public void SetCheckpoint(Vector3 position)
	{
		_respawnPosition = position;
		_hasCheckpoint = true;
		EmitSignal(SignalName.CheckpointReached, position);
	}

	public Vector3 GetRespawnPosition() => _respawnPosition;
}
