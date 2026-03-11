using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GoldMeridian.Tomoko.Updating;
using Velopack;
using Velopack.Sources;

namespace GoldMeridian.Tomoko.Velopack;

public sealed class VelopackUpdateProvider(
	IVelopackBootstrapper bootstrapper,
	IUpdateSource updateSource
) : IUpdateProvider
{
	public string Name => "Velopack";

	private readonly UpdateManager manager = new(updateSource);
	private UpdateInfo? updates;

	public bool HandleStartupCommands(IReadOnlyList<string> args)
	{
		bootstrapper.Run();
		return false;
	}

	public async ValueTask<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
	{
		updates = await manager.CheckForUpdatesAsync();
		if (updates is null) {
			return new UpdateCheckResult(UpdateAvailability.None);
		}

		return new UpdateCheckResult(
			Availability: UpdateAvailability.Available,
			Update: new UpdateDescriptor(
				Version: updates.TargetFullRelease.Version.ToString(),
				Channel: null
			)
		);
	}

	public async ValueTask DownloadUpdatesAsync(
		IProgress<UpdateProgress>? progress = null,
		CancellationToken cancellationToken = default
	)
	{
		if (updates is null) {
			throw new InvalidOperationException("Cannot download updates without first checking for them");
		}

		await manager.DownloadUpdatesAsync(
			updates,
			x => {
				progress?.Report(
					new UpdateProgress(
						BytesReceived: null,
						TotalBytes: null,
						Percent: x,
						Phase: "download"
					)
				);
			},
			cancellationToken
		);
	}

	public async ValueTask ApplyUpdatesAsync(
		bool restartAfterApplication,
		CancellationToken cancellationToken = default
	)
	{
		if (updates is null) {
			throw new InvalidOperationException("Cannot apply updates without first checking for them");
		}

		var targetRelease = updates.TargetFullRelease;
		await manager.WaitExitThenApplyUpdatesAsync(targetRelease, restart: restartAfterApplication).ConfigureAwait(false);
	}
}