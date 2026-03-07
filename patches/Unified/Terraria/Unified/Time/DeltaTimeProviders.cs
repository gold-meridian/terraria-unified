using System;
using System.Runtime.CompilerServices;

namespace Terraria.Unified.Time;

public interface IDeltaTimeProvider<TSelf>
	where TSelf : IDeltaTimeProvider<TSelf>
{
	static abstract float ActualDeltaTime { get; }
}

public static class DeltaTimeProviderExtensions
{
	public sealed class DeltaTimeOverrider<T> : IDisposable
		where T : IDeltaTimeProvider<T>
	{
		public static float? OverrideDeltaTime { get; set; }

		private readonly float? oldDelta;

		public DeltaTimeOverrider(float overrideDelta)
		{
			oldDelta = OverrideDeltaTime;
			OverrideDeltaTime = overrideDelta;
		}

		public void Dispose()
		{
			OverrideDeltaTime = oldDelta;
		}
	}

	extension<T>(T) where T : IDeltaTimeProvider<T>
	{
		public static float DeltaTime => DeltaTimeOverrider<T>.OverrideDeltaTime ?? T.ActualDeltaTime;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IDisposable Override(float value)
		{
			return new DeltaTimeOverrider<T>(value);
		}
	}
}

public readonly struct MainDelta : IDeltaTimeProvider<MainDelta>
{
	public static float ActualDeltaTime => Main.CurrentFactor;
}

public readonly struct NpcFrameCounter : IDeltaTimeProvider<NpcFrameCounter>
{
	public static float ActualDeltaTime => Main.CurrentFactor;
}

public readonly struct ProjectileFrameCounter : IDeltaTimeProvider<ProjectileFrameCounter>
{
	public static float ActualDeltaTime => Main.CurrentFactor;
}