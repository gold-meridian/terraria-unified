using System.Threading;
using System.Threading.Tasks;

namespace GoldMeridian.Tomoko.Application;

public interface IApplicationIdentityProvider
{
	ValueTask<IApplicationIdentity> GetIdentityAsync(
		CancellationToken cancellationToken = default
	);
}