using System.Runtime.CompilerServices;

namespace Terraria.Unified.Time;

public struct DeltaDouble<TProvider>(double value)
	where TProvider : IDeltaFactorProvider<TProvider>
{
	public double Value = value;

	public static float Factor {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => TProvider.DeltaFactor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator double(DeltaDouble<TProvider> v)
	{
		return v.Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator DeltaDouble<TProvider>(double v)
	{
		return new(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaDouble<TProvider> operator +(DeltaDouble<TProvider> a, double b)
	{
		a.Value += b * Factor;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaDouble<TProvider> operator -(DeltaDouble<TProvider> a, double b)
	{
		a.Value -= b * Factor;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaDouble<TProvider> operator ++(DeltaDouble<TProvider> a)
	{
		a.Value += 1f * Factor;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaDouble<TProvider> operator --(DeltaDouble<TProvider> a)
	{
		a.Value -= 1f * Factor;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaDouble<TProvider> operator *(DeltaDouble<TProvider> a, double b)
	{
		a.Value *= b * Factor;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaDouble<TProvider> operator /(DeltaDouble<TProvider> a, double b)
	{
		a.Value /= b * Factor;
		return a;
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(double perTick)
	{
		Value += perTick * Factor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Sub(double perTick)
	{
		Value -= perTick * Factor;
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
