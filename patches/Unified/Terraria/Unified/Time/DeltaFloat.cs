using System.Runtime.CompilerServices;
using static Terraria.Unified.Time.DeltaTimeProviderExtensions;

namespace Terraria.Unified.Time;

public struct DeltaFloat<TProvider>(float value)
	where TProvider : IDeltaTimeProvider<TProvider>
{
	public float Value = value;

	public static float Dt {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => DeltaTimeOverrider<TProvider>.OverrideDeltaTime ?? TProvider.ActualDeltaTime;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator float(DeltaFloat<TProvider> v)
		=> v.Value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator DeltaFloat<TProvider>(float v)
		=> new(v);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaFloat<TProvider> operator +(DeltaFloat<TProvider> a, float b)
	{
		a.Value += b * Dt;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaFloat<TProvider> operator -(DeltaFloat<TProvider> a, float b)
	{
		a.Value -= b * Dt;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaFloat<TProvider> operator *(DeltaFloat<TProvider> a, float b)
	{
		a.Value *= b * Dt;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaFloat<TProvider> operator /(DeltaFloat<TProvider> a, float b)
	{
		a.Value /= b * Dt;
		return a;
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(float perTick)
	{
		Value += perTick * Dt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Sub(float perTick)
	{
		Value -= perTick * Dt;
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}

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
