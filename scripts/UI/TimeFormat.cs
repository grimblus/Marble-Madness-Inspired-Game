using Godot;

// Time formatting shared by the HUD, the result screen and the level select,
// so every screen shows times the same way.
public static class TimeFormat
{
	// Stopwatch style, mm:ss.cc, e.g. 00:27.48.
	public static string Clock(float seconds)
	{
		int centis = Mathf.FloorToInt(Mathf.Max(0.0f, seconds) * 100.0f);
		int minutes = centis / 6000;
		int secs = centis / 100 % 60;
		int hundredths = centis % 100;
		return $"{minutes:00}:{secs:00}.{hundredths:00}";
	}

	// Short style for star thresholds, m:ss, e.g. 5:00.
	public static string Short(float seconds)
	{
		int total = Mathf.RoundToInt(seconds);
		return $"{total / 60}:{total % 60:00}";
	}
}
