using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace VendingMachineKiosk.Services
{
    public class VendingMachineHardwareService
    {
        private I2cDevice _i2CController;
        private GpioPin _readyPin;

        public async Task<bool> InitializeAsync()
        {
            string selector = I2cDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(selector);
            var settings = new I2cConnectionSettings(0x77);
            _i2CController = await I2cDevice.FromIdAsync(devices[0].Id, settings);

            var gpioController = await GpioController.GetDefaultAsync();

            var pinOpened = gpioController.TryOpenPin(Config.ReadyPin, GpioSharingMode.Exclusive, out var pin, out var status);

            if (!pinOpened || status != GpioOpenStatus.PinOpened)
            {
                return false;
            }

            _readyPin = pin;
            _readyPin.ValueChanged += ReadyPinChanged;

            return true;
        }

        private bool ReleaseOk { get; set; }

        private void ReadyPinChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                ReleaseOk = GetReleaseSuccess();
            }
        }

        private bool GetReleaseSuccess()
        {
            var command = new byte[1];
            var reg = new byte[16];

            command[0] = (byte)XiaoTianQuanI2CProtocol.Registers.I2CRegRs0;

            _i2CController.WriteRead(command, reg);

            return reg.All(r => r == 0);
        }


        public async Task<bool> ReleaseItemAsync(uint slotId)
        {
            ReleaseOk = false;
            var data = new byte[2];
            data[0] = (byte)(slotId / 0x8 + (uint)XiaoTianQuanI2CProtocol.Registers.I2CRegRc0);
            var bit = slotId % 0x8;
            data[1] = (byte)(1u << (int)bit);
            _i2CController.Write(data);

            // wait for 5s, check for ready
            await Task.Delay(Config.ReleaseTimeout);

            return ReleaseOk;
        }


    }
}
