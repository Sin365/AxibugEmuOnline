using Sandbox.Network;

namespace sGBA;

public static class LobbyDataKeys
{
	public const string RomTitle = "rom";
	public const string GameCode = "code";
	public const string RomSha1 = "sha1";
	public const string Visibility = "vis";
	public const string HostName = "host";
	public const string Mode = "mode";
}

public static class LobbyMetadata
{
	public static string GetRomTitle( this LobbyInformation lobby ) =>
		lobby.Get( LobbyDataKeys.RomTitle, string.Empty );

	public static string GetGameCode( this LobbyInformation lobby ) =>
		lobby.Get( LobbyDataKeys.GameCode, string.Empty );

	public static string GetRomSha1( this LobbyInformation lobby ) =>
		lobby.Get( LobbyDataKeys.RomSha1, string.Empty );

	public static string GetHostName( this LobbyInformation lobby ) =>
		lobby.Get( LobbyDataKeys.HostName, string.Empty );

	public static SessionVisibility GetVisibility( this LobbyInformation lobby )
	{
		var raw = lobby.Get( LobbyDataKeys.Visibility, "0" );
		return int.TryParse( raw, out var v ) ? (SessionVisibility)v : SessionVisibility.Public;
	}

	public static NetworkManager.SessionMode GetMode( this LobbyInformation lobby )
	{
		var raw = lobby.Get( LobbyDataKeys.Mode, "0" );
		return int.TryParse( raw, out var v ) ? (NetworkManager.SessionMode)v : NetworkManager.SessionMode.WirelessAdapter;
	}
}
