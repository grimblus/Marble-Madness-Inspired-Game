using Godot;

public partial class MarbleController : RigidBody3D
{
	[Export] public float MoveForce = 12.0f;
	[Export] public float MaxSpeed = 8.0f;
	//just the default value, the real value is in each gethit method
	[Export] public float SpinoutTime = 0.0f;
	[Export] public float KnockbackForce = 2.0f;

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

		// The win/lose flag lives on an autoload, so it survives the scene
		// change from a result screen (retry, level select, menu). Clear it
		// here too, or the next level starts already "ended" and frozen.
		_gameState = GetNode<GameStateManager>("/root/GameStateManager");
		_gameState.Reset();
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
		Vector3 force = GetCameraRelativeDirection(input) * MoveForce;

		if (LinearVelocity.Length() < MaxSpeed)
		{
			ApplyCentralForce(force);
		}
	}
	
	// Maps input onto the ground plane relative to the active camera, so W
	// rolls the marble up the screen rather than along world -Z (which reads
	// as diagonal under the rig's 45-degree isometric yaw). The camera's
	// forward/right are flattened to XZ so its downward pitch doesn't push
	// the marble into the floor. Falls back to world axes with no camera,
	// e.g. when the marble scene is run on its own.
	private Vector3 GetCameraRelativeDirection(Vector2 input)
	{
		Camera3D camera = GetViewport().GetCamera3D();
		if (camera == null)
		{
			return new Vector3(input.X, 0, input.Y);
		}

		Basis basis = camera.GlobalBasis;
		Vector3 forward = new Vector3(-basis.Z.X, 0, -basis.Z.Z).Normalized();
		Vector3 right = new Vector3(basis.X.X, 0, basis.X.Z).Normalized();

		// input.Y is negative for move_forward, hence the subtraction.
		return right * input.X - forward * input.Y;
	}

	public void GetHitPhys(Vector3 hazardPosition)
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
		
		//Gets pushed directly away when touching physical hazard
		Vector3 pushDirection = GlobalPosition - hazardPosition;
		pushDirection.Y = 0;
		if (pushDirection.LengthSquared() > 0.0001f)
		{
			pushDirection = pushDirection.Normalized();
		}
		else
		{
			pushDirection = Vector3.Forward;
		}
		ApplyCentralImpulse(pushDirection * KnockbackForce);
		
		SpinoutTime = 0.6f;
		
		GetTree().CreateTimer(SpinoutTime).Timeout += () =>
		{
			_inSpinout = false;
		};
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
		
		SpinoutTime = 1.0f;
		
		GetTree().CreateTimer(SpinoutTime).Timeout += () =>
		{
			_inSpinout = false;
		};
	}
	
	private void HurtboxAreaHit(Area3D area) {
		if (area.IsInGroup("enemies") && area.IsInGroup("stationary"))
		{
			GetHit();
		}
		else if (area.IsInGroup("enemies"))
		{
			GetHitPhys(area.GlobalPosition);
		}
	}
	
	private void HurtboxBodyHit(Node3D body) {
		if (body.IsInGroup("enemies") && body.IsInGroup("stationary"))
		{
			GetHit();
		}
		else if (body.IsInGroup("enemies"))
		{
			GetHitPhys(body.GlobalPosition);
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
