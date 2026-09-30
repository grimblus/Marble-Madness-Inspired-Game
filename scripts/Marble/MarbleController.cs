using Godot;

public partial class MarbleController : RigidBody3D
{
	[Export] public float MoveForce = 12.0f;
	[Export] public float MaxSpeed = 8.0f;
	[Export] public float SpinoutTime = 2.0f;

	// Raised whenever the marble is sent back to a checkpoint. Listen to
	// this for respawn SFX/UI rather than coupling to whatever triggered
	// the death (a kill zone today, a hazard later).
	[Signal] public delegate void DiedEventHandler();

	private GameStateManager _gameState;
	private bool _inSpinout = false;

	public override void _Ready()
	{
		AddToGroup("marble");

		// Every fresh load of the marble (level start or retry) begins a
		// new run: clear any leftover checkpoint and fall back to
		// wherever the level placed the marble until a real one is hit.
		var checkpoints = GetNode<CheckpointManager>("/root/CheckpointManager");
		checkpoints.Reset();
		checkpoints.SetStartPoint(GlobalPosition);

		_gameState = GetNode<GameStateManager>("/root/GameStateManager");
	}

	public override void _PhysicsProcess(double delta)
	{
		// Stop responding to input once the level has ended, win or lose,
		// so the marble doesn't keep rolling around behind a win/lose screen.
		if (_gameState.IsEnded)
		{
			return;
		}

		if (_inSpinout) 
		{
			return;
		}
		
		Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 force = new Vector3(input.X, 0, input.Y) * MoveForce;

		if (LinearVelocity.Length() < MaxSpeed)
		{
			ApplyCentralForce(force);
		}
	}
	
	public void GetHit()
	{
		if (_gameState.IsEnded) 
		{
			return;
		}
		
		if (_inSpinout) 
		{
			return;
		}
		
		_inSpinout = true;
		
		GetTree().CreateTimer(SpinoutTime).Timeout += () =>
		{
			_inSpinout = false;
		};
	}
	
	private void HurtboxAreaHit(Area3D area) {
		if (area.IsInGroup("enemies")) {
			GetHit();
		}
	}
	
	private void HurtboxBodyHit(Node3D body) {
		if (body.IsInGroup("enemies")) {
			GetHit();
		}
	}

	// Sends the marble back to its last checkpoint. A kill zone calls
	// this directly; a future hazard can call it the same way, or via
	// body.Call("Die") if it only holds a Node3D reference.
	public void Die()
	{
		// Once the level has ended (win or lose) the marble stays put.
		// Without this, a fall at the moment time runs out respawns the
		// marble behind the game over screen as if the run were still live.
		if (_gameState.IsEnded)
		{
			return;
		}

		var checkpoints = GetNode<CheckpointManager>("/root/CheckpointManager");
		Vector3 respawnPosition = checkpoints.GetRespawnPosition();

		GlobalTransform = new Transform3D(Basis.Identity, respawnPosition);
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;
		Sleeping = false;
		_inSpinout = false;

		EmitSignal(SignalName.Died);
	}
}
