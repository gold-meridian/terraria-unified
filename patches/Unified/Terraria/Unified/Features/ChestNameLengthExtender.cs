namespace Terraria.Unified.Features;

internal static class ChestNameLengthExtender
{
	public static int VanillaMaxChestNameLength => Chest.MaxNameLength;

	// Can be an arbitrary number...
	public static int ModdedMaxChestNameLength => 63;

	// TODO: Conditionally set this based on possible preferences.  i.e. would
	// it be unwise to attempt to bypass this on a TShock server?
	public static bool BypassServerNameLengthChecks => true;

	public static int PreferredMaxChestNameLength => BypassServerNameLengthChecks ? ModdedMaxChestNameLength : VanillaMaxChestNameLength;
}
