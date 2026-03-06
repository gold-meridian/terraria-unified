#nullable enable

using System;
using ReLogic.Content.Sources;

namespace ReLogic.Content;

public readonly record struct AssetLoadContext(
	string Path,
	IContentSource ContentSource,
	IServiceProvider? Services = null
);