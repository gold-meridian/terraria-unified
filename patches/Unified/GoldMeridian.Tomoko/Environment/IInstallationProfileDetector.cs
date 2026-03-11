using System.Threading;
using System.Threading.Tasks;

namespace GoldMeridian.Tomoko.Environment;

public interface IInstallationProfileDetector
{
	ValueTask<IInstallationProfile> DetectAsync(
		CancellationToken cancellationToken = default
	);
}