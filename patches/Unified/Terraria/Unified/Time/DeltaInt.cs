using System.Runtime.CompilerServices;
using static Terraria.Unified.Time.DeltaTimeProviderExtensions;

namespace Terraria.Unified.Time;

// Stored as a float for obvious reasons.
public struct DeltaInt<TProvider>(float value)
	where TProvider : IDeltaTimeProvider<TProvider>
{
	public float Value = value;

	public static float Dt {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => DeltaTimeOverrider<TProvider>.OverrideDeltaTime ?? TProvider.ActualDeltaTime;
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
		a.Value += b * Dt;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaInt<TProvider> operator -(DeltaInt<TProvider> a, int b)
	{
		a.Value -= b * Dt;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaInt<TProvider> operator ++(DeltaInt<TProvider> a)
	{
		a.Value += 1f * Dt;
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DeltaInt<TProvider> operator --(DeltaInt<TProvider> a)
	{
		a.Value -= 1f * Dt;
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
		Value += perTick * Dt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Sub(int perTick)
	{
		Value -= perTick * Dt;
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
