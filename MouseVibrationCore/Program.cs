using System;
using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Info;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using LibUsbDotNet.Main;

namespace MouseVibrationCore
{
    class Program
    {
        public static UsbDevice MyUsbDevice;
        private const int VID = 0x046d;
        private const int PID = 0xc030;
        
        static void Main(string[] args)
        {
            var context = new UsbContext();
            var usbDeviceCollection = context.List();
            foreach (var device in usbDeviceCollection)
            {
                if (device.ProductId == PID)
                {
                    if (device.VendorId == VID)
                    {
                        MyUsbDevice = (UsbDevice)device;
                        Console.WriteLine("Found it");
                        break;
                    }
                }
            }
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName= "modprobe";
            info.Arguments = "-r usbhid";
            Process p = Process.Start(info);
            p.WaitForExit();
            MyUsbDevice.Open();
            MyUsbDevice.ClaimInterface(MyUsbDevice.Configs[0].Interfaces[0].Number);
            var writeEndpoint = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
            var readEnpoint = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
            int Intensity = 255;
            int Delay = 5;
            int Count = 255;
            Byte[] command = {0x11, 0x0a, (byte) Intensity, (byte) Delay, 0x00, (byte) Count};
            writeEndpoint.Write(command, 3000, out var bytesWritten);
        }
    }
}
    