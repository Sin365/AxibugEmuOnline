#if UNITY_EDITOR
using AxibugEmuOnline.Client.Network;
using Google.Protobuf;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;

public static class GenCode
{
    const string TEMPLATE = @"
namespace [NAMESPACE]
{
    public sealed partial class [CLASSNAME] : IResetable
    {
        public void Reset()
        {[RESETCODE]
        }
    }
}
";
    [MenuItem("Axibug移植工具/生成Protobuff Reset代码文件")]
    public static void GenResetCode()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using AxibugEmuOnline.Client.Network;");

        var msgInterfaceType = typeof(IMessage);
        var protoMsgTypes = typeof(NetMsg).Assembly.GetExportedTypes().Where(t => msgInterfaceType.IsAssignableFrom(t)).ToArray();
        var flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.GetProperty;
        foreach (var msgType in protoMsgTypes)
        {
            if (msgType.IsAbstract) continue;

            var props = msgType.GetProperties(flag);
            StringBuilder resetCodeSB = new StringBuilder();
            foreach (var prop in props)
            {
                resetCodeSB.AppendLine();
                if (prop.PropertyType.IsValueType)
                    resetCodeSB.Append($"\t\t\t{prop.Name} = default;");
                else if (typeof(IBufferMessage).IsAssignableFrom(prop.PropertyType))
                    resetCodeSB.Append($"\t\t\t{prop.Name}?.Reset();");
                else if (typeof(IList).IsAssignableFrom(prop.PropertyType))
                    resetCodeSB.Append($"\t\t\t{prop.Name}?.Clear();");
                else if (typeof(string) == prop.PropertyType)
                    resetCodeSB.Append($"\t\t\t{prop.Name} = string.Empty;");
                else if (typeof(ByteString) == prop.PropertyType)
                    resetCodeSB.Append($"\t\t\t{prop.Name} = Google.Protobuf.ByteString.Empty;");
                else throw new Exception($"Not Impl Reset Op {msgType}.{prop.Name} : {prop.PropertyType}");
            }
            var code = TEMPLATE
                .Replace("[NAMESPACE]", msgType.Namespace)
                .Replace("[CLASSNAME]", msgType.Name)
                .Replace("[RESETCODE]", resetCodeSB.ToString());


            sb.AppendLine(code);
        }

        File.WriteAllText("Assets/Script/AppMain/Network/ProtobufferMsgPool.g.cs", sb.ToString());

        AssetDatabase.Refresh();
    }
}
#endif