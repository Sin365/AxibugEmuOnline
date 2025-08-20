using cpu.m6502;

namespace MAME.Core
{
    public unsafe partial class Dataeast
    {
        public static void SaveStateBinary_pcktgal(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(dsw);
            writer.Write(basebankmain1);
            writer.Write(basebankmain2);
            writer.Write(basebanksnd);
            writer.Write(msm5205next);
            writer.Write(toggle);
            for (i = 0; i < 0x200; i++)
            {
                writer.Write(Palette.entry_color[i]);
            }
            writer.Write(Memory.mainram, 0, 0x800);
            writer.Write(Generic.videoram, 0, 0x800);
            writer.Write(Generic.spriteram, 0, 0x200);
            writer.Write(Memory.audioram, 0, 0x800);
            M6502.mm1[0].SaveStateBinary(writer);
            M6502.mm1[1].SaveStateBinary(writer);
            Cpuint.SaveStateBinary(writer);
            writer.Write(EmuTimer.global_basetime.seconds);
            writer.Write(EmuTimer.global_basetime.attoseconds);
            Video.SaveStateBinary(writer);
            writer.Write(Sound.last_update_second);
            Cpuexec.SaveStateBinary(writer);
            EmuTimer.SaveStateBinary(writer);
            AY8910.AA8910[0].SaveStateBinary(writer);
            YM2203.FF2203[0].SaveStateBinary(writer);
            YM3812.SaveStateBinary(writer);
            MSM5205.mm1[0].SaveStateBinary(writer);
            writer.Write(Sound.latched_value[0]);
            writer.Write(Sound.utempdata[0]);
            writer.Write(AY8910.AA8910[0].stream.output_sampindex);
            writer.Write(AY8910.AA8910[0].stream.output_base_sampindex);
            writer.Write(YM2203.FF2203[0].stream.output_sampindex);
            writer.Write(YM2203.FF2203[0].stream.output_base_sampindex);
            writer.Write(Sound.ym3812stream.output_sampindex);
            writer.Write(Sound.ym3812stream.output_base_sampindex);
            writer.Write(MSM5205.mm1[0].voice.stream.output_sampindex);
            writer.Write(MSM5205.mm1[0].voice.stream.output_base_sampindex);
            writer.Write(Sound.mixerstream.output_sampindex);
            writer.Write(Sound.mixerstream.output_base_sampindex);
        }
        public static void LoadStateBinary_pcktgal(System.IO.BinaryReader reader)
        {
            int i;
            dsw = reader.ReadByte();
            basebankmain1 = reader.ReadInt32();
            basebankmain2 = reader.ReadInt32();
            basebanksnd = reader.ReadInt32();
            msm5205next = reader.ReadInt32();
            toggle = reader.ReadInt32();
            for (i = 0; i < 0x200; i++)
            {
                Palette.entry_color[i] = reader.ReadUInt32();
            }
            Memory.Set_mainram(reader.ReadBytes(0x800));
            Generic.videoram_set = reader.ReadBytes(0x800);
            Generic.spriteram_set = reader.ReadBytes(0x200);
            Memory.Set_audioram(reader.ReadBytes(0x800));
            M6502.mm1[0].LoadStateBinary(reader);
            M6502.mm1[1].LoadStateBinary(reader);
            Cpuint.LoadStateBinary(reader);
            EmuTimer.global_basetime.seconds = reader.ReadInt32();
            EmuTimer.global_basetime.attoseconds = reader.ReadInt64();
            Video.LoadStateBinary(reader);
            Sound.last_update_second = reader.ReadInt32();
            Cpuexec.LoadStateBinary(reader);
            EmuTimer.LoadStateBinary(reader);
            AY8910.AA8910[0].LoadStateBinary(reader);
            YM2203.FF2203[0].LoadStateBinary(reader);
            YM3812.LoadStateBinary(reader);
            MSM5205.mm1[0].LoadStateBinary(reader);
            Sound.latched_value[0] = reader.ReadUInt16();
            Sound.utempdata[0] = reader.ReadUInt16();
            AY8910.AA8910[0].stream.output_sampindex = reader.ReadInt32();
            AY8910.AA8910[0].stream.output_base_sampindex = reader.ReadInt32();
            YM2203.FF2203[0].stream.output_sampindex = reader.ReadInt32();
            YM2203.FF2203[0].stream.output_base_sampindex = reader.ReadInt32();
            Sound.ym3812stream.output_sampindex = reader.ReadInt32();
            Sound.ym3812stream.output_base_sampindex = reader.ReadInt32();
            MSM5205.mm1[0].voice.stream.output_sampindex = reader.ReadInt32();
            MSM5205.mm1[0].voice.stream.output_base_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_sampindex = reader.ReadInt32();
            Sound.mixerstream.output_base_sampindex = reader.ReadInt32();
        }
    }
}
