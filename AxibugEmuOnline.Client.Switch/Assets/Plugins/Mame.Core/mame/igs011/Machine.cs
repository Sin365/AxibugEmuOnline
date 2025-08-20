namespace MAME.Core
{
    public partial class IGS011
    {
        public static void lhb2_interrupt()
        {
            if (Cpuexec.iloops == 0)
            {
                Cpuint.cpunum_set_input_line(0, 6, LineState.HOLD_LINE);
            }
            else
            {
                Cpuint.cpunum_set_input_line(0, 5, LineState.HOLD_LINE);
            }
        }
        public static void wlcc_interrupt()
        {
            if (Cpuexec.iloops == 0)
            {
                Cpuint.cpunum_set_input_line(0, 3, LineState.HOLD_LINE);
            }
            else
            {
                Cpuint.cpunum_set_input_line(0, 6, LineState.HOLD_LINE);
            }
        }
        public static void lhb_interrupt()
        {
            if (lhb_irq_enable == 0)
            {
                return;
            }
            if (Cpuexec.iloops == 0)
            {
                Cpuint.cpunum_set_input_line(0, 6, LineState.HOLD_LINE);
            }
            else
            {
                Cpuint.cpunum_set_input_line(0, 5, LineState.HOLD_LINE);
            }
        }
        public static void vbowl_interrupt()
        {
            if (Cpuexec.iloops == 0)
            {
                Cpuint.cpunum_set_input_line(0, 6, LineState.HOLD_LINE);
            }
            else
            {
                Cpuint.cpunum_set_input_line(0, 3, LineState.HOLD_LINE);
            }
        }
        public static int BIT(int x, int n)
        {
            return (x >> n) & 1;
        }
    }
}
