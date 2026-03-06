using System;
using System.Threading;

namespace ReLogic.Threading;

// This is a completely rewritten implmenetation of the FastParallel public API
// taking advantage of newer APIs in the stdlib Parallel class.

public static class FastParallel
{
	private record struct WorkerState(
		ParallelForAction Action,
		object Context,
		int ToExclusive,
		int ChunkSize,
		ManualResetEventSlim DoneEvent,
		int WorkerCount
	)
	{
		public int NextIndex;

		public int RemainingWorkers;
	}

	public static void For(int fromInclusive, int toExclusive, ParallelForAction callback, object context = null)
	{
		int length = toExclusive - fromInclusive;
		if (length <= 0) {
			return;
		}

		var done = new ManualResetEventSlim(false);

		var workers = Math.Min(Environment.ProcessorCount, length);
		var chunkSize = Math.Max(1, length / (workers * 4));

		var state = new WorkerState(
			callback,
			context,
			toExclusive,
			chunkSize,
			done,
			workers
		) {
			NextIndex = fromInclusive,
			RemainingWorkers = workers
		};

		for (var i = 1; i < workers; i++) {
			ThreadPool.UnsafeQueueUserWorkItem(
				static s => s.Invoke(),
				Worker,
				preferLocal: false
			);
		}

		Worker();

		if (!done.Wait(10000)) {
			ThreadPool.GetAvailableThreads(out int workerThreads, out _);
			throw new Exception($"Fatal Deadlock in FastParallelFor. pending: {ThreadPool.PendingWorkItemCount}. avail: {workerThreads}");
		}

		return;

		void Worker()
		{
			while (true) {
				var start = Interlocked.Add(ref state.NextIndex, state.ChunkSize) - state.ChunkSize;

				if (start >= state.ToExclusive) {
					break;
				}

				var end = Math.Min(start + state.ChunkSize, state.ToExclusive);

				state.Action.Invoke(start, end, state.Context);
			}

			if (Interlocked.Decrement(ref state.RemainingWorkers) == 0) {
				state.DoneEvent.Set();
			}
		}
	}
}

/*
public static class FastParallel
{
	// TODO: Do I care to support this?  Goes unused in vanilla.
	public static bool ForceTasksOnCallingThread { get; set; } = false;

	public static void For(int fromInclusive, int toExclusive, ParallelForAction callback, object context = null)
	{
		var length = toExclusive - fromInclusive;
		if (length <= 0) {
			return;
		}

		var chunk = Math.Max(1, length / (Environment.ProcessorCount * 4));
		Parallel.ForEach(
			Partitioner.Create(fromInclusive, toExclusive, chunk),
			range => callback(range.Item1, range.Item2, context)
		);
	}
}
*/