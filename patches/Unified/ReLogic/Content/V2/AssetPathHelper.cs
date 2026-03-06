#nullable enable

using System;
using System.IO;

namespace ReLogic.Content;

/// <summary>
///		Utilities for working with asset paths.
/// </summary>
public static class AssetPathHelper
{
	/// <summary>
	///		Sanitizes the input path.
	/// </summary>
	public static string CleanPath(string path)
	{
		// Normalize paths to use '\' separators.
		path = path.Replace("/", @"\", StringComparison.Ordinal);

		// Collapse redundant '\.\' parts into '\'.
		path = path.Replace(@"\.\", @"\", StringComparison.Ordinal);

		// For as long as the path starts with '.\', collapse it until the first
		// real path part.
		while (path.StartsWith(@".\", StringComparison.Ordinal)) {
			path = path[@".\".Length..];
		}

		// For as long as the path ends with '\.', collapse it until the last
		// real path part.
		while (path.EndsWith(@"\.", StringComparison.Ordinal)) {
			path = ((path.Length <= @"\.".Length) ? @"\" : path[..^@"\.".Length]);
		}

		int num;
		for (num = 1; num < path.Length; num = CollapseParentDirectory(ref path, num, @"\..\".Length)) {
			num = path.IndexOf(@"\..\", num, StringComparison.Ordinal);
			if (num < 0) {
				break;
			}
		}

		if (path.EndsWith(@"\..", StringComparison.Ordinal)) {
			int num2 = path.Length - @"\..".Length;
			if (num2 > 0) {
				CollapseParentDirectory(ref path, num2, @"\..".Length);
			}
		}

		if (path == ".") {
			path = string.Empty;
		}

		if (Path.DirectorySeparatorChar != '\\') {
			path = path.Replace("\\", Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
		}

		return path;
	}

	private static int CollapseParentDirectory(ref string path, int position, int removeLength)
	{
		int num = path.LastIndexOf("\\", position - 1, StringComparison.Ordinal) + 1;
		path = path.Remove(num, position - num + removeLength);
		return Math.Max(num - 1, 1);
	}
}