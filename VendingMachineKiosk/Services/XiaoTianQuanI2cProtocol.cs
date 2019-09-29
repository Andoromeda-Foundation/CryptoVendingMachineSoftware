using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachineKiosk.Services
{
    public static class XiaoTianQuanI2CProtocol
    {
        public enum Registers
        {
            I2CRegRc0 = 0x10,
            I2CRegRc1 = 0x11,
            I2CRegRc2 = 0x12,
            I2CRegRc3 = 0x13,
            I2CRegRc4 = 0x14,
            I2CRegRc5 = 0x15,
            I2CRegRc6 = 0x16,
            I2CRegRc7 = 0x17,
            I2CRegRc8 = 0x18,
            I2CRegRc9 = 0x19,
            I2CRegRcA = 0x1A,
            I2CRegRcB = 0x1B,
            I2CRegRcC = 0x1C,
            I2CRegRcD = 0x1D,
            I2CRegRcE = 0x1E,
            I2CRegRcF = 0x1F,

            I2CRegRs0 = 0x20,
            I2CRegRs1 = 0x21,
            I2CRegRs2 = 0x22,
            I2CRegRs3 = 0x23,
            I2CRegRs4 = 0x24,
            I2CRegRs5 = 0x25,
            I2CRegRs6 = 0x26,
            I2CRegRs7 = 0x27,
            I2CRegRs8 = 0x28,
            I2CRegRs9 = 0x29,
            I2CRegRsA = 0x2A,
            I2CRegRsB = 0x2B,
            I2CRegRsC = 0x2C,
            I2CRegRsD = 0x2D,
            I2CRegRsE = 0x2E,
            I2CRegRsF = 0x2F,

            I2CRegRss = 0x30,
            I2CRegRe = 0x31,

            I2CRegPwr = 0x80,
            I2CRegBat = 0x81,
            I2CRegBatL = I2CRegBat,
            I2CRegBatH = 0x82,
        }


}
}
