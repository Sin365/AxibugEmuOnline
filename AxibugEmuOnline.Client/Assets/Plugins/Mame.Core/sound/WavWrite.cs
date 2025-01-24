using System.IO;

namespace MAME.Core
{
    public class WavWrite
    {
        public static FileStream mWaveFile = null;
        private static BinaryWriter mWriter = null;
        private static int mSampleCount = 0;
        public static void CreateSoundFile(string filename)
        {
            mWaveFile = new FileStream(filename, FileMode.Create);
            mWriter = new BinaryWriter(mWaveFile);
            /**************************************************************************
             Hereiswherethefilewillbecreated.A
             wavefileisaRIFFfile,whichhaschunks
             ofdatathatdescribewhatthefilecontains.
             AwaveRIFFfileisputtogetherlikethis:
             The12byteRIFFchunkisconstructedlikethis:
             Bytes0-3:　'R''I''F''F'
             Bytes4-7:　Lengthoffile,minusthefirst8bytesoftheRIFFdescription.
                      (4bytesfor"WAVE"+24bytesforformatchunklength+
                      8bytesfordatachunkdescription+actualsampledatasize.)
              Bytes8-11:'W''A''V''E'
              The24byteFORMATchunkisconstructedlikethis:
              Bytes0-3:'f''m''t'''
              Bytes4-7:Theformatchunklength.Thisisalways16.
              Bytes8-9:Filepadding.Always1.
              Bytes10-11:Numberofchannels.Either1formono,　or2forstereo.
              Bytes12-15:Samplerate.
              Bytes16-19:Numberofbytespersecond.
              Bytes20-21:Bytespersample.1for8bitmono,2for8bitstereoor
                      16bitmono,4for16bitstereo.
              Bytes22-23:Numberofbitspersample.
              TheDATAchunkisconstructedlikethis:
              Bytes0-3:'d''a''t''a'
              Bytes4-7:Lengthofdata,inbytes.
              Bytes8-:Actualsampledata.
             ***************************************************************************/
            char[] ChunkRiff = { 'R', 'I', 'F', 'F' };
            char[] ChunkType = { 'W', 'A', 'V', 'E' };
            char[] ChunkFmt = { 'f', 'm', 't', ' ' };
            char[] ChunkData = { 'd', 'a', 't', 'a' };
            short shPad = 1;　　　　　　　　//Filepadding
            int nFormatChunkLength = 0x10;　//Formatchunklength.
            int nLength = 0;　　　　　　　　//Filelength,minusfirst8bytesofRIFFdescription.Thiswillbefilledinlater.
            mSampleCount = 0;
            //RIFF
            mWriter.Write(ChunkRiff);
            mWriter.Write(nLength);
            //WAVE
            mWriter.Write(ChunkType);
            mWriter.Write(ChunkFmt);
            mWriter.Write(nFormatChunkLength);
            mWriter.Write(shPad);
            mWriter.Write((short)2);
            mWriter.Write(48000);
            mWriter.Write(192000);
            mWriter.Write((short)4);
            mWriter.Write((short)16);
            //data
            mWriter.Write(ChunkData);
            mWriter.Write((int)0);
        }
        public static void CloseSoundFile()
        {
            mWriter.Seek(4, SeekOrigin.Begin);
            mWriter.Write((int)(mSampleCount + 36));
            mWriter.Seek(40, SeekOrigin.Begin);
            mWriter.Write(mSampleCount);
            mWriter.Close();
            mWaveFile.Close();
            mWriter = null;
            mWaveFile = null;
        }
        public static void wav_add_data_16(byte[] bb1, int length)
        {
            mWriter.Write(bb1, 0, length);
            mSampleCount += length;
            mWriter.Flush();
        }
    }
}