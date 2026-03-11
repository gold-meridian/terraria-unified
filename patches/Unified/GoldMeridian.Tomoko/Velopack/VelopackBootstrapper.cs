using System;
using Velopack;

namespace GoldMeridian.Tomoko.Velopack;

public interface IVelopackBootstrapper
{
	event Action<VelopackApp>? OnConfigure;

	void Run();
}

public sealed class VelopackBootstrapper : IVelopackBootstrapper
{
	public event Action<VelopackApp>? OnConfigure;

	public void Run()
	{
		var app = VelopackApp.Build();
		OnConfigure?.Invoke(app);
		app.Run();
	}
}