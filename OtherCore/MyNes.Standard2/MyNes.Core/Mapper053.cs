using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Unknown", 53)]
    [HassIssues]
    internal class Mapper053 : Board
    {
    	private byte[] regs = new byte[2];

    	private bool epromFirst;

    	internal override string Issues => MNInterfaceLanguage.IssueMapper53;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		regs = new byte[2];
    		epromFirst = true;
    		Switch08KPRG(0, PRGArea.Area6000);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		regs[1] = data;
    		UpdatePrg();
    	}

    	internal override void WriteSRM(ref ushort address, ref byte data)
    	{
    		regs[0] = data;
    		UpdatePrg();
    		Switch01KNMTFromMirroring(((data & 0x20) == 32) ? Mirroring.Horz : Mirroring.Vert);
    	}

    	private void UpdatePrg()
    	{
    		int num = (regs[0] << 3) & 0x78;
    		Switch08KPRG(((num << 1) | 0xF) + (epromFirst ? 4 : 0), PRGArea.Area6000);
    		Switch16KPRG(((regs[0] & 0x10) == 16) ? ((num | (regs[1] & 7)) + (epromFirst ? 2 : 0)) : ((!epromFirst) ? 128 : 0), PRGArea.Area8000);
    		Switch16KPRG(((regs[0] & 0x10) == 16) ? ((num | 7) + (epromFirst ? 2 : 0)) : (epromFirst ? 1 : 129), PRGArea.AreaC000);
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(regs);
    		stream.Write(epromFirst);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		stream.Read(regs, 0, 2);
    		epromFirst = stream.ReadBoolean();
    	}
    }
}
