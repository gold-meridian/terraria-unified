using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ReLogic.Threading;

// This is a completely rewritten implmenetation of the FastParallel public API
// taking advantage of newer APIs in the stdlib Parallel class.

public static class FastParallel
{
	private sealed class WorkerState
	{
		public ParallelForAction Action;
		public object Context;
		public int ToExclusive;
		public int ChunkSize;

		public int NextIndex;

		long _pad0, _pad1, _pad2, _pad3;

		public int RemainingWorkers;
	}

	public static void For(int fromInclusive, int toExclusive, ParallelForAction callback, object context = null)
	{
		var length = toExclusive - fromInclusive;
		if (length <= 0) {
			return;
		}

		var workers = Math.Min(Environment.ProcessorCount, length);
		if (workers == 1) {
			callback(fromInclusive, toExclusive, context);
			return;
		}

		var chunkSize = Math.Max(1, length / (workers * 8));
		var state = new WorkerState {
			Action = callback,
			Context = context,
			ToExclusive = toExclusive,
			ChunkSize = chunkSize,
			NextIndex = fromInclusive,
			RemainingWorkers = workers
		};

		for (int i = 1; i < workers; i++) {
			ThreadPool.UnsafeQueueUserWorkItem(
				Worker,
				state,
				preferLocal: true
			);
		}

		Worker(state);

		var spinner = new SpinWait();
		while (Volatile.Read(ref state.RemainingWorkers) != 0) {
			spinner.SpinOnce();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Worker(WorkerState state)
	{
		var action = state.Action;
		var context = state.Context;
		var chunkSize = state.ChunkSize;
		var toExclusive = state.ToExclusive;

		while (true) {
			var start = Interlocked.Add(ref state.NextIndex, chunkSize) - chunkSize;

			if (start >= toExclusive) {
				break;
			}

			var end = start + chunkSize;
			if (end > toExclusive) {
				end = toExclusive;
			}

			action(start, end, context);
		}

		Interlocked.Decrement(ref state.RemainingWorkers);
	}
}