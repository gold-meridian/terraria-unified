using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GoldMeridian.Tomoko.Updating;

public sealed class NullUpdateProvider : IUpdateProvider
{
	public string Name => "None";

	public bool HandleStartupCommands(
		IReadOnlyList<string> args
	)
	{
		return false;
	}

	public ValueTask<UpdateCheckResult> CheckForUpdatesAsync(
		CancellationToken cancellationToken = default
	)
	{
		return ValueTask.FromResult(
			new UpdateCheckResult(
				UpdateAvailability.None,
				ProviderMessage: "No update provider is configured."
			)
		);
	}

	public ValueTask DownloadUpdatesAsync(
		IProgress<UpdateProgress>? progress = null,
		CancellationToken cancellationToken = default
	)
	{
		throw new NotSupportedException("No update provider is configured.");
	}

	public ValueTask ApplyUpdatesAsync(
		bool restartAfterApplication,
		CancellationToken cancellationToken = default
	)
	{
		throw new NotSupportedException("No update provider is configured.");
	}
}