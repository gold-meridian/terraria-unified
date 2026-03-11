using System;
using GoldMeridian.Tomoko.Updating;
using Microsoft.Extensions.DependencyInjection;
using Velopack.Sources;

namespace GoldMeridian.Tomoko.Velopack;

public static class VelopackServiceCollectionExtensions
{
	public static IServiceCollection AddVelopackUpdateProvider(
		this IServiceCollection services,
		IUpdateSource updateSource,
		Action<IVelopackBootstrapper> configureBootstrapper
	)
	{
		var bootstrapper = new VelopackBootstrapper();
		{
			configureBootstrapper(bootstrapper);
		}
		var updateProvider = new VelopackUpdateProvider(bootstrapper, updateSource);

		services.AddSingleton<IVelopackBootstrapper>(bootstrapper);
		services.AddSingleton<IUpdateProvider>(updateProvider);

		return services;
	}
}