using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;

namespace MyNes.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
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
