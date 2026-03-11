using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GoldMeridian.Tomoko.Environment.Probes;

public sealed class DevelopmentBuildInstallationProbe : IInstallationProfileProbe
{
	public int Priority => 1000;

	public ValueTask<IInstallationProfile?> TryDetectAsync(CancellationToken cancellationToken = default)
	{
		var baseDirectory = AppContext.BaseDirectory;
		var processPath = System.Environment.ProcessPath ?? string.Empty;

		var isDevelopment = Debugger.IsAttached ||
		                    baseDirectory.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
		                    processPath.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);

		if (!isDevelopment) {
			return ValueTask.FromResult<IInstallationProfile?>(null);
		}

		var profile = new InstallationProfile(
			Kind: InstallationKind.Development,
			IsPortable: false,
			IsPackageManaged: false,
			IsDevelopmentBuild: true,
			CanSelfUpdate: false,
			PackageManagerName: null,
			InstallRoot: baseDirectory
		);
		return ValueTask.FromResult<IInstallationProfile?>(profile);
	}
}