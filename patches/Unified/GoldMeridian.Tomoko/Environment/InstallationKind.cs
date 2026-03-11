namespace GoldMeridian.Tomoko.Environment;

/// <summary>
///		The kind of installation.
/// </summary>
public enum InstallationKind
{
	/// <summary>
	///		Unknown, the fallback option.
	/// </summary>
	Unknown,

	/// <summary>
	///		A portable installation.
	/// </summary>
	Portable,

	/// <summary>
	///		A managed installation installed to a definite location.
	/// </summary>
	Installed,

	/// <summary>
	///		An installation managed externally by an independent package
	///		manager.
	/// </summary>
	PackageManaged,

	/// <summary>
	///		A development build with no proper installation.
	/// </summary>
	Development,
}
