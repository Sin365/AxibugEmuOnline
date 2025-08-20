using AxibugEmuOnline.Client.Tools;
using AxibugProtobuf;

namespace AxibugEmuOnline.Client
{
    public partial class SaveFile
    {
        public class ConflictState : SimpleFSM<SaveFile>.State
        {
            public Protobuf_Mine_GameSavInfo NetData { get; set; }
        }
    }
}