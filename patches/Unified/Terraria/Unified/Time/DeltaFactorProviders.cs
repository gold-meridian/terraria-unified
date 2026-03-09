using System;
using System.Runtime.CompilerServices;

namespace Terraria.Unified.Time;

public interface IDeltaFactorProvider<TSelf>
	where TSelf : IDeltaFactorProvider<TSelf>
{
	static abstract float RealFactor { get; }

	static virtual float DeltaFactor => DeltaFactorOverrider<TSelf>.OverrideFactor ?? TSelf.RealFactor;
}

public sealed class DeltaFactorOverrider<T> : IDisposable
	where T : IDeltaFactorProvider<T>
{
	public static float? OverrideFactor { get; set; }

	private readonly float? oldFactor;

	public DeltaFactorOverrider(float overrideFactor)
	{
		oldFactor = OverrideFactor;
		OverrideFactor = overrideFactor;
	}

	public void Dispose()
	{
		OverrideFactor = oldFactor;
	}
}

public static class DeltaFactorProviderExtensions
{
	extension<T>(T) where T : IDeltaFactorProvider<T>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IDisposable Override(float value)
		{
			return new DeltaFactorOverrider<T>(value);
		}
	}
}

public readonly struct CurrentDelta : IDeltaFactorProvider<CurrentDelta>
{
	public static float RealFactor => Main.CurrentFactor;
}

public readonly struct UpdateDelta : IDeltaFactorProvider<UpdateDelta>
{
	public static float RealFactor => Main.UpdateFactor;
}

public readonly struct DrawDelta : IDeltaFactorProvider<DrawDelta>
{
	public static float RealFactor => Main.DrawFactor;
}

public readonly struct CloudDelta : IDeltaFactorProvider<CloudDelta>
{
	public static float RealFactor => Main.CurrentFactor;
}
