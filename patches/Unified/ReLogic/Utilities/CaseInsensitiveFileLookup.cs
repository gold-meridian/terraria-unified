#nullable enable
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace ReLogic.Utilities;

public sealed class CaseInsensitiveFileLookup
{
	private readonly FrozenDictionary<string, string> _fileToPath;

	public enum LookupKeyMode
	{
		FileName,
		RelativePath,
		AbsolutePath
	}

	public CaseInsensitiveFileLookup(
		string corePath,
		LookupKeyMode keyMode = LookupKeyMode.FileName,
		bool recursive = false)
		: this(
			GetFiles(corePath, recursive),
			keyMode,
			keyMode == LookupKeyMode.RelativePath ? Path.GetFullPath(corePath) : null)
	{ }

	public CaseInsensitiveFileLookup(
		IEnumerable<string> files,
		LookupKeyMode keyMode = LookupKeyMode.FileName,
		string? basePath = null)
	{
		if (keyMode == LookupKeyMode.RelativePath)
			ArgumentNullException.ThrowIfNull(basePath);
		else if (basePath != null)
			throw new ArgumentException($"{nameof(basePath)} must be null (because {nameof(keyMode)} isn't '{LookupKeyMode.RelativePath}').", nameof(basePath));

		var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		foreach (string file in files)
		{
			string key = keyMode switch {
				LookupKeyMode.FileName => Path.GetFileName(file),
				LookupKeyMode.RelativePath => Path.GetRelativePath(basePath!, file),
				LookupKeyMode.AbsolutePath => file,
				_ => throw new InvalidEnumArgumentException(nameof(keyMode))
			};

			dict.TryAdd(key, file);
		}

		_fileToPath = dict.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
	}

	private static IEnumerable<string> GetFiles(string corePath, bool recursive)
	{
		ArgumentNullException.ThrowIfNull(corePath);

		if (!Directory.Exists(corePath))
			throw new DirectoryNotFoundException(corePath);

		return Directory.EnumerateFiles(
			corePath,
			"*",
			recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
		);
	}

	public bool TryGetFile(string fileName, out string filePath)
	{
		return _fileToPath.TryGetValue(fileName, out filePath!);
	}
}