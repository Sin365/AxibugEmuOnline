using System.Collections.Generic;

namespace MyNes.Core
{
    public struct NesCartDatabaseGameInfo
    {
    	public string Game_Name;

    	public string Game_AltName;

    	public string Game_Class;

    	public string Game_Catalog;

    	public string Game_Publisher;

    	public string Game_Developer;

    	public string Game_Region;

    	public string Game_Players;

    	public string Game_ReleaseDate;

    	public List<NesCartDatabaseCartridgeInfo> Cartridges;

    	public static NesCartDatabaseGameInfo Empty => default(NesCartDatabaseGameInfo);
    }
}
