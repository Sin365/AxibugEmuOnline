namespace Essgee.Emulation.Audio
{
    public class SegaSMSPSG : SN76489
    {
        /* LFSR is 16 bits, tapped bits are 0 and 3 (mask 0x0009), going into bit 15 */
        protected override ushort noiseLfsrMask => 0xFFFF;
        protected override ushort noiseTappedBits => 0x0009;
        protected override int noiseBitShift => 15;

        public SegaSMSPSG() : base() { }

        public override void Reset()
        {
            base.Reset();

            noiseLfsr = 0x8000;
        }

        #region AxiState

        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
            base.LoadAxiStatus(data);
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = base.SaveAxiStatus();
            return data;
        }
        #endregion

    }
}
