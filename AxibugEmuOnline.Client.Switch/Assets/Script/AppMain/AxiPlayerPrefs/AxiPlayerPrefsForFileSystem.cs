using System.Collections.Generic;
using UnityEngine;

public class AxiPlayerPrefsForFileSystem : AxiPlayerPrefsFileBase
{
    public AxiPlayerPrefsForFileSystem() : base(LoadData, SaveData)
    {
        Debug.Log($"AxiPlayerPrefsForFileSystem Init");
    }

    protected static Dictionary<string, AxiPlayerPrefsKeyValye> LoadData()
    {
        if (!AxiIO.AxiIO.io.file_Exists(AxiPlayerPrefsFilePath))
            return new Dictionary<string, AxiPlayerPrefsKeyValye>();
        else
        {
            string outputData = string.Empty;
            byte[] loadedData = AxiIO.AxiIO.io.file_ReadAllBytes(AxiPlayerPrefsFilePath);
            if (loadedData != null && loadedData.Length != 0)
            {
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream(loadedData))
                {
                    using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
                    {
                        outputData = reader.ReadString();
                    }
                }
            }
            if (string.IsNullOrEmpty(outputData))
                return new Dictionary<string, AxiPlayerPrefsKeyValye>();
            return AxiPlayerPrefsFileBase.JsonStrToData(outputData);
        }
    }

    protected static void SaveData(Dictionary<string, AxiPlayerPrefsKeyValye> data)
    {
        string jsonStr = AxiPlayerPrefsFileBase.DataToJsonStr(data);
        byte[] dataByteArray;
        using (System.IO.MemoryStream stream = new System.IO.MemoryStream(jsonStr.Length * sizeof(char)))
        {
            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(stream);
            binaryWriter.Write(jsonStr);
            dataByteArray = stream.GetBuffer();
            stream.Close();
        }
        AxiIO.AxiIO.io.file_WriteAllBytes(AxiPlayerPrefsFilePath, dataByteArray, false);
    }
}