using System;
using System.Threading;

namespace ReLogic.Content;

public static class RunOnceAction
{
	public static Action OnlyRunnableOnce(this Action action)
	{
		return () => Interlocked.Exchange(ref action, null)?.Invoke();
	}
}
