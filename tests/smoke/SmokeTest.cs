using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

// Headless smoke test of the running game (see docs/testing.md). Run it with:
//   godot --headless --path . res://tests/smoke/SmokeTest.tscn
//
// For every level in LevelCatalog it loads the real Main shell, the same way
// the level select does, and checks that:
// - the level has a start marker, a goal, a kill zone and at least one checkpoint;
// - the marble starts at the start marker and the level isn't already over;
// - passing a checkpoint moves the respawn point there;
// - falling into the kill zone sends the marble back to that checkpoint;
// - reaching the goal wins the level and shows the result screen.
// It then opens the start menu and level select. Any error or warning Godot
// logs along the way also fails the test. It quits with exit code 0 if every
// check passed and 1 otherwise, so CI can use it directly.
//
// The marble is moved by teleporting it, not by rolling it, so this tests
// that the level is wired up, not that its route can be completed.
public partial class SmokeTest : Node
{
	private const string MainScenePath = "res://scenes/main/Main.tscn";
	private const string ProgressPath = "user://progress.cfg";

	private static readonly string[] MenuScenePaths =
	{
		"res://scenes/ui/menus/startMenu.tscn",
		"res://scenes/ui/menus/LevelSelect.tscn"
	};

	private readonly List<string> _failures = new();
	private LogWatcher _log;
	private byte[] _savedProgress;

	public override async void _Ready()
	{
		// Keep running while the result screen pauses the game.
		ProcessMode = ProcessModeEnum.Always;

		_log = new LogWatcher();
		OS.AddLogger(_log);
		BackUpProgress();

		// The root is still adding its children during this _Ready, so wait a
		// frame before adding scenes to it.
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		foreach (LevelCatalog.LevelEntry level in LevelCatalog.Levels)
		{
			if (level.IsPlayable)
			{
				await TestLevel(level);
			}
		}

		foreach (string path in MenuScenePaths)
		{
			await TestMenu(path);
		}

		GameSession.Clear();
		RestoreProgress();
		OS.RemoveLogger(_log);

		if (_failures.Count == 0)
		{
			GD.Print("SMOKE TEST PASSED");
			GetTree().Quit(0);
			return;
		}

		GD.Print($"SMOKE TEST FAILED ({_failures.Count}):");
		foreach (string failure in _failures)
		{
			GD.Print("  - " + failure);
		}
		GetTree().Quit(1);
	}

	private async Task TestLevel(LevelCatalog.LevelEntry level)
	{
		string name = level.Id;
		GD.Print($"Testing {name}");

		// Load the level into Main as the level select would.
		GameSession.Select(level.ScenePath, level.Id, level.Label);
		Node main = GD.Load<PackedScene>(MainScenePath).Instantiate();
		GetTree().Root.AddChild(main);
		await WaitPhysicsFrames(5);

		var marble = main.GetNode<MarbleController>("Marble");
		var resultScreen = main.GetNode<CanvasLayer>("ResultScreen");
		var gameState = GetNode<GameStateManager>("/root/GameStateManager");
		var checkpoints = GetNode<CheckpointManager>("/root/CheckpointManager");
		Node levelRoot = main.GetNode("LevelLoader").GetChildOrNull<Node>(0);

		if (!Check(levelRoot != null, name, "the level scene was loaded into Main"))
		{
			ReportLogErrors(name);
			await Unload(main);
			return;
		}

		var start = levelRoot.FindChild("PlayerStart", true, false) as Node3D;
		List<Goal> goals = FindAll<Goal>(levelRoot);
		List<KillZone> killZones = FindAll<KillZone>(levelRoot);
		List<Checkpoint> checkpointNodes = FindAll<Checkpoint>(levelRoot);

		Check(start != null, name, "has a PlayerStart marker");
		Check(goals.Count > 0, name, "has a goal");
		Check(killZones.Count > 0, name, "has a kill zone");
		Check(checkpointNodes.Count > 0, name, "has at least one checkpoint");
		Check(!gameState.IsEnded, name, "is not already over when it starts");
		if (start != null)
		{
			Check(marble.GlobalPosition.DistanceTo(start.GlobalPosition) < 1.0f, name,
				"the marble starts at PlayerStart");
		}

		// Passing a checkpoint moves the respawn point to it.
		Checkpoint checkpoint = checkpointNodes.Count > 0 ? checkpointNodes[0] : null;
		if (checkpoint != null)
		{
			Teleport(marble, checkpoint.GlobalPosition);
			await WaitPhysicsFrames(3);
			Check(checkpoints.GetRespawnPosition().IsEqualApprox(checkpoint.GlobalPosition), name,
				"passing a checkpoint sets the respawn point");
		}

		// Falling into the kill zone respawns the marble at that checkpoint.
		if (killZones.Count > 0)
		{
			bool died = false;
			marble.Died += () => died = true;

			Teleport(marble, killZones[0].GlobalPosition);
			await WaitPhysicsFrames(3);

			Check(died, name, "falling into the kill zone kills the marble");
			Check(marble.GlobalPosition.DistanceTo(checkpoints.GetRespawnPosition()) < 1.0f, name,
				"the marble respawns at the last checkpoint");
			Check(!gameState.IsEnded, name, "a fall doesn't end the level");
		}

		// Reaching the goal wins and shows the result screen. The goal's
		// trigger box sits 0.25 m above its origin.
		if (goals.Count > 0)
		{
			Teleport(marble, goals[0].GlobalPosition + Vector3.Up * 0.25f);
			await WaitPhysicsFrames(3);

			Check(gameState.IsEnded, name, "reaching the goal ends the level");
			Check(resultScreen.Visible, name, "the result screen is shown after a win");
		}

		ReportLogErrors(name);
		await Unload(main);
	}

