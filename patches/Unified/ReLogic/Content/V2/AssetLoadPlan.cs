#nullable enable

using System;
using System.Collections.Generic;
using System.IO;

namespace ReLogic.Content;

internal sealed record AssetLoadPlan(
	string Name,
	bool IsTracked,
	IReadOnlyList<AssetLoadCandidate> Candidates
);

internal readonly record struct AssetLoadCandidate(
	string Name,
	string Extension,
	Func<Stream> OpenStream,
	object? SourceTag
);