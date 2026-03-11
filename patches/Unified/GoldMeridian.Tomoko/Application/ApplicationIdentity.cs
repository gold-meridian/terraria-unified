using NuGet.Versioning;

namespace GoldMeridian.Tomoko.Application;

/// <summary>
///		The basic, immutable application identity.
/// </summary>
public interface IApplicationIdentity
{
	/// <summary>
	///		A unique application ID, likely corresponding to a package
	///		name or its identification in an update source.
	/// </summary>
	string ApplicationId { get; }

	/// <summary>
	///		The human-friendly application name.
	/// </summary>
	string DisplayName { get; }

	/// <summary>
	///		The application version.
	///		<br />
	///		Semantic Versioning 2.0.0 versions are expected and enforced for
	///		the sake of compatibility with most update managers, packagers, and
	///		package managers.
	/// </summary>
	SemanticVersion Version { get; }

	/// <summary>
	///		The update channel.
	///		<br />
	///		<br />
	///		If <see langword="null"/>, then the default channel should be
	///		assumed.
	/// </summary>
	string? Channel { get; }

	/// <summary>
	///		The absolute path to the executable ran to start this application.
	/// </summary>
	/// <remarks>
	///		It's important to note this will not necessarily point to the .NET
	///		application host, so it cannot be assumed it has anything to do
	///		with any currently loaded assemblies, nor that it's an assembly in
	///		the first place.
	/// </remarks>
	string ExecutablePath { get; }

	/// <summary>
	///		The base directory the application should be launched from.  This
	///		is the expected working directory of the application and is what
	///		is used in portable installations.
	/// </summary>
	string BaseDirectory { get; }
}

public sealed record ApplicationIdentity(
	string ApplicationId,
	string DisplayName,
	SemanticVersion Version,
	string? Channel,
	string ExecutablePath,
	string BaseDirectory
) : IApplicationIdentity;