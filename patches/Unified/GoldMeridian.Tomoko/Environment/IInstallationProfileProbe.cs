using System.Threading;
using System.Threading.Tasks;

namespace GoldMeridian.Tomoko.Environment;

public interface IInstallationProfileProbe
{
	int Priority { get; }

	ValueTask<IInstallationProfile?> TryDetectAsync(
		CancellationToken cancellationToken = default
	);
}