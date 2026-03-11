namespace GoldMeridian.Tomoko.Versioning;

/// <summary>
///		Provides information about the deployment of a game.
/// </summary>
public interface IDeploymentProvider
{
	/// <summary>
	///		Whether this is a debug build.
	/// </summary>
	bool IsDebugBuild { get; }

	/// <summary>
	///		
	/// </summary>
	string Version { get; }
}
