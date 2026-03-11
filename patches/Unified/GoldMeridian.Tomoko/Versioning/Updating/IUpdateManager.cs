using System;
using System.Threading;
using System.Threading.Tasks;

namespace GoldMeridian.Tomoko.Versioning.Updating;

/// <summary>
///		Responsible for managing game versions, handling checking for and
///		installing updates, and may expect to handle game launches.
///		<br />
///		Additionally responsible for managing game sources and installation
///		configurations (portable install, package-managed state, etc.).
/// </summary>
public interface IUpdateManager : IDisposable
{
	/// <summary>
	///		Whether the application is managed by an external package manager.
	/// </summary>
	bool IsPackageManaged { get; }

	/// <summary>
	///		Handles an initial program launch.
	/// </summary>
	/// <returns>
	///		Whether the update manager expects the process to exit after
	///		finishing.
	///	</returns>
	bool HandleLaunch(string[] args);

	/// <summary>
	///		Asynchronously checks for any available updates.
	/// </summary>
	Task<bool> CheckForUpdateAsync(CancellationToken cancellationToken = default);
}
