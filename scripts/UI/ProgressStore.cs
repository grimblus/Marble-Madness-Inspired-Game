using Godot;

// Saves each level's best time and best star count to user://progress.cfg.
// The result screen records finished runs; the level select and start menu
// read them back.
//
// The file is read once and then kept in memory, so the level select can
// ask for every level's stars on each card change without touching the disk.
// It's written back only when a run is recorded or progress is reset.
public static class ProgressStore
{
	private const string SavePath = "user://progress.cfg";

	private static ConfigFile _config;
	private static bool _resetThisLaunch;

	// Best (lowest) time in seconds, or null if the level has never been beaten.
	public static float? GetBestTime(string levelId)
	{
		ConfigFile cfg = Config;
		if (!cfg.HasSectionKey(levelId, "best_time"))
		{
			return null;
		}
		return cfg.GetValue(levelId, "best_time").AsSingle();
	}

	// Best star count (0-3). 0 if the level has never been beaten.
	public static int GetBestStars(string levelId)
	{
		ConfigFile cfg = Config;
		if (!cfg.HasSectionKey(levelId, "best_stars"))
		{
			return 0;
		}
		return cfg.GetValue(levelId, "best_stars").AsInt32();
	}

	// Records a finished run, keeping whichever time and star count is better.
	public static void Record(string levelId, float time, int stars)
	{
		float? bestTime = GetBestTime(levelId);
		if (!bestTime.HasValue || time < bestTime.Value)
		{
			Config.SetValue(levelId, "best_time", time);
		}

		if (stars > GetBestStars(levelId))
		{
			Config.SetValue(levelId, "best_stars", stars);
		}

		Config.Save(SavePath);
	}

	// Deletes all saved stars and best times.
	public static void ResetAll()
	{
		if (FileAccess.FileExists(SavePath))
		{
			DirAccess.RemoveAbsolute(SavePath);
		}
		_config = new ConfigFile();
	}

	// Resets progress the first time it is called after the game launches, and does
	// nothing on later calls (the start menu is shown many times per run).
	public static void ResetOncePerLaunch()
	{
		if (_resetThisLaunch)
		{
			return;
		}

		_resetThisLaunch = true;
		ResetAll();
	}

	// The saved progress, loaded from disk on first use.
	private static ConfigFile Config
	{
		get
		{
			if (_config == null)
			{
				_config = new ConfigFile();
				_config.Load(SavePath); // a missing file just leaves it empty
			}
			return _config;
		}
	}
}
