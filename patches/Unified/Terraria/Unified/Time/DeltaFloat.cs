using System.Runtime.CompilerServices;

namespace Terraria.Unified.Time;

public struct DeltaFloat<TProvider>(float value)
	where TProvider : IDeltaFactorProvider<TProvider>
{
	public float Value = value;

	public static float Factor {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => TProvider.DeltaFactor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator float(DeltaFloat<TProvider> v)
	{
		return v.Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator DeltaFloat<TProvider>(float v)
	{
		return new(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void operator +=(float b)
	{
		Value += b * Factor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void operator -=(float b)
	{
		Value -= b * Factor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void operator *=(float b)
	{
		Value *= b * Factor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void operator /=(float b)
	{
		Value /= b * Factor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaFloat<TProvider> operator ++(DeltaFloat<TProvider> a)
	{
		a.Value += 1f * Factor;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaFloat<TProvider> operator --(DeltaFloat<TProvider> a)
	{
		a.Value -= 1f * Factor;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(float perTick)
	{
		Value += perTick * Factor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Sub(float perTick)
	{
		Value -= perTick * Factor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(DeltaFloat<TProvider> a, DeltaFloat<TProvider> b)
	{
		return a.Value == b.Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(DeltaFloat<TProvider> a, DeltaFloat<TProvider> b)
	{
		return a.Value == b.Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(DeltaFloat<TProvider> a, float b)
	{
		return a.Value == b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(DeltaFloat<TProvider> a, float b)
	{
		return a.Value == b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(float a, DeltaFloat<TProvider> b)
	{
		return a == b.Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(float a, DeltaFloat<TProvider> b)
	{
		return a == b.Value;
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
