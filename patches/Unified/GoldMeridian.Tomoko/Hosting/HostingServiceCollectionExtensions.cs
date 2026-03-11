using System.Reflection;
using GoldMeridian.Tomoko.Application;
using GoldMeridian.Tomoko.Environment;
using GoldMeridian.Tomoko.Environment.Probes;
using GoldMeridian.Tomoko.Updating;
using Microsoft.Extensions.DependencyInjection;

namespace GoldMeridian.Tomoko.Hosting;

public static class HostingServiceCollectionExtensions
{
	public static IServiceCollection AddHosting(
		this IServiceCollection services,
		string applicationId,
		string displayName,
		Assembly assembly,
		string packageManagerEnvVar,
		string? channel = null
	)
	{
		services.AddSingleton<IApplicationIdentityProvider>(
			new AssemblyApplicationIdentityProvider(
				assembly,
				applicationId,
				displayName,
				channel
			)
		);

		services.AddSingleton<IInstallationProfileProbe, DevelopmentBuildInstallationProbe>();
		services.AddSingleton<IInstallationProfileProbe>(_ => new PortableMarkerInstallationProbe());
		services.AddSingleton<IInstallationProfileProbe>(_ => new EnvironmentVariablePackageManagerProbe(packageManagerEnvVar));

		services.AddSingleton<IInstallationProfileDetector, CompositeInstallationProfileDetector>();

		services.AddSingleton<IApplicationIdentity>(
			sp =>
				sp.GetRequiredService<IApplicationIdentityProvider>()
				  .GetIdentityAsync()
				  .GetAwaiter()
				  .GetResult()
		);

		services.AddSingleton<IInstallationProfile>(
			sp =>
				sp.GetRequiredService<IInstallationProfileDetector>()
				  .DetectAsync()
				  .GetAwaiter()
				  .GetResult()
		);

		services.AddSingleton<IUpdateProvider, NullUpdateProvider>();
		services.AddSingleton<IApplicationHostingContext, ApplicationHostingContext>();

		return services;
	}
}