	private async Task TestMenu(string path)
	{
		GD.Print($"Testing {path}");

		var scene = GD.Load<PackedScene>(path);
		if (!Check(scene != null, path, "the scene loads"))
		{
			return;
		}

		Node menu = scene.Instantiate();
		GetTree().Root.AddChild(menu);
		await WaitPhysicsFrames(10);

		ReportLogErrors(path);
		await Unload(menu);
	}

	private async Task Unload(Node node)
	{
		GetTree().Paused = false;
		node.QueueFree();
		await WaitPhysicsFrames(2);
	}

	// Records a failure if the condition is false, and returns the condition.
	private bool Check(bool condition, string context, string description)
	{
		if (!condition)
		{
			_failures.Add($"{context}: {description}");
			GD.Print($"  FAIL {description}");
		}
		return condition;
	}

	// Turns any errors or warnings Godot logged since the last call into failures.
	private void ReportLogErrors(string context)
	{
		foreach (string message in _log.TakeMessages())
		{
			_failures.Add($"{context}: Godot logged \"{message}\"");
			GD.Print($"  FAIL Godot logged \"{message}\"");
		}
	}

	private async Task WaitPhysicsFrames(int count)
	{
		for (int i = 0; i < count; i++)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		}
	}

	private static void Teleport(RigidBody3D body, Vector3 position)
	{
		body.GlobalPosition = position;
		body.LinearVelocity = Vector3.Zero;
		body.AngularVelocity = Vector3.Zero;
		body.Sleeping = false;
	}

	private static List<T> FindAll<T>(Node root) where T : Node
	{
		var found = new List<T>();
		foreach (Node child in root.GetChildren())
		{
			if (child is T match)
			{
				found.Add(match);
			}
			found.AddRange(FindAll<T>(child));
		}
		return found;
	}

	// The smoke test wins every level, which would save a best time to the
	// player's real progress file. Keep a copy and put it back afterwards.
	private void BackUpProgress()
	{
		_savedProgress = FileAccess.FileExists(ProgressPath)
			? FileAccess.GetFileAsBytes(ProgressPath)
			: null;
	}

	private void RestoreProgress()
	{
		if (_savedProgress == null)
		{
			if (FileAccess.FileExists(ProgressPath))
			{
				DirAccess.RemoveAbsolute(ProgressPath);
			}
			return;
		}

		using var file = FileAccess.Open(ProgressPath, FileAccess.ModeFlags.Write);
		file.StoreBuffer(_savedProgress);
	}

	// Collects every error and warning Godot logs, from any thread.
	private partial class LogWatcher : Logger
	{
		private readonly object _lock = new();
		private readonly List<string> _messages = new();

		public override void _LogError(string function, string file, int line, string code,
			string rationale, bool editorNotify, int errorType,
			Godot.Collections.Array<ScriptBacktrace> scriptBacktraces)
		{
			string kind = errorType == (int)ErrorType.Warning ? "WARNING" : "ERROR";
			string text = string.IsNullOrEmpty(rationale) ? code : rationale;
			lock (_lock)
			{
				_messages.Add($"{kind}: {text} ({file}:{line})");
			}
		}

		public List<string> TakeMessages()
		{
			lock (_lock)
			{
				var taken = new List<string>(_messages);
				_messages.Clear();
				return taken;
			}
		}
	}
}
