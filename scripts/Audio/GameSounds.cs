using Godot;

// Sounds for game events (Main.tscn → GameSounds): reaching a checkpoint,
// winning, losing, a caryatid knocking the marble back and slipping on a
// wet floor. Each sound is a child
// AudioStreamPlayer, so its clip and volume are set in the Inspector.
//
// It only listens to signals that already exist for these events, so the
// checkpoint, goal, timer, hazards and marble contain no audio code:
// - CheckpointManager.CheckpointReached
// - GameStateManager.LevelWon / LevelLost
// - MarbleController.Hit / Slipped
//
// Respawning after a fall is deliberately silent: the marble reappearing
// and the camera sweeping back to it already make it obvious, and a sound
// on top felt jarring.
//
// Processing is Always: the result screen pauses the game the moment the
// level is won or lost, and the jingle has to play through that pause.
public partial class GameSounds : Node
{
	// The marble whose hits and slips are heard.
	[Export] public NodePath MarblePath = "../Marble";

	private AudioStreamPlayer _checkpoint;
	private AudioStreamPlayer _win;
	private AudioStreamPlayer _lose;
	private AudioStreamPlayer _hit;
	private AudioStreamPlayer _slip;

	private CheckpointManager _checkpoints;
	private GameStateManager _gameState;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		_checkpoint = GetNode<AudioStreamPlayer>("Checkpoint");
		_win = GetNode<AudioStreamPlayer>("Win");
		_lose = GetNode<AudioStreamPlayer>("Lose");
		_hit = GetNode<AudioStreamPlayer>("Hit");
		_slip = GetNode<AudioStreamPlayer>("Slip");

		_checkpoints = GetNode<CheckpointManager>("/root/CheckpointManager");
		_checkpoints.CheckpointReached += OnCheckpointReached;

		_gameState = GetNode<GameStateManager>("/root/GameStateManager");
		_gameState.LevelWon += OnLevelWon;
		_gameState.LevelLost += OnLevelLost;

		// The marble is freed together with this node, so its connections
		// don't need removing.
		var marble = GetNodeOrNull<MarbleController>(MarblePath);
		if (marble != null)
		{
			marble.Hit += OnHit;
			marble.Slipped += OnSlipped;
		}
		else
		{
			GD.PushWarning("GameSounds: could not find the marble. Set Marble Path on the GameSounds node.");
		}
	}

	// Autoloads outlive a scene reload (Retry), so drop these subscriptions
	// when this node goes away rather than leaving stale handlers behind.
	public override void _ExitTree()
	{
		_checkpoints.CheckpointReached -= OnCheckpointReached;
		_gameState.LevelWon -= OnLevelWon;
		_gameState.LevelLost -= OnLevelLost;
	}

	private void OnCheckpointReached(Vector3 position) => _checkpoint.Play();
	private void OnLevelWon() => _win.Play();
	private void OnLevelLost() => _lose.Play();
	private void OnHit() => _hit.Play();
	private void OnSlipped() => _slip.Play();
}
