namespace GoldMeridian.Tomoko.Updating;

public sealed record UpdateProgress(
	long? BytesReceived,
	long? TotalBytes,
	double? Percent,
	string? Phase = null
);