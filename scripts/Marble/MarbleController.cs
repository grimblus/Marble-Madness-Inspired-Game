using Godot;

public partial class MarbleController : RigidBody3D
{
	[Export] public float MoveForce = 12.0f;
	[Export] public float MaxSpeed = 8.0f;

	// Raised whenever the marble is sent back to a checkpoint. Listen to
	// this for respawn SFX/UI rather than coupling to whatever triggered
	// the death (a kill zone today, a hazard later).
	[Signal] public delegate void DiedEventHandler();

	public override void _Ready()
	{
		AddToGroup("marble");

		// Every fresh load of the marble (level start or retry) begins a
		// new run: clear any leftover checkpoint and fall back to
		// wherever the level placed the marble until a real one is hit.
		var checkpoints = GetNode<CheckpointManager>("/root/CheckpointManager");
		checkpoints.Reset();
		checkpoints.SetStartPoint(GlobalPosition);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 force = new Vector3(input.X, 0, input.Y) * MoveForce;

		if (LinearVelocity.Length() < MaxSpeed)
		{
			ApplyCentralForce(force);
		}
	}

	// Sends the marble back to its last checkpoint. A kill zone calls
	// this directly; a future hazard can call it the same way, or via
	// body.Call("Die") if it only holds a Node3D reference.
	public void Die()
	{
		var checkpoints = GetNode<CheckpointManager>("/root/CheckpointManager");
		Vector3 respawnPosition = checkpoints.GetRespawnPosition();

		GlobalTransform = new Transform3D(Basis.Identity, respawnPosition);
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;
		Sleeping = false;

		EmitSignal(SignalName.Died);
	}
}
