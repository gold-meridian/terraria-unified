using System;
using System.Threading;
using System.Threading.Tasks;

namespace GoldMeridian.Tomoko.Environment.Probes;

public sealed class EnvironmentVariablePackageManagerProbe(string environmentVariable) : IInstallationProfileProbe
{
	public int Priority => 800;

	public ValueTask<IInstallationProfile?> TryDetectAsync(CancellationToken cancellationToken = default)
	{
		var baseDirectory = AppContext.BaseDirectory;
		var manager = System.Environment.GetEnvironmentVariable(environmentVariable);
		if (string.IsNullOrWhiteSpace(manager)) {
			return ValueTask.FromResult<IInstallationProfile?>(null);
		}

		var profile = new InstallationProfile(
			Kind: InstallationKind.PackageManaged,
			IsPortable: false,
			IsPackageManaged: true,
			IsDevelopmentBuild: false,
			CanSelfUpdate: false,
			PackageManagerName: manager,
			InstallRoot: baseDirectory
		);
		return ValueTask.FromResult<IInstallationProfile?>(profile);
	}
}