using Essgee.Emulation.Machines;
using Essgee.Exceptions;
using Essgee.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace Essgee.Emulation
{
    public static class CartridgeLoader
    {
        static Dictionary<string, Type> fileExtensionSystemDictionary;

        static CartridgeLoader()
        {
            fileExtensionSystemDictionary = new Dictionary<string, Type>();
            foreach (var machineType in Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(IMachine).IsAssignableFrom(x) && !x.IsInterface).OrderBy(x => x.GetCustomAttribute<MachineIndexAttribute>()?.Index))
            {
                if (machineType == null) continue;

                var instance = (IMachine)Activator.CreateInstance(machineType);
                foreach (var extension in instance.FileFilter.Extension.Split(';'))
                    fileExtensionSystemDictionary.Add(extension, machineType);
            }
        }

        public static (Type, byte[]) Load(string fileName, string fileType)
        {
            Type machineType = null;
            byte[] romData = null;

            if (!File.Exists(fileName))
                throw new CartridgeLoaderException($"{fileType} file not found.");

            try
            {
                var fileExtension = Path.GetExtension(fileName);
                if (fileExtension == ".zip")
                {
                    using (var zip = ZipFile.Open(fileName, ZipArchiveMode.Read))
                    {
                        foreach (var entry in zip.Entries)
                        {
                            var entryExtension = Path.GetExtension(entry.Name);
                            if (fileExtensionSystemDictionary.ContainsKey(entryExtension))
                            {
                                machineType = fileExtensionSystemDictionary[entryExtension];
                                using (var stream = entry.Open())
                                {
                                    romData = new byte[entry.Length];
                                    stream.Read(romData, 0, romData.Length);
                                }
                                break;
                            }
                        }
                    }
                }
                else if (fileExtensionSystemDictionary.ContainsKey(fileExtension))
                {
                    machineType = fileExtensionSystemDictionary[fileExtension];
                    romData = File.ReadAllBytes(fileName);
                }
            }
            catch (Exception ex) when (!AppEnvironment.DebugMode)
            {
                throw new CartridgeLoaderException("File load error", ex);
            }

            if (machineType == null)
                throw new CartridgeLoaderException($"File could not be recognized as {fileType}.");

            return (machineType, romData);
        }
    }
}
