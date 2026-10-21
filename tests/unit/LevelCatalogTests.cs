using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

// LevelCatalog: the level list, its order, and each level's star times.
public class LevelCatalogTests
{
	[Fact]
	public void LevelIdsAreUnique()
	{
		var ids = LevelCatalog.Levels.Select(level => level.Id).ToList();
		Assert.Equal(ids.Count, ids.Distinct().Count());
	}

	[Fact]
	public void EveryLevelIsPlayableAndItsSceneExists()
	{
		foreach (LevelCatalog.LevelEntry level in LevelCatalog.Levels)
		{
			Assert.True(level.IsPlayable, $"{level.Id} has no scene path");
			Assert.True(File.Exists(RepoPaths.FromRes(level.ScenePath)), $"{level.Id}: {level.ScenePath} doesn't exist");
		}
	}

	[Fact]
	public void StarTimesAreOrderedAndInsideTheTimeLimit()
	{
		float timeLimit = ReadTimeLimitSeconds();

		foreach (LevelCatalog.LevelEntry level in LevelCatalog.Levels)
		{
			Assert.True(level.ThreeStarSeconds > 0.0f, $"{level.Id}: three-star time must be positive");
			Assert.True(level.ThreeStarSeconds < level.TwoStarSeconds, $"{level.Id}: three-star time must be under the two-star time");
			Assert.True(level.TwoStarSeconds < timeLimit, $"{level.Id}: two-star time must be under the {timeLimit} s time limit");
		}
	}

	[Fact]
	public void StarTimesGrowWithEachGallery()
	{
		var levels = LevelCatalog.Levels;
		for (int i = 1; i < levels.Length; i++)
		{
			Assert.True(levels[i].ThreeStarSeconds >= levels[i - 1].ThreeStarSeconds, $"{levels[i].Id}: three-star time is shorter than the previous gallery's");
			Assert.True(levels[i].TwoStarSeconds >= levels[i - 1].TwoStarSeconds, $"{levels[i].Id}: two-star time is shorter than the previous gallery's");
		}
	}

	// Gallery 01's times are the ones shown in the video demonstration.
	[Fact]
	public void Gallery01KeepsTheStarTimesFromTheVideo()
	{
		LevelCatalog.LevelEntry level = LevelCatalog.Find("level_01");
		Assert.NotNull(level);
		Assert.Equal(30.0f, level.ThreeStarSeconds);
		Assert.Equal(60.0f, level.TwoStarSeconds);
	}

	// "Under" a star time: finishing exactly on it drops to the next grade.
	[Theory]
	[InlineData(0.0f, 3)]
	[InlineData(29.99f, 3)]
	[InlineData(30.0f, 2)]
	[InlineData(59.99f, 2)]
	[InlineData(60.0f, 1)]
	[InlineData(299.0f, 1)]
	public void StarsForGradesAgainstTheLevelsTimes(float seconds, int expectedStars)
	{
		var level = new LevelCatalog.LevelEntry("test", "TEST", "", 30.0f, 60.0f);
		Assert.Equal(expectedStars, level.StarsFor(seconds));
	}

	[Fact]
	public void FindReturnsNullForAnUnknownId()
	{
		Assert.Null(LevelCatalog.Find("no_such_level"));
		Assert.Equal(-1, LevelCatalog.IndexOf("no_such_level"));
	}

	[Fact]
	public void NextAndPreviousFollowTheListOrder()
	{
		var levels = LevelCatalog.Levels;

		Assert.Null(LevelCatalog.PreviousPlayable(levels[0].Id));
		Assert.Null(LevelCatalog.NextPlayable(levels[^1].Id));

		for (int i = 0; i + 1 < levels.Length; i++)
		{
			Assert.Equal(levels[i + 1], LevelCatalog.NextPlayable(levels[i].Id));
			Assert.Equal(levels[i], LevelCatalog.PreviousPlayable(levels[i + 1].Id));
		}
	}

	// The run's time limit is set on the LevelTimer in Main.tscn.
	private static float ReadTimeLimitSeconds()
	{
		string main = File.ReadAllText(RepoPaths.FromRes("res://scenes/main/Main.tscn"));
		Match match = Regex.Match(main, @"^TimeLimitSeconds = ([0-9.]+)", RegexOptions.Multiline);
		Assert.True(match.Success, "Main.tscn doesn't set TimeLimitSeconds on the LevelTimer");
		return float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
	}
}
