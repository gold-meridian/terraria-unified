using System;
using System.Runtime.CompilerServices;

namespace ReLogic.Content;

public readonly struct MainThreadCreationContext : INotifyCompletion
{
	private readonly AssetRepository.ContinuationScheduler _continuationScheduler;

	public bool IsCompleted => AssetRepository.IsMainThread;

	internal MainThreadCreationContext(AssetRepository.ContinuationScheduler continuationScheduler)
	{
		_continuationScheduler = continuationScheduler;
	}

	public MainThreadCreationContext GetAwaiter()
	{
		return this;
	}

	public void GetResult() { }

	public void OnCompleted(Action action)
	{
		_continuationScheduler.OnCompleted(action);
	}
}
