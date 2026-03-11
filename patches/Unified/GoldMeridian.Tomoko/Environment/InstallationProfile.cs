namespace GoldMeridian.Tomoko.Environment;

public interface IInstallationProfile
{
	InstallationKind Kind { get; }

	bool IsPortable { get; }

	bool IsPackageManaged { get; }

	bool IsDevelopmentBuild { get; }

	bool CanSelfUpdate { get; }

	string? PackageManagerName { get; }

	string InstallRoot { get; }
}

public sealed record InstallationProfile(
	InstallationKind Kind,
	bool IsPortable,
	bool IsPackageManaged,
	bool IsDevelopmentBuild,
	bool CanSelfUpdate,
	string? PackageManagerName,
	string InstallRoot
) : IInstallationProfile;