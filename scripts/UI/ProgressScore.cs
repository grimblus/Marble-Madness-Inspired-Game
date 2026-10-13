using Godot;

// Saves each level's best time and best star count to user://progress.cfg.
// The result screen writes to it; the level select screen will read from it.

public static class ProgressStore
{
	private const string SavePath = "user://progress.cfg";

	// Best (lowest) time in seconds, or null if the level has never been beaten.
	public static float? GetBestTime(string levelId)
	{
		var cfg = Load();
		if (!cfg.HasSectionKey(levelId, "best_time"))
		{
			return null;
		}
		return cfg.GetValue(levelId, "best_time").AsSingle();
	}

	// Best star count (0-3). 0 if the level has never been beaten.
	public static int GetBestStars(string levelId)
	{
		var cfg = Load();
		if (!cfg.HasSectionKey(levelId, "best_stars"))
		{
			return 0;
		}
		return cfg.GetValue(levelId, "best_stars").AsInt32();
	}

	// Records a finished run, keeping whichever time and star count is better.
	public static void Record(string levelId, float time, int stars)
	{
		var cfg = Load();

		float? bestTime = GetBestTime(levelId);
		if (!bestTime.HasValue || time < bestTime.Value)
		{
			cfg.SetValue(levelId, "best_time", time);
		}

		if (stars > GetBestStars(levelId))
		{
			cfg.SetValue(levelId, "best_stars", stars);
		}

		cfg.Save(SavePath);
	}

	private static ConfigFile Load()
	{
		var cfg = new ConfigFile();
		cfg.Load(SavePath); // a missing file just leaves it empty
		return cfg;
	}
}
