using GoldMeridian.Tomoko.Application;
using GoldMeridian.Tomoko.Environment;
using GoldMeridian.Tomoko.Updating;

namespace GoldMeridian.Tomoko.Hosting;

public interface IApplicationHostingContext
{
	IApplicationIdentity Identity { get; }

	IInstallationProfile Installation { get; }

	IUpdateProvider UpdateProvider { get; }
}

public sealed record ApplicationHostingContext(
	IApplicationIdentity Identity,
	IInstallationProfile Installation,
	IUpdateProvider UpdateProvider
) : IApplicationHostingContext;