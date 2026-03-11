using System;

namespace GoldMeridian.Tomoko.Updating;

public sealed record UpdateDescriptor(
	string Version,
	string? Channel,
	string? Summary = null,
	DateTimeOffset? PublishedAt = null
);