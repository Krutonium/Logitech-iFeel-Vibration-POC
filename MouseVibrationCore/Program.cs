using System;
using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Info;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
            //writeEndpoint.Write(BuildCommand(255, 10, 255), 3000, out var bytesWritten);
            //We don't care about bytesWritten, but we have to provide somthing soo...
            int x = 0;
            while (!Console.KeyAvailable)
            {
                writeEndpoint.Write(CancelVibration(), 3000, out var bytesWritten0);
                writeEndpoint.Write(BuildCommand(255, x, 255), 3000, out var bytesWritten1);
                x += 1;
                if (x > 100)
                {
                    x = 0; 
                }
                Console.WriteLine(x);
                System.Threading.Thread.Sleep(500);
            }
            writeEndpoint.Write(CancelVibration(),3000, out var bytesWritten);
            MyUsbDevice.Close();
            info.Arguments = "usbhid";
            p = Process.Start(info);
            p.WaitForExit();
        }
 
        static byte[] BuildCommand(int Intensity, int Delay, int Count)
        {
            Byte[] command = {0x11, 0x0a, (byte) Intensity, (byte) Delay, 0x0, (byte) Count, 0x0};
            //0x11 0x0a - These values seem to tell the mouse that the following is intended to run the motor
            //Intensity is how hard to run the motor, Delay is how Often to run the motor, and Count is how many times.
            //0x0 values are unknown - They may do somthing, but nothing has been discovered as to what.
            return command;
        }

        static byte[] CancelVibration()
        { 
            Byte[] command = {0x13, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0};
            return command;
        }
    }
}
    