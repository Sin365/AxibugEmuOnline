using Essgee.Exceptions;
using Essgee.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Essgee.Emulation
{
    public static class SaveStateHandler
    {
        public static string ExpectedVersion = $"ESGST{new Version(EmuStandInfo.ProductVersion).Major:D3}";

        public static Dictionary<string, dynamic> Load(Stream stream, string machineName)
        {
            stream.Position = 0;

            using (var reader = new BinaryReader(stream))
            {
                /* Read and check version string */
                var version = Encoding.ASCII.GetString(reader.ReadBytes(ExpectedVersion.Length));
                if (version != ExpectedVersion) throw new EmulationException("Unsupported savestate version");

                /* Read and check filesize */
                var filesize = reader.ReadUInt32();
                if (filesize != reader.BaseStream.Length) throw new EmulationException("Savestate filesize mismatch");

                /* Read CRC32 */
                var crc32 = reader.ReadUInt32();

                /* Read and check machine ID */
                var machineId = Encoding.ASCII.GetString(reader.ReadBytes(16));
                if (machineId != GenerateMachineIdString(machineName)) throw new EmulationException("Savestate machine mismatch");

                /* Check CRC32 */
                using (var stateStream = new MemoryStream())
                {
                    reader.BaseStream.CopyTo(stateStream);
                    stateStream.Position = 0;
                    var expectedCrc32 = Crc32.Calculate(stateStream);
                    if (crc32 != expectedCrc32) throw new EmulationException("Savestate checksum error");

                    /* Read state data */
                    var binaryFormatter = new BinaryFormatter();
                    return (binaryFormatter.Deserialize(stateStream) as Dictionary<string, dynamic>);
                }
            }
        }

        public static void Save(Stream stream, string machineName, Dictionary<string, dynamic> state)
        {
            using (var writer = new BinaryWriter(new MemoryStream()))
            {
                /* Write version string */
                writer.Write(Encoding.ASCII.GetBytes(ExpectedVersion));

                /* Write filesize placeholder */
                var filesizePosition = writer.BaseStream.Position;
                writer.Write(uint.MaxValue);

                /* Write CRC32 placeholder */
                var crc32Position = writer.BaseStream.Position;
                writer.Write(uint.MaxValue);

                /* Write machine ID */
                writer.Write(Encoding.ASCII.GetBytes(GenerateMachineIdString(machineName)));

                /* Current position is end of header, store for later */
                var headerSize = writer.BaseStream.Position;

                /* Write state data */
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(writer.BaseStream, state);

                /* Write filesize */
                var lastOffset = writer.BaseStream.Position;
                writer.BaseStream.Position = filesizePosition;
                writer.Write((uint)writer.BaseStream.Length);
                writer.BaseStream.Position = lastOffset;

                /* Calculate CRC32 for state data, then write CRC32 */
                lastOffset = writer.BaseStream.Position;

                writer.BaseStream.Position = 0;
                var crc32 = Crc32.Calculate(writer.BaseStream, (int)headerSize, (int)(writer.BaseStream.Length - headerSize));

                writer.BaseStream.Position = crc32Position;
                writer.Write(crc32);
                writer.BaseStream.Position = lastOffset;

                /* Copy to file */
                writer.BaseStream.Position = 0;
                writer.BaseStream.CopyTo(stream);
            }
        }

        private static string GenerateMachineIdString(string machineId)
        {
            return machineId.Substring(0, Math.Min(machineId.Length, 16)).PadRight(16);
        }

        //public static void PerformSetState(object obj, Dictionary<string, dynamic> state)

        public static void PerformSetState(object obj, Dictionary<string, object> state)
        {
            if (obj != null)
            {
                /* Restore property values from state */
                foreach (var prop in obj.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttributes(typeof(StateRequiredAttribute), false).Length != 0))
                {
                    prop.SetValue(obj, state[prop.Name]);
                }

                /* Restore field values from state */
                foreach (var field in obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttributes(typeof(StateRequiredAttribute), false).Length != 0))
                {
                    field.SetValue(obj, state[field.Name]);
                }
            }
        }

        //public static Dictionary<string, dynamic> PerformGetState(object obj)
        public static Dictionary<string, object> PerformGetState(object obj)
        {
            //var state = new Dictionary<string, dynamic>();
            var state = new Dictionary<string, object>();

            if (obj != null)
            {
                /* Copy property values to state */
                foreach (var prop in obj.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttributes(typeof(StateRequiredAttribute), false).Length != 0))
                {
                    state.Add(prop.Name, prop.GetValue(obj));
                }

                /* Copy field values to state */
                foreach (var field in obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttributes(typeof(StateRequiredAttribute), false).Length != 0))
                {
                    state.Add(field.Name, field.GetValue(obj));
                }
            }

            return state;
        }
    }
}
