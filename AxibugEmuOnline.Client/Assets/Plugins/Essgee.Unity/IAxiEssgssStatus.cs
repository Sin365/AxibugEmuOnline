using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class AxiEssgssStatusData
{
    public Dictionary<string, byte[]> MemberData = new Dictionary<string, byte[]>();
    public Dictionary<string, AxiEssgssStatusData_2DArray> Array2DMemberData = new Dictionary<string, AxiEssgssStatusData_2DArray>();
    public Dictionary<string, AxiEssgssStatusData> ClassData = new Dictionary<string, AxiEssgssStatusData>();
}


[Serializable]
public class AxiEssgssStatusData_2DArray
{
    public int rows;
    public int cols;
    public byte[] array1D;
    public AxiEssgssStatusData_2DArray(byte[,] data2D)
    {
        rows = data2D.GetLength(0);
        cols = data2D.GetLength(1);
        array1D = data2D.FlattenByteArray2D();
    }

    public byte[,] Get2DArrayBytesData()
    {
        return array1D.CreateByteArray2D(rows, cols);
    }
}
internal static class AxiEssgssStatusDataExtention
{
    internal static byte[] ToByteArray(this AxiEssgssStatusData data)
    {
        return AxiStatus.saveCover.ToByteArray(data);
    }

    internal static AxiEssgssStatusData ToAxiEssgssStatusData(this byte[] byteArray)
    {
        return AxiStatus.saveCover.ToAxiEssgssStatusData(byteArray);
    }
}
public interface IAxiEssgssStatus
{
    public void LoadAxiStatus(AxiEssgssStatusData data);
    public AxiEssgssStatusData SaveAxiStatus();
}

public interface IAxiEssgssStatusBytesCover
{
    public byte[] ToByteArray(AxiEssgssStatusData data);
    public AxiEssgssStatusData ToAxiEssgssStatusData(byte[] byteArray);
}

internal static class AxiStatus
{
    public static IAxiEssgssStatusBytesCover saveCover { get; private set; }
    public static void Init(IAxiEssgssStatusBytesCover coverter)
    {
        saveCover = coverter;
    }
    // 将任意枚举数组转换为byte[]
    public static byte[] ToByteArray<TEnum>(this TEnum[] enumArray) where TEnum : struct, Enum
    {
        if (!typeof(TEnum).IsEnum) throw new ArgumentException("TEnum must be an enumerated type");

        // 获取枚举的基础类型（通常是Int32）
        Type enumUnderlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        if (enumUnderlyingType != typeof(byte)) // 检查基础类型是否为byte，如果不是，则需要进行额外的处理
        {
            // 如果基础类型不是byte，我们需要决定如何处理。这里我们假设基础类型是int，并将其转换为byte数组，
            // 但要注意，这可能会导致数据丢失（如果枚举值超出了byte的范围）。
            // 一个更安全的方法是使用更大的数据类型（如int[]）作为中间表示。
            // 但为了简化示例，我们将继续并假设所有枚举值都可以安全地转换为byte。
            // 警告：这个假设可能不适用于所有情况！

            // 由于我们假设基础类型是int（这是最常见的枚举基础类型），我们可以直接转换每个枚举值为int，
            // 然后检查它们是否可以安全地转换为byte。如果不能，这里将抛出异常。
            return enumArray.Select(e =>
            {
                int intValue = Convert.ToInt32(e);
                if (intValue < byte.MinValue || intValue > byte.MaxValue)
                    throw new OverflowException($"Enum value {e} ({intValue}) is out of range for byte.");
                return (byte)intValue;
            }).ToArray();
        }
        else
        {
            // 如果基础类型已经是byte，则直接转换
            return Array.ConvertAll(enumArray, e => Convert.ToByte(e));
        }
    }

    // 从byte[]转换为任意枚举数组
    public static TEnum[] ToEnumArray<TEnum>(this byte[] byteArray) where TEnum : struct, Enum
    {
        if (!typeof(TEnum).IsEnum) throw new ArgumentException("TEnum must be an enumerated type");

        // 直接转换每个byte为枚举值
        return Array.ConvertAll(byteArray, b => (TEnum)Enum.ToObject(typeof(TEnum), b));
    }

