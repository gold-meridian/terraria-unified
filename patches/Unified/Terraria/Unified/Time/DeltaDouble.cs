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
	public void operator +=(double b)
	{
		Value += b * Factor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void  operator -=(double b)
	{
		Value -= b * Factor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void operator *=(double b)
	{
		Value *= b * Factor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void operator /=(double b)
	{
		Value /= b * Factor;
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
	public void Add(double perTick)
	{
		Value += perTick * Factor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Sub(double perTick)
	{
		Value -= perTick * Factor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(DeltaDouble<TProvider> a, DeltaDouble<TProvider> b)
	{
		return a.Value == b.Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(DeltaDouble<TProvider> a, DeltaDouble<TProvider> b)
	{
		return a.Value == b.Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(DeltaDouble<TProvider> a, double b)
	{
		return a.Value == b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(DeltaDouble<TProvider> a, double b)
	{
		return a.Value == b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(double a, DeltaDouble<TProvider> b)
	{
		return a == b.Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(double a, DeltaDouble<TProvider> b)
	{
		return a == b.Value;
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
