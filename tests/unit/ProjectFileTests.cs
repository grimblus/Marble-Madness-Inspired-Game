using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

// Checks the project's files without running Godot, so a moved or deleted
// asset is caught before anyone opens the editor.
public class ProjectFileTests
{
	private static readonly string[] ScannedExtensions = { ".tscn", ".tres", ".cs", ".godot" };

	// Folders that hold build output or aren't part of the game.
	private static readonly string[] SkippedFolders = { ".godot", ".git", "bin", "obj", "docs", "unit" };

	// A res:// path inside quotes. Paths built at runtime from pieces end in
	// "/" or contain "{", and are skipped below.
	private static readonly Regex ResPath = new(@"""(res://[^""]+)""");

	[Fact]
	public void EveryReferencedResPathExists()
	{
		var missing = new List<string>();

		foreach (string file in ProjectFiles())
		{
			foreach (Match match in ResPath.Matches(File.ReadAllText(file)))
			{
				string resPath = match.Groups[1].Value;
				if (resPath.EndsWith("/") || resPath.Contains('{'))
				{
					continue;
				}

				if (!File.Exists(RepoPaths.FromRes(resPath)))
				{
					missing.Add($"{Path.GetRelativePath(RepoPaths.Root, file)} -> {resPath}");
				}
			}
		}

		Assert.True(missing.Count == 0, "Missing files:\n" + string.Join("\n", missing));
	}

	[Fact]
	public void TheMainSceneAndAutoloadsExist()
	{
		string project = File.ReadAllText(Path.Combine(RepoPaths.Root, "project.godot"));

		Assert.Contains("run/main_scene=\"res://scenes/ui/menus/startMenu.tscn\"", project);
		foreach (string autoload in new[] { "CheckpointManager", "GameStateManager", "MusicPlayer", "UiSounds" })
		{
			Assert.Matches(new Regex($@"^{autoload}=""\*res://", RegexOptions.Multiline), project);
		}
	}

	private static IEnumerable<string> ProjectFiles()
	{
		return Directory.EnumerateFiles(RepoPaths.Root, "*", SearchOption.AllDirectories)
			.Where(file => ScannedExtensions.Contains(Path.GetExtension(file)))
			.Where(file => !Path.GetRelativePath(RepoPaths.Root, file)
				.Split(Path.DirectorySeparatorChar)
				.Any(part => SkippedFolders.Contains(part)));
	}
}
