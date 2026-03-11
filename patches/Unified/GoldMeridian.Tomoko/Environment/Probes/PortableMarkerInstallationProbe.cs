using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GoldMeridian.Tomoko.Environment.Probes;

public sealed class PortableMarkerInstallationProbe(string[]? markers = null) : IInstallationProfileProbe
{
	private static readonly string[] default_markers = [".portable"];

	public int Priority => 900;

	private readonly string[] markers = markers ?? default_markers;

	public ValueTask<IInstallationProfile?> TryDetectAsync(CancellationToken cancellationToken = default)
	{
		var baseDirectory = AppContext.BaseDirectory;

		foreach (var markerFile in markers) {
			var fullPath = Path.Combine(baseDirectory, markerFile);
			if (!File.Exists(fullPath)) {
				continue;
			}

			var profile = new InstallationProfile(
				Kind: InstallationKind.Portable,
				IsPortable: true,
				IsPackageManaged: false,
				IsDevelopmentBuild: false,
				CanSelfUpdate: true,
				PackageManagerName: null,
				InstallRoot: baseDirectory
			);
			return ValueTask.FromResult<IInstallationProfile?>(profile);
		}

		return ValueTask.FromResult<IInstallationProfile?>(null);
	}
}