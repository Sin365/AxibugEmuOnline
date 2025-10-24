namespace cpu.nec
{
    public partial class Nec
    {

        /// <summary>
        /// 重写的Nec GetEAOpCode指令调度
        /// </summary>
        /// <param name="op"></param>
        public int DoNecGetEAOpCode(int op)
        {
            NecGetEAOpCode opType = NecGetEAOpCodeArr[op];
            switch (opType)
            {
                case NecGetEAOpCode.EA_000: return EA_000();
                case NecGetEAOpCode.EA_001: return EA_001();
                case NecGetEAOpCode.EA_002: return EA_002();
                case NecGetEAOpCode.EA_003: return EA_003();
                case NecGetEAOpCode.EA_004: return EA_004();
                case NecGetEAOpCode.EA_005: return EA_005();
                case NecGetEAOpCode.EA_006: return EA_006();
                case NecGetEAOpCode.EA_007: return EA_007();
                case NecGetEAOpCode.EA_100: return EA_100();
                case NecGetEAOpCode.EA_101: return EA_101();
                case NecGetEAOpCode.EA_102: return EA_102();
                case NecGetEAOpCode.EA_103: return EA_103();
                case NecGetEAOpCode.EA_104: return EA_104();
                case NecGetEAOpCode.EA_105: return EA_105();
                case NecGetEAOpCode.EA_106: return EA_106();
                case NecGetEAOpCode.EA_107: return EA_107();
                case NecGetEAOpCode.EA_200: return EA_200();
                case NecGetEAOpCode.EA_201: return EA_201();
                case NecGetEAOpCode.EA_202: return EA_202();
                case NecGetEAOpCode.EA_203: return EA_203();
                case NecGetEAOpCode.EA_204: return EA_204();
                case NecGetEAOpCode.EA_205: return EA_205();
                case NecGetEAOpCode.EA_206: return EA_206();
                case NecGetEAOpCode.EA_207: return EA_207();
                default:
                    throw new System.Exception("NecGetEAOpCode Err");
            }
        }
    }
}