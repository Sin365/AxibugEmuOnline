using MAME.Core;

namespace MAME.Core
{
    public static class AxiTimeSpan
    {
        public static ITimeSpan itime { get; private set; }
        public static void Init(ITimeSpan itimespan)
        {
            itime = itimespan;
        }
    }
}
