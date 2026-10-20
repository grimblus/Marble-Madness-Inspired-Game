using Godot;

// Loads the level the player picked into Main.tscn. Main is the shared "shell"
// (marble, camera, HUD, pause menu, result screen, timer); each level scene
// holds only its own tiles, goal, checkpoints, kill zone and hazards.
//
// Setup in Main.tscn:
//  - Add this as a Node3D child of Main and drag it to the TOP of Main's
//    children, so its _Ready runs before the marble, HUD and result screen read
//    their settings.
//  - Each level scene needs a Marker3D named PlayerStart where the marble begins.
//
// If nothing was picked (for example you run Main.tscn directly), it loads the
// default level below.

public partial class LevelLoader : Node3D
{
	// Level loaded when nothing was picked, with its save id and HUD label
	// (see LevelCatalog for the real values).
	[Export(PropertyHint.File, "*.tscn")]
	public string DefaultLevelScenePath { get; set; } = "res://scenes/levels/level_01/Level01.tscn";
	[Export] public string DefaultLevelId { get; set; } = "level_01";
	[Export] public string DefaultLevelLabel { get; set; } = "GALLERY 01";

	// Paths are relative to this node (a child of Main).
	[Export] public NodePath MarblePath { get; set; } = "../Marble";
	[Export] public NodePath ResultScreenPath { get; set; } = "../ResultScreen";
	[Export] public NodePath HudPath { get; set; } = "../GameHud";

	// Name of the marker inside each level where the marble starts.
	[Export] public string StartNodeName { get; set; } = "PlayerStart";

	public override void _Ready()
	{
		bool picked = GameSession.HasSelection;
		string path = picked ? GameSession.LevelScenePath : DefaultLevelScenePath;
		string id = picked ? GameSession.LevelId : DefaultLevelId;
		string label = picked ? GameSession.LevelLabel : DefaultLevelLabel;

		var scene = GD.Load<PackedScene>(path);
		if (scene == null)
		{
			GD.PushError($"LevelLoader: could not load level '{path}'.");
			return;
		}

		// Tell the result screen and HUD which level this is.
		var result = Find<ResultScreen>(ResultScreenPath);
		if (result != null)
		{
			result.LevelId = id;
			result.LevelLabel = label;
			WarnIfAlreadyReady(result);
		}

		var hud = Find<GameHud>(HudPath);
		if (hud != null)
		{
			hud.LevelLabel = label;
			WarnIfAlreadyReady(hud);
		}

		Node level = scene.Instantiate();
		AddChild(level);

		PlaceMarble(level);
	}

	private void PlaceMarble(Node level)
	{
		var start = level.FindChild(StartNodeName, true, false) as Node3D;
		if (start == null)
		{
			GD.PushWarning($"LevelLoader: the level has no '{StartNodeName}' marker, so the marble keeps its position in Main.");
			return;
		}

		var marble = Find<Node3D>(MarblePath);
		if (marble == null)
		{
			GD.PushWarning("LevelLoader: could not find the Marble. Check Marble Path.");
			return;
		}

		marble.GlobalPosition = start.GlobalPosition;
		if (marble is RigidBody3D body)
		{
			body.LinearVelocity = Vector3.Zero;
			body.AngularVelocity = Vector3.Zero;
		}
	}

	private T Find<T>(NodePath path) where T : Node
	{
		if (path == null || path.IsEmpty)
		{
			return null;
		}
		return GetNodeOrNull<T>(path);
	}

	private static void WarnIfAlreadyReady(Node node)
	{
		if (node.IsNodeReady())
		{
			GD.PushWarning($"LevelLoader: '{node.Name}' was set up before the LevelLoader, so its level name may be wrong. Drag the LevelLoader to the top of Main's children.");
		}
	}
}
