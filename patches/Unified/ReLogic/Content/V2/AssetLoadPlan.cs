#nullable enable

using System;
using System.Collections.Generic;
using System.IO;

namespace ReLogic.Content;

public sealed record AssetLoadPlan(
	string Name,
	bool IsTracked,
	IReadOnlyList<AssetLoadCandidate> Candidates
);

public readonly record struct AssetLoadCandidate(
	string Name,
	string Extension,
	Func<Stream> OpenStream,
	object? SourceTag
);