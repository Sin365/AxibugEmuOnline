namespace sGBA;

public static class WirelessAdapterGames
{
	private static readonly HashSet<string> GameCodes =
	[
		"BPR", // Pokémon FireRed
		"BPG", // Pokémon LeafGreen
		"BPE", // Pokémon Emerald
		// You can add more as needed through a PR, I'm too lazy to research any further
	];

	public static bool UsesWirelessAdapter( string gameCode )
	{
		if ( string.IsNullOrEmpty( gameCode ) || gameCode.Length < 3 )
			return false;

		return GameCodes.Contains( gameCode[..3] );
	}

	public static NetworkManager.SessionMode DetectMode( string gameCode ) =>
		UsesWirelessAdapter( gameCode )
			? NetworkManager.SessionMode.WirelessAdapter
			: NetworkManager.SessionMode.LinkCable;
}
