namespace sGBA;

public static class GamePreferences
{
	[Title( "Reproduce classic feel" )]
	[Group( "Game Screen" )]
	[ConVar( "sgba.classicfeel", ConVarFlags.Saved )]
	public static bool ReproduceClassicFeel { get; set; } = true;

	[Title( "Display with small screen" )]
	[Group( "Game Screen" )]
	[ConVar( "sgba.smallscreen", ConVarFlags.Saved )]
	public static bool DisplayWithSmallScreen { get; set; }

	public static void SetReproduceClassicFeel( bool value )
	{
		if ( ReproduceClassicFeel == value )
			return;

		ReproduceClassicFeel = value;
		EmulatorComponent.Current?.ApplyDisplaySettings();
	}

	public static void SetDisplayWithSmallScreen( bool value )
	{
		if ( DisplayWithSmallScreen == value )
			return;

		DisplayWithSmallScreen = value;
	}
}
