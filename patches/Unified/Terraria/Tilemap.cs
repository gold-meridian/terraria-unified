using System.Diagnostics;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;

namespace Terraria;

public readonly struct Tilemap
{
	public readonly ushort Width;
	public readonly ushort Height;

	public Tile this[int x, int y] {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get {
			Debug.Assert(x >= 0 && x < Width && y >= 0 && y < Height);
			return new Tile((uint)(y + (x * Height)));
		}
	}

	public Tile this[Point pos] => this[pos.X, pos.Y];

	public Tile this[Point16 pos] => this[pos.X, pos.Y];

	internal Tilemap(ushort width, ushort height)
	{
		Width = width;
		Height = height;
		TileData.Length = (uint)width * height;
	}

	public void ClearEverything() => TileData.ClearEverything();

	public T[] GetData<T>() where T : unmanaged, ITileData => TileData<T>.Data;
}
