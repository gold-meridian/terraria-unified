using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GoldMeridian.Tomoko.Updating;

public interface IUpdateProvider
{
	string Name { get; }

	bool HandleStartupCommands(
		IReadOnlyList<string> args
	);

	ValueTask<UpdateCheckResult> CheckForUpdatesAsync(
		CancellationToken cancellationToken = default
	);

	ValueTask DownloadUpdatesAsync(
		IProgress<UpdateProgress>? progress = null,
		CancellationToken cancellationToken = default
	);

	ValueTask ApplyUpdatesAsync(
		bool restartAfterApplication,
		CancellationToken cancellationToken = default
	);
}