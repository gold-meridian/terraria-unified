using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using GoldMeridian.Tomoko.Application;
using GoldMeridian.Tomoko.Environment;
using GoldMeridian.Tomoko.Hosting;
using GoldMeridian.Tomoko.Updating;
using GoldMeridian.Tomoko.Velopack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReLogic.OS;
using Velopack;
using Velopack.Sources;

namespace Terraria.Unified.Startup;

/// <summary>
///		Responsible for launching the game and managing its lifetime.
/// </summary>
public static class GameLaunch
{
	/// <summary>
	///		The host container and lifetime of the game.
	/// </summary>
	public static GameLifetime Instance {
		get => field ?? throw new InvalidOperationException("Cannot access game lifetime before the game has started");
		private set;
	}

#region Program member contracts
	// The following members are from the original Program implementation and
	// are depended on by various parts of the game.

	/// <summary>
	///		Whether the game is running under XNA; always false.
	/// </summary>
	internal static bool IsXna => false;

	/// <summary>
	///		Whether the game is running under FNA; always true.
	/// </summary>
	internal static bool IsFna => true;

	/// <summary>
	///		Whether the game is running under Mono; always false.
	///		<br />
	///		It's technically possible for the game to be running using Mono,
	///		but any checks that use this do not matter.
	/// </summary>
	internal static bool IsMono => false;

	/// <summary>
	///		Parsed launch arguments.
	/// </summary>
	internal static Dictionary<string, string> LaunchParameters { get; private set; }

	/// <summary>
	///		Whether the main Terraria assembly has been fully manually JITed
	///		and has had its static members all initialized.
	/// </summary>
	internal static bool LoadedEverything => Instance.Host.Services.GetRequiredService<IPreJitPolicy>().FinishedLoading;

	/// <summary>
	///		The root directory in which game content is saved.  This is
	///		typically per-user and stored separately, but may be the game's
	///		root in cases where it isn't accessible.
	/// </summary>
	internal static string SavePath { get; private set; }
#endregion

	private const string app_id = "dev.tomat.terraria.unified";
	private const string app_name = "Terraria: Unified";
	private const string app_package_manager = "TERRARIA_UNIFIED_PACKAGE_MANAGER";
	private const string app_update_url = "https://github.com/gold-meridian/terraria-unified";

	internal static void StartGame(string[] args)
	{
		var skipVelopackSetup = ParseArguments(args);
		{
			SavePath = LaunchParameters.TryGetValue("-savedirectory", out string savePath) ? savePath : Platform.Get<IPathService>().GetStoragePath("Terraria");
			Main.dedServ = LaunchParameters.ContainsKey("-server");
		}

		var builder = Host.CreateApplicationBuilder(args);
		{
			InitializeLogging(builder);
			InitializeApplicationServices(builder);
			InitializeGameServices(builder);
		}

		// Game start services.
		var host = builder.Build();

		var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
		{
			Logging.RedirectConsole(loggerFactory);
		}

		var logger = loggerFactory.CreateLogger("Terraria");

		logger.LogInformation("Using launch arguments:");
		foreach (var (key, value) in LaunchParameters) {
			if (string.IsNullOrEmpty(value)) {
				logger.LogInformation($"{0}", key);
			}
			else {
				logger.LogInformation($"{key}: {value}");
			}
		}

		var install = host.Services.GetRequiredService<IInstallationProfile>();
		if (skipVelopackSetup) {
			logger.LogInformation("Skipping update provider setup because arguments imply it should be skipped...");
		}
		else if (!install.CanSelfUpdate) {
			logger.LogInformation("Skipping update provider setup because the install reports that it doesn't support self-updating...");
		}
		else {
			var updater = host.Services.GetRequiredService<IUpdateProvider>();
			logger.LogInformation("Running setup for updater: " + updater.Name);

			if (updater.HandleStartupCommands(args)) {
				logger.LogInformation("Updater has requested the process to exit, canceling start-up...");
				return;
			}

			logger.LogInformation("Updater handled startup comments without exiting...");
		}

		Instance = new GameLifetime(host, logger);

		host.Services.GetRequiredService<INativeLibraryResolver>().Initialize();
		host.Services.GetRequiredService<IEngineBackendInitializer>().Initialize();
		host.Services.GetRequiredService<IEngineRunner>().Run();

		host.StopAsync().GetAwaiter().GetResult();
		host.Dispose();
	}

	private static bool ParseArguments(string[] args)
	{
		// TODO: Do we need this?  tModLoader uses it because Mono does, but we
		//       aren't actually running Mono... so?
		/*
		args = Utils.ConvertMonoArgsToDotNet(args);
		*/

		LaunchParameters = Utils.ParseArguements(args);

		// Skip the Velopack setup if the game is launched with any non-velopack
		// arguments as its first arguments, indicating it may be being used as
		// a server, used to open a file, etc.
		return args.Length > 0 && !args[0].StartsWith("--velo", StringComparison.Ordinal);
	}

	private static void InitializeLogging(HostApplicationBuilder builder)
	{
		// TODO: Is this really needed?
		/*
		try {
			Console.OutputEncoding = Encoding.UTF8;
			Console.InputEncoding = Platform.IsWindows ? Encoding.Unicode : Encoding.UTF8;
		}
		catch {
			// no-op
		}
		*/

		Logging.Initialize(builder.Logging);
	}

	private static void InitializeApplicationServices(HostApplicationBuilder builder)
	{
		// TODO: Eventually read the channel from a config file or something...
		builder.Services.AddHosting(
			app_id,
			app_name,
			// We use Tomoko to store our version information, etc.
			typeof(ApplicationIdentity).Assembly,
			packageManagerEnvVar: app_package_manager,
			channel: null
		);

		builder.Services.AddVelopackUpdateProvider(
			new GithubSource(app_update_url, null, prerelease: false),
			b => {
				b.OnConfigure += BootstrapVelopack;
			}
		);
	}

	private static void BootstrapVelopack(VelopackApp app)
	{
		// TODO: First-run actions, etc.?
	}

	private static void InitializeGameServices(HostApplicationBuilder builder)
	{
		builder.Services.AddSingleton<INativeLibraryResolver, NativeLibraryResolver>();
		builder.Services.AddSingleton<IEngineBackendInitializer, EngineBackendInitializer>();
		builder.Services.AddSingleton<IEngineRunner, EngineRunner>();
		builder.Services.AddSingleton<IPreJitPolicy, DefaultPreJitPolicy>();
		builder.Services.AddSingleton<IContentDirectoryResolver, ContentDirectoryResolver>();
	}
}