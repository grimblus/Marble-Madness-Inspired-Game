using Godot;

// The player's marble (scenes/marble/Marble.tscn): a RigidBody3D rolled by
// forces from the move input, relative to the camera. Other systems talk to
// it rather than the other way round:
// - hazards call GetHitPhys (knockback), GetHit (slip) and GetHitTramp
//   (bounce), so no hazard needs to know how the marble moves;
// - kill zones call Die, which respawns it at the last checkpoint;
// - it raises Hit, Slipped and Died for sounds and effects, and reads
//   GameStateManager.IsEnded itself to stop once the level is won or lost.
// Its sounds live in child nodes that only read its physics state.
public partial class MarbleController : RigidBody3D
{
	// Rolling force from full input, in newtons.
	[Export] public float MoveForce = 12.0f;

	// Speed, in m/s, above which input can steer and brake but not speed the
	// marble up any further.
	[Export] public float MaxSpeed = 8.0f;

	// Seconds without control after a caryatid knocks the marble back.
	[Export] public float KnockbackSpinoutSeconds = 0.6f;

	// Seconds without control after rolling through a wet floor.
	[Export] public float SlipSpinoutSeconds = 1.0f;

	// Impulse away from a caryatid when it hits the marble, in N·s.
	[Export] public float KnockbackForce = 2.0f;

	// Upward impulse from a trampoline, in N·s.
	[Export] public float TrampolineForce = 6.0f;

	// Raised whenever the marble is sent back to a checkpoint. Listen to
	// this for respawn SFX/UI rather than coupling to whatever triggered
	// the death (a kill zone today, a hazard later).
	[Signal] public delegate void DiedEventHandler();

	// Raised when a caryatid knocks the marble back.
	[Signal] public delegate void HitEventHandler();

	// Raised when the marble rolls through a wet floor and loses control.
	[Signal] public delegate void SlippedEventHandler();

	private GameStateManager _gameState;
	private CheckpointManager _checkpoints;
	private bool _inSpinout;

	// Counts spin-outs, so a timer from an earlier hit can't end a later
	// spin-out early: only the timer of the latest one clears it.
	private int _spinoutCount;

	public override void _Ready()
	{
		AddToGroup("marble");

		// Every fresh load of the marble (level start or retry) begins a
		// new run: clear any leftover checkpoint and fall back to
		// wherever the level placed the marble until a real one is hit.
		_checkpoints = GetNode<CheckpointManager>("/root/CheckpointManager");
		_checkpoints.Reset();
		_checkpoints.SetStartPoint(GlobalPosition);

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
		if (_gameState.IsEnded || _inSpinout)
		{
			return;
		}

		Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 force = GetCameraRelativeDirection(input) * MoveForce;

		// At the speed cap, drop only the part of the push that would add
		// speed. Ignoring input altogether would also stop the player
		// steering or braking after a fast drop.
		Vector3 velocity = LinearVelocity;
		if (velocity.Length() >= MaxSpeed)
		{
			Vector3 heading = velocity.Normalized();
			float speedingUp = force.Dot(heading);
			if (speedingUp > 0.0f)
			{
				force -= heading * speedingUp;
			}
		}

		ApplyCentralForce(force);
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

	// Called by a caryatid on contact: knocks the marble straight away from
	// it and takes control away briefly.
	public void GetHitPhys(Vector3 hazardPosition)
	{
		if (_gameState.IsEnded || _inSpinout)
		{
			return;
		}

		Vector3 pushDirection = GlobalPosition - hazardPosition;
		pushDirection.Y = 0;
		pushDirection = pushDirection.LengthSquared() > 0.0001f
			? pushDirection.Normalized()
			: Vector3.Forward;

		ApplyCentralImpulse(pushDirection * KnockbackForce);
		StartSpinout(KnockbackSpinoutSeconds);
		EmitSignal(SignalName.Hit);
	}

	// Called by a trampoline: launches the marble upwards, with a slight
	// nudge away from the trampoline's centre so it carries on forwards.
	public void GetHitTramp(Vector3 hazardPosition)
	{
		if (_gameState.IsEnded)
		{
			return;
		}

		Vector3 pushDirection = GlobalPosition - hazardPosition;
		pushDirection.Y = 0;
		pushDirection = pushDirection.LengthSquared() > 0.0001f
			? pushDirection.Normalized() * 0.3f
			: Vector3.Zero;

		Vector3 launch = (Vector3.Up + pushDirection).Normalized();

		// Cancel any downward speed first, so every bounce reaches the same
		// height however far the marble fell onto the trampoline.
		Vector3 velocity = LinearVelocity;
		if (velocity.Y < 0)
		{
			velocity.Y = 0;
		}
		LinearVelocity = velocity;

		Sleeping = false;
		ApplyCentralImpulse(launch * TrampolineForce);
	}

	// Called by a wet floor: the marble keeps its speed but can't be steered
	// for a moment.
	public void GetHit()
	{
		if (_gameState.IsEnded || _inSpinout)
		{
			return;
		}

		StartSpinout(SlipSpinoutSeconds);
		EmitSignal(SignalName.Slipped);
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

		GlobalTransform = new Transform3D(Basis.Identity, _checkpoints.GetRespawnPosition());
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;
		Sleeping = false;

		// Cancels any spin-out still running.
		_inSpinout = false;
		_spinoutCount++;

		EmitSignal(SignalName.Died);
	}

	private void StartSpinout(float seconds)
	{
		_inSpinout = true;
		int spinout = ++_spinoutCount;

		// The timer pauses with the game, so a spin-out doesn't run out behind
		// the pause menu.
		GetTree().CreateTimer(seconds, processAlways: false).Timeout += () =>
		{
			if (spinout == _spinoutCount)
			{
				_inSpinout = false;
			}
		};
	}
}
