using System;
using AxibugProtobuf;
using AxibugEmuOnline.Client.Common;

/// <summary>
/// ÓÃProtoBuff¼ÇÂ¼´æµµ
/// </summary>
public class UEGSaveByteConvert : IAxiEssgssStatusBytesCover
{
    public AxiEssgssStatusData ToAxiEssgssStatusData(byte[] byteArray)
    {
        pb_AxiEssgssStatusData dataFromPool = ProtoBufHelper.DeSerizlizeFromPool<pb_AxiEssgssStatusData>(byteArray);
        AxiEssgssStatusData data = pbAxiEssgssStatusDataToSrcData(dataFromPool);
        ProtoBufHelper.ReleaseToPool(dataFromPool);
        return data;
    }
    public byte[] ToByteArray(AxiEssgssStatusData data)
    {
        pb_AxiEssgssStatusData pbdata = AxiEssgssStatusDataToPbData(data);
        return ProtoBufHelper.Serizlize(pbdata);
    }
    static AxiEssgssStatusData pbAxiEssgssStatusDataToSrcData(pb_AxiEssgssStatusData pbdata)
    {
        AxiEssgssStatusData data = new AxiEssgssStatusData();
        data.MemberData = new System.Collections.Generic.Dictionary<string, byte[]>();
        foreach (var memberData in pbdata.MemberData)
        {
            data.MemberData[memberData.KeyName] = memberData.Raw.ToByteArray();
        }
        foreach (var arrayData in pbdata.Array2DMemberData)
        {
            data.Array2DMemberData[arrayData.KeyName] = new AxiEssgssStatusData_2DArray(CreateByteArray2D(arrayData.Raw.ToByteArray(), arrayData.Rows, arrayData.Cols));
        }
        foreach (var classData in pbdata.ClassData)
        {
            data.ClassData[classData.KeyName] = pbAxiEssgssStatusDataToSrcData(classData.ClassData);
        }
        return data;
    }
    static pb_AxiEssgssStatusData AxiEssgssStatusDataToPbData(AxiEssgssStatusData data)
    {
        pb_AxiEssgssStatusData pbdata = new pb_AxiEssgssStatusData();
        foreach (var memberdata in data.MemberData)
        {
            pbdata.MemberData.Add(new pb_AxiEssgssStatusData_ByteData()
            {
                KeyName = memberdata.Key,
                Raw = Google.Protobuf.ByteString.CopyFrom(memberdata.Value)
            });
        }

        foreach (var arrayData in data.Array2DMemberData)
        {
            pbdata.Array2DMemberData.Add(new pb_AxiEssgssStatusData_2DArray()
            {
                KeyName = arrayData.Key,
                Cols = arrayData.Value.cols,
                Rows = arrayData.Value.rows,
                Raw = Google.Protobuf.ByteString.CopyFrom(arrayData.Value.array1D)
            });
        }

        foreach (var classdata in data.ClassData)
        {
            pbdata.ClassData.Add(new pb_AxiEssgssStatusData_ClassData()
            {
                KeyName = classdata.Key,
                ClassData = AxiEssgssStatusDataToPbData(classdata.Value),
            });
        }

        return pbdata;
    }
    static byte[,] CreateByteArray2D(byte[] array1D, int rows, int cols)
    {
        if (array1D.Length != rows * cols)
        {
            throw new ArgumentException("The length of the 1D array does not match the specified dimensions for the 2D array.");
        }
        byte[,] array2D = new byte[rows, cols];
        int index = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                array2D[i, j] = array1D[index++];
            }
        }
        return array2D;
    }
}
