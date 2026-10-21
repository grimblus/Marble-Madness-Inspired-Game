using System;
using System.IO;

// Finds files in the Godot project from inside the test runner, which runs
// from the test project's build folder rather than the repository root.
public static class RepoPaths
{
	private static string _root;

	// The folder holding project.godot.
	public static string Root
	{
		get
		{
			if (_root != null)
			{
				return _root;
			}

			var dir = new DirectoryInfo(AppContext.BaseDirectory);
			while (dir != null && !File.Exists(Path.Combine(dir.FullName, "project.godot")))
			{
				dir = dir.Parent;
			}

			_root = dir?.FullName ?? throw new InvalidOperationException("Couldn't find project.godot above the test folder.");
			return _root;
		}
	}

	// Turns a res:// path into a path on disk.
	public static string FromRes(string resPath)
	{
		return Path.Combine(Root, resPath.Substring("res://".Length));
	}
}
