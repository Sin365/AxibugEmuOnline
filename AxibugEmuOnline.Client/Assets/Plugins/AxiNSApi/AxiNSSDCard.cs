#if UNITY_SWITCH
using nn.account;
#endif
public class AxiNSSDCard
{
#if UNITY_SWITCH

#if DEVELOPMENT_BUILD || NN_FS_SD_CARD_FOR_DEBUG_ENABLE
        [DllImport(Nn.DllName,
            CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "nn_fs_MountSdCard")]
        public static extern nn.Result Mount(string name);
#else

	public static nn.Result Mount(string name)
	{
		return new nn.Result();
	}
#endif


#endif
}