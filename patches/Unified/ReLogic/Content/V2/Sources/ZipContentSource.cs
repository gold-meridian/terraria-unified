using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ionic.Zip;

namespace ReLogic.Content.Sources;

public sealed partial class ZipContentSource : AbstractContentSource, IDisposable
{
	private readonly ZipFile zipFile;
	private readonly Dictionary<string, ZipEntry> entries = new();
	private readonly string basePath;
	private bool isDisposed;

	public int EntryCount => entries.Count;

	public ZipContentSource(string path)
		: this(path, "") { }

	public ZipContentSource(string path, string contentDir)
		: this(ZipFile.Read(path), contentDir) { }

	public ZipContentSource(ZipFile zip, string contentDir)
	{
		zipFile = zip;
		if (ZipPathContainsInvalidCharacters(contentDir)) {
			throw new ArgumentException("Content directory cannot contain \"..\"", nameof(contentDir));
		}

		basePath = CleanZipPath(contentDir);
		Refresh();
	}

	public override Stream OpenStream(string assetName)
	{
		if (!entries.TryGetValue(assetName, out var value)) {
			throw new FileNotFoundException(assetName);
		}

		var memoryStream = new MemoryStream((int)value.UncompressedSize);
		lock (zipFile) {
			value.Extract(memoryStream);
		}

		memoryStream.Position = 0L;
		return memoryStream;
	}

	public override void Refresh()
	{
		entries.Clear();
		foreach (var item in zipFile.Entries.Where(entry => !entry.IsDirectory && entry.FileName.StartsWith(basePath))) {
			var fileName = item.FileName;
			var path = fileName.Substring(basePath.Length, fileName.Length - basePath.Length);
			path = AssetPathHelper.CleanPath(path);
			entries[path] = item;
		}

		SetAssetNames(entries.Keys);
	}

	private void Dispose(bool disposing)
	{
		if (isDisposed) {
			return;
		}

		if (disposing) {
			entries.Clear();
			zipFile.Dispose();
		}

		isDisposed = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	private static bool ZipPathContainsInvalidCharacters(string path)
	{
		return path.Contains("../") || path.Contains("..\\");
	}

	private static string CleanZipPath(string path)
	{
		path = path.Replace('\\', '/');
		path = MyRegex().Replace(path, "");
		if (path.Length != 0 && !path.EndsWith('/')) {
			path += "/";
		}

		return path;
	}

	[GeneratedRegex("^[./]+")]
	private static partial Regex MyRegex();
}