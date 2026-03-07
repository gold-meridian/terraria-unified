using System.Runtime.CompilerServices;

namespace Terraria.Unified.Time;

// Stored as a float for obvious reasons.
public struct DeltaInt<TProvider>(float value)
	where TProvider : IDeltaFactorProvider<TProvider>
{
	public float Value = value;

	public static float Factor {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => TProvider.DeltaFactor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator int(DeltaInt<TProvider> v)
	{
		return (int)v.Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator DeltaInt<TProvider>(int v)
	{
		return new(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaInt<TProvider> operator +(DeltaInt<TProvider> a, int b)
	{
		a.Value += b * Factor;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaInt<TProvider> operator -(DeltaInt<TProvider> a, int b)
	{
		a.Value -= b * Factor;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaInt<TProvider> operator ++(DeltaInt<TProvider> a)
	{
		a.Value += 1f * Factor;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaInt<TProvider> operator --(DeltaInt<TProvider> a)
	{
		a.Value -= 1f * Factor;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaInt<TProvider> operator *(DeltaInt<TProvider> a, int b)
	{
		a.Value *= b;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaInt<TProvider> operator /(DeltaInt<TProvider> a, int b)
	{
		a.Value /= b;
		return a;
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(int perTick)
	{
		Value += perTick * Factor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Sub(int perTick)
	{
		Value -= perTick * Factor;
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
