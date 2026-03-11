using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GoldMeridian.Tomoko.Environment;

public sealed class CompositeInstallationProfileDetector(IEnumerable<IInstallationProfileProbe> probes) : IInstallationProfileDetector
{
	public async ValueTask<IInstallationProfile> DetectAsync(CancellationToken cancellationToken = default)
	{
		foreach (var probe in probes.OrderByDescending(static x => x.Priority)) {
			var profile = await probe.TryDetectAsync(cancellationToken);
			if (profile is null) {
				continue;
			}

			return profile;
		}

		var baseDirectory = AppContext.BaseDirectory;
		return new InstallationProfile(
			Kind: InstallationKind.Unknown,
			IsPortable: false,
			IsPackageManaged: false,
			IsDevelopmentBuild: false,
			CanSelfUpdate: true,
			PackageManagerName: null,
			InstallRoot: baseDirectory
		);
	}
}