    // 将单个枚举转换为byte[]（通常这个转换不太常见，因为结果将只有一个字节，但我们还是实现它）
    public static byte[] ToByteArray<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
    {
        if (!typeof(TEnum).IsEnum) throw new ArgumentException("TEnum must be an enumerated type");

        // 获取枚举的基础类型
        Type enumUnderlyingType = Enum.GetUnderlyingType(typeof(TEnum));

        // 检查基础类型是否为byte，如果不是，则进行转换（这里我们假设可以安全地转换为byte，但通常枚举的基础类型是int）
        if (enumUnderlyingType == typeof(byte))
        {
            // 如果基础类型已经是byte，则直接返回包含该字节的数组
            return new[] { Convert.ToByte(enumValue) };
        }
        else if (enumUnderlyingType == typeof(int)) // 最常见的枚举基础类型
        {
            // 将枚举值转换为int，然后检查是否可以安全地转换为byte
            int intValue = Convert.ToInt32(enumValue);
            if (intValue < byte.MinValue || intValue > byte.MaxValue)
                throw new OverflowException($"Enum value {enumValue} ({intValue}) is out of range for byte.");

            // 返回包含转换后的字节的数组
            return new[] { (byte)intValue };
        }
        else
        {
            // 如果基础类型不是byte或int，则抛出异常（或者你可以添加更多的类型检查和处理逻辑）
            throw new NotSupportedException($"The underlying type of the enum {typeof(TEnum).Name} is not supported.");
        }
    }

    // 从byte[]转换为单个枚举（假设byte[]只有一个字节）
    public static TEnum ToEnum<TEnum>(this byte[] byteArray) where TEnum : struct, Enum
    {
        if (byteArray == null || byteArray.Length != 1)
            throw new ArgumentException("The byte array must contain exactly one byte.");

        // 直接从字节转换为枚举值
        return (TEnum)Enum.ToObject(typeof(TEnum), byteArray[0]);
    }

    // ushort[] 转 byte[]
    public static byte[] ToByteArray(this ushort[] ushortArray)
    {
        byte[] byteArray = new byte[ushortArray.Length * 2];
        Buffer.BlockCopy(ushortArray, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    // byte[] 转 ushort[]
    public static ushort[] ToUShortArray(this byte[] byteArray)
    {
        if (byteArray.Length % 2 != 0)
            throw new ArgumentException("byte数组长度必须是偶数");

        ushort[] ushortArray = new ushort[byteArray.Length / 2];
        Buffer.BlockCopy(byteArray, 0, ushortArray, 0, byteArray.Length);
        return ushortArray;
    }



    public static byte[] ToByteArray(this bool[] boolArr)
    {
        byte[] byteArray = new byte[boolArr.Length];
        for (int i = 0; i < byteArray.Length; i++)
        {
            byteArray[i] = (byte)(boolArr[i] ? 1 : 0);
        }
        return byteArray;
    }

    public static bool[] ToBoolArray(this byte[] byteArray)
    {
        bool[] boolArr = new bool[byteArray.Length];
        for (int i = 0; i < byteArray.Length; i++)
        {
            boolArr[i] = byteArray[i] == 0 ? false : true;
        }
        return boolArr;
    }

    public static byte[] FlattenByteArray2D(this byte[,] array2D)
    {
        int rows = array2D.GetLength(0);
        int cols = array2D.GetLength(1);
        byte[] array1D = new byte[rows * cols];

        int index = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                array1D[index++] = array2D[i, j];
            }
        }

        return array1D;
    }
    public static byte[,] CreateByteArray2D(this byte[] array1D, int rows, int cols)
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

    public static byte[] FlattenByteArray3D(this byte[,,] array3D)
    {
        int layer = array3D.GetLength(0);
        int rows = array3D.GetLength(1);
        int cols = array3D.GetLength(2);
        byte[] array1D = new byte[layer * rows * cols];

        int index = 0;
        for (int i = 0; i < layer; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                for (int k = 0; k < cols; k++)
                {
                    array1D[index++] = array3D[i, j, k];
                }
            }
        }

        return array1D;
    }

    public static byte[,,] CreateByteArray3D(this byte[] array1D, int layer, int rows, int cols)
    {
        if (array1D.Length != layer * rows * cols)
        {
            throw new ArgumentException("The length of the 1D array does not match the specified dimensions for the 3D array.");
        }

        byte[,,] array3D = new byte[layer, rows, cols];

        int index = 0;
        for (int i = 0; i < layer; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                for (int k = 0; k < cols; k++)
                {
                    array3D[i, j, k] = array1D[index++];
                }
            }
        }

        return array3D;
    }
}
