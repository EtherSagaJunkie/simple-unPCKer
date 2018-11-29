namespace BeySoft
{
    // Algorithm ID's for various MMO's
    //   released by Wanmei.
    // --------------------------------
    // Jade Dynasty/Zhuxian :  id =   0
    // Perfect World        :  id =   0
    // Hot Dance Party      :  id = 111
    // Ether Saga Odyssey   :  id = 121
    // Forsaken World       :  id = 131
    // Saint Seiya          :  id = 161
    // Swordsman Online     :  id = 161?

    public class AlgorithmId
    {
        public uint PckGuardByte0 { get; set; }
        public uint PckGuardByte1 { get; set; }
        public uint PckMaskDword { get; set; }
        public uint PckCheckMask { get; set; }

        public AlgorithmId(uint id = 0)
        {
            SetAlgorithmId(id);
        }

        private void SetAlgorithmId(uint id)
        {
            switch (id)
            {
                case 111:
                    PckGuardByte0 = 0xAB12908F;
                    PckGuardByte1 = 0xB3231902;
                    PckMaskDword  = 0x2A63810E;
                    PckCheckMask  = 0x18734563;
                    break;

                default:
                    PckGuardByte0 = 0xFDFDFEEE + id * 0x72341F2;
                    PckGuardByte1 = 0xF00DBEEF + id * 0x1237A73;
                    PckMaskDword  = 0xA8937462 + id * 0xAB2321F;
                    PckCheckMask  = 0x59374231 + id * 0x987A223;
                    break;
            }
        }
    }
}
