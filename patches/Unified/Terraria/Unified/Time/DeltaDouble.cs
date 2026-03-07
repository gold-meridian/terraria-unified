using System.Runtime.CompilerServices;
using static Terraria.Unified.Time.DeltaTimeProviderExtensions;

namespace Terraria.Unified.Time;

public struct DeltaDouble<TProvider>(double value)
	where TProvider : IDeltaTimeProvider<TProvider>
{
	public double Value = value;

	public static float Dt {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => DeltaTimeOverrider<TProvider>.OverrideDeltaTime ?? TProvider.ActualDeltaTime;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator double(DeltaDouble<TProvider> v)
		=> v.Value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator DeltaDouble<TProvider>(double v)
		=> new(v);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaDouble<TProvider> operator +(DeltaDouble<TProvider> a, double b)
	{
		a.Value += b * Dt;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaDouble<TProvider> operator -(DeltaDouble<TProvider> a, double b)
	{
		a.Value -= b * Dt;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaDouble<TProvider> operator *(DeltaDouble<TProvider> a, double b)
	{
		a.Value *= b * Dt;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaDouble<TProvider> operator /(DeltaDouble<TProvider> a, double b)
	{
		a.Value /= b * Dt;
		return a;
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(double perTick)
	{
		Value += perTick * Dt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Sub(double perTick)
	{
		Value -= perTick * Dt;
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
