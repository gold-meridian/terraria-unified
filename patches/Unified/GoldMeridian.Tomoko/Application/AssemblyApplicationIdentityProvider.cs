using System;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NuGet.Versioning;

namespace GoldMeridian.Tomoko.Application;

public sealed class AssemblyApplicationIdentityProvider(
	Assembly assembly,
	string applicationId,
	string displayName,
	string? channel
) : IApplicationIdentityProvider
{
	private static readonly Version default_sys_version = new(0, 0, 0);
	private static readonly SemanticVersion default_sem_version = new(0, 0, 0);

	public ValueTask<IApplicationIdentity> GetIdentityAsync(CancellationToken cancellationToken = default)
	{
		if (assembly.GetName().Version is not { } asmVersion || !Version.TryParse(asmVersion.ToString(), out var sysVersion)) {
			sysVersion = default_sys_version;
		}

		if (!SemanticVersion.TryParse($"{sysVersion.Major}.{sysVersion.Minor}.{sysVersion.Build}", out var semVer)) {
			semVer = default_sem_version;
		}

		var executablePath = System.Environment.ProcessPath ?? assembly.Location;
		var baseDirectory = AppContext.BaseDirectory;

		var identity = new ApplicationIdentity(
			ApplicationId: applicationId,
			DisplayName: displayName,
			Version: semVer,
			Channel: channel,
			ExecutablePath: executablePath,
			BaseDirectory: baseDirectory
		);
		return ValueTask.FromResult<IApplicationIdentity>(identity);
	}
}