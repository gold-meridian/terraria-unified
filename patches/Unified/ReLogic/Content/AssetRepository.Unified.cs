using System;
using System.Linq;
using System.Threading;

namespace ReLogic.Content;

partial class AssetRepository
{
	internal struct ContinuationScheduler
	{
		public readonly IAsset asset;
		public readonly AssetRepository repository;

		internal ContinuationScheduler(IAsset asset, AssetRepository repository)
		{
			this.asset = asset;
			this.repository = repository;
		}

		public void OnCompleted(Action continuation)
		{
			if (asset == null) {
				throw new Exception("Main thread transition requested without an asset");
			}

			continuation = continuation.OnlyRunnableOnce();
			repository._assetTransferQueue.Enqueue(continuation);
			asset.Continuation = continuation;
		}
	}

	private static Thread _mainThread;

	public static bool IsMainThread => Thread.CurrentThread == _mainThread;

	public static event Action<TimeSpan> OnBlockingLoadCompleted;

	public static void SetMainThread()
	{
		if (_mainThread != null) {
			throw new InvalidOperationException("Main thread already set");
		}

		_mainThread = Thread.CurrentThread;
	}

	public static void ThrowIfNotMainThread()
	{
		if (!IsMainThread) {
			throw new Exception("Must be on main thread");
		}
	}

	private void Invoke(Action action)
	{
		// Skip loading assets if this is a dedicated server; this avoids
		// deadlocks on waiting for queue to empty.
		if (_readers == null) {
			_assetTransferQueue.Clear();
			return;
		}

		var evt = new ManualResetEvent(false);
		_assetTransferQueue.Enqueue(() => { action(); evt.Set(); });
		evt.WaitOne();
	}

	public IAsset[] GetLoadedAssets()
	{
		lock (_requestLock) {
			return _assets.Values.ToArray();
		}
	}
}
