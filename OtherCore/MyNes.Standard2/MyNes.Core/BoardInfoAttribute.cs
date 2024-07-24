using System;

namespace MyNes.Core
{
    internal class BoardInfoAttribute : Attribute
    {
    	public string Name { get; private set; }

    	public int Mapper { get; private set; }

    	public int DefaultPRG_RAM_8KB_BanksCount { get; private set; }

    	public int DefaultCHR_RAM_1KB_BanksCount { get; private set; }

    	public bool Enabled_ppuA12ToggleTimer { get; private set; }

    	public bool PPUA12TogglesOnRaisingEdge { get; private set; }

    	public BoardInfoAttribute(string boardName, int inesMapperNumber)
    	{
    		Name = boardName;
    		Mapper = inesMapperNumber;
    		DefaultPRG_RAM_8KB_BanksCount = 1;
    		DefaultCHR_RAM_1KB_BanksCount = 8;
    		Enabled_ppuA12ToggleTimer = (PPUA12TogglesOnRaisingEdge = false);
    	}

    	public BoardInfoAttribute(string boardName, int inesMapperNumber, int defaultPRG_RAM_8KB_BanksCount, int defaultCHR_RAM_1KB_BanksCount)
    	{
    		Name = boardName;
    		Mapper = inesMapperNumber;
    		DefaultPRG_RAM_8KB_BanksCount = defaultPRG_RAM_8KB_BanksCount;
    		DefaultCHR_RAM_1KB_BanksCount = defaultCHR_RAM_1KB_BanksCount;
    		Enabled_ppuA12ToggleTimer = (PPUA12TogglesOnRaisingEdge = false);
    	}

    	public BoardInfoAttribute(string boardName, int inesMapperNumber, bool Enabled_ppuA12ToggleTimer, bool PPUA12TogglesOnRaisingEdge)
    	{
    		Name = boardName;
    		Mapper = inesMapperNumber;
    		DefaultPRG_RAM_8KB_BanksCount = 1;
    		DefaultCHR_RAM_1KB_BanksCount = 8;
    		this.Enabled_ppuA12ToggleTimer = Enabled_ppuA12ToggleTimer;
    		this.PPUA12TogglesOnRaisingEdge = PPUA12TogglesOnRaisingEdge;
    	}

    	public BoardInfoAttribute(string boardName, int inesMapperNumber, int defaultPRG_RAM_8KB_BanksCount, int defaultCHR_RAM_1KB_BanksCount, bool Enabled_ppuA12ToggleTimer, bool PPUA12TogglesOnRaisingEdge)
    	{
    		Name = boardName;
    		Mapper = inesMapperNumber;
    		DefaultPRG_RAM_8KB_BanksCount = defaultPRG_RAM_8KB_BanksCount;
    		DefaultCHR_RAM_1KB_BanksCount = defaultCHR_RAM_1KB_BanksCount;
    		this.Enabled_ppuA12ToggleTimer = Enabled_ppuA12ToggleTimer;
    		this.PPUA12TogglesOnRaisingEdge = PPUA12TogglesOnRaisingEdge;
    	}
    }
}
