namespace GoldMeridian.Tomoko.Updating;

public sealed record UpdateCheckResult(
	UpdateAvailability Availability,
	UpdateDescriptor? Update = null,
	string? ProviderMessage = null
);