// The list of levels, in play order. The level select screen shows them and the
// result screen uses the same list to find the next / previous level.
//
// To add a level, add a line to Levels. Leave the scene path empty for a level
// that isn't ready yet: it shows as "Coming soon" and Next Level skips to nothing.

public static class LevelCatalog
{
	public sealed record LevelEntry(string Id, string Label, string ScenePath)
	{
		public bool IsPlayable => !string.IsNullOrEmpty(ScenePath);
	}

	// Each path is the level's own scene (tiles, goal, hazards), not Main.tscn.
	// Main.tscn is the shared scene the chosen level gets loaded into.
	public static readonly LevelEntry[] Levels =
	{
		// Use Level01.tscn here instead if you are using the newer level 1 layout.
		new("level_01", "GALLERY 01", "res://scenes/levels/level_01/Level01Old.tscn"),
		new("level_02", "GALLERY 02", "res://scenes/levels/level_02/Level02.tscn"),
		// Work in progress: add the scene path here when level 3 is ready.
		new("level_03", "GALLERY 03", "")
	};

	// Position of a level in Levels, or -1 if the id isn't in the list.
	public static int IndexOf(string id)
	{
		for (int i = 0; i < Levels.Length; i++)
		{
			if (Levels[i].Id == id)
			{
				return i;
			}
		}
		return -1;
	}

	// The level after this one, or null if this is the last one or the next isn't ready.
	public static LevelEntry NextPlayable(string id)
	{
		int index = IndexOf(id);
		if (index < 0 || index + 1 >= Levels.Length)
		{
			return null;
		}

		LevelEntry next = Levels[index + 1];
		return next.IsPlayable ? next : null;
	}

	// The level before this one, or null if this is the first.
	public static LevelEntry PreviousPlayable(string id)
	{
		int index = IndexOf(id);
		if (index <= 0)
		{
			return null;
		}

		LevelEntry previous = Levels[index - 1];
		return previous.IsPlayable ? previous : null;
	}
}
