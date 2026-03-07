using System.Runtime.CompilerServices;

namespace Terraria.Unified.Time;

public struct DeltaVector2<TProvider>(Vector2 value)
	where TProvider : IDeltaFactorProvider<TProvider>
{
	public DeltaFloat<TProvider> X = value.X;
	public DeltaFloat<TProvider> Y = value.Y;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector2(DeltaVector2<TProvider> v)
	{
		return new Vector2(v.X.Value, v.Y.Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator DeltaVector2<TProvider>(Vector2 v)
	{
		return new(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void operator +=(Vector2 b)
	{
		X += b.X;
		Y += b.Y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void operator -=(Vector2 b)
	{
		X -= b.X;
		Y -= b.Y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void operator *=(float b)
	{
		X *= b;
		Y *= b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void operator /=(float b)
	{
		X /= b;
		Y /= b;
	}

	/*
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(Vector2 perTick)
	{
		X.Add(perTick.X);
		Y.Add(perTick.Y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Sub(Vector2 perTick)
	{
		X.Sub(perTick.X);
		Y.Sub(perTick.Y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(float x, float y)
	{
		X.Add(x);
		Y.Add(y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Sub(float x, float y)
	{
		X.Sub(x);
		Y.Sub(y);
	}
	*/

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(DeltaVector2<TProvider> a, DeltaVector2<TProvider> b)
	{
		return a.X == b.X && a.Y == b.Y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(DeltaVector2<TProvider> a, DeltaVector2<TProvider> b)
	{
		return a.X != b.X || a.Y != b.Y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(DeltaVector2<TProvider> a, Vector2 b)
	{
		return a.X == b.X && a.Y == b.Y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(DeltaVector2<TProvider> a, Vector2 b)
	{
		return a.X != b.X || a.Y != b.Y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Vector2 a, DeltaVector2<TProvider> b)
	{
		return a.X == b.X && a.Y == b.Y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Vector2 a, DeltaVector2<TProvider> b)
	{
		return a.X != b.X || a.Y != b.Y;
	}

	public override string ToString()
	{
		return $"({X.Value}, {Y.Value})";
	}
}