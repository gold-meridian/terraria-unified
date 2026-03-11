using System;
using System.Threading;
using System.Threading.Tasks;
using Velopack;

namespace GoldMeridian.Tomoko.Versioning.Updating;

public abstract class VelopackUpdateManager : IUpdateManager, IDisposable
{
	public abstract bool IsPackageManaged { get; }

	public virtual Task<bool> CheckForUpdateAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();

	public virtual bool HandleLaunch(string[] args)
	{
		if (args.Length > 0 && !args[0].StartsWith("--velo", StringComparison.Ordinal)) {
			return false;
		}

		if (IsPackageManaged) {
			return false;
		}

		var app = VelopackApp.Build();

		// TODO: Do something on first run?
		// app.OnFirstRun(_ => isFirstRun = true);
		ConfigureApp(app);

		app.Run();
		return false;
	}

	protected virtual void ConfigureApp(VelopackApp app) { }

	public virtual void Dispose()
	{

	}
}
