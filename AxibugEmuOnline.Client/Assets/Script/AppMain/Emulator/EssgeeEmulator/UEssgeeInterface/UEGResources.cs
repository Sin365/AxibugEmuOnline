using Essgee.Metadata;
using UnityEngine;

public class UEGResources : IGameMetaReources
{
    const string ResourceRoot = "Essgee.Unity/";

    public bool GetCartMetadataDatabase(out string loadedData)
    {
        try
        {
            loadedData = Resources.Load<TextAsset>(ResourceRoot + "MetadataDatabase.json").text;
            return true;
        }
        catch
        {
            loadedData = null;
            return false;
        }
    }

    public bool GetDatBytes(string DatName, out byte[] loadedData)
    {
        try
        {
            loadedData = Resources.Load<TextAsset>(ResourceRoot + "Dat/" + DatName).bytes;
            return true;
        }
        catch
        {
            loadedData = null;
            return false;
        }
    }
}