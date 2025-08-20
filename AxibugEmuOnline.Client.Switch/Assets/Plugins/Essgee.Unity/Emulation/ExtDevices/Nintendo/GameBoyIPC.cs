using Essgee.EventArguments;
using System;
using System.ComponentModel;
using System.IO.MemoryMappedFiles;

namespace Essgee.Emulation.ExtDevices.Nintendo
{
    [Description("Game Boy (Link Cable)")]
    //todo Unity [ElementPriority(1)]
    public class GameBoyIPC : ISerialDevice
    {
        // TODO: ensure correct mmf/accessor disposal?

        const string ipcName = "EssgeeGBLink";
        const int ipcLength = 16;

        const int ipcBaseOffsetSerialData = 0;
        //

        public event EventHandler<SaveExtraDataEventArgs> SaveExtraData;
        protected virtual void OnSaveExtraData(SaveExtraDataEventArgs e) { SaveExtraData?.Invoke(this, e); }

        MemoryMappedFile mmf;
        MemoryMappedViewAccessor accessor;

        bool ipcConnectionExists;
        int ipcOffsetSelf, ipcOffsetRemote;

        public GameBoyIPC()
        {
            mmf = null;
            accessor = null;

            ipcConnectionExists = false;
            ipcOffsetSelf = ipcOffsetRemote = 0;
        }

        public void Initialize()
        {
            //
        }

        public void Shutdown()
        {
            if (ipcConnectionExists)
            {
                accessor.Flush();
                accessor.Dispose();
                mmf.Dispose();

                ipcConnectionExists = false;
            }
        }

        private void EstablishIPCConnection()
        {
            if (ipcConnectionExists) return;

            try
            {
                // Try to open existing mapped file; if it exists, assume other instance is first machine
                mmf = MemoryMappedFile.OpenExisting(ipcName);
                ipcOffsetSelf = ipcBaseOffsetSerialData + 1;
                ipcOffsetRemote = ipcBaseOffsetSerialData + 0;
            }
            catch (Exception ex)
            {
                // Mapped file does not yet exist, create file and assume this instance is first machine
                mmf = MemoryMappedFile.CreateOrOpen(ipcName, ipcLength);
                ipcOffsetSelf = ipcBaseOffsetSerialData + 0;
                ipcOffsetRemote = ipcBaseOffsetSerialData + 1;
            }
            accessor = mmf.CreateViewAccessor(0, ipcLength, MemoryMappedFileAccess.ReadWrite);

            ipcConnectionExists = true;
        }

        public byte ExchangeBit(int left, byte data)
        {
            if (!ipcConnectionExists) EstablishIPCConnection();

            accessor.Write(ipcOffsetSelf, data);
            return accessor.ReadByte(ipcOffsetRemote);
        }
    }
}
