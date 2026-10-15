// Remembers which level the player picked, so it survives the scene change from
// the level select (or start menu) into Main.tscn. The LevelLoader in Main reads
// it. Retry reloads Main.tscn, so the same level is loaded again.

public static class GameSession
{
	// The level's scene (e.g. res://scenes/levels/level_02/Level02.tscn).
	public static string LevelScenePath { get; private set; } = "";

	// Used for saving progress (ProgressStore) and shown on the HUD / result screen.
	public static string LevelId { get; private set; } = "";
	public static string LevelLabel { get; private set; } = "";

	public static bool HasSelection => !string.IsNullOrEmpty(LevelScenePath);

	public static void Select(string levelScenePath, string levelId, string levelLabel)
	{
		LevelScenePath = levelScenePath;
		LevelId = levelId;
		LevelLabel = levelLabel;
	}

	// Forget the choice, so Main falls back to its default level (Gallery 01).
	public static void Clear()
	{
		LevelScenePath = "";
		LevelId = "";
		LevelLabel = "";
	}
}
