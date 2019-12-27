using System;
using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Info;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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

            for (int intensity = 255; intensity <= 255; intensity-=5)
            {
                for (int delay = 0; delay < 100; delay+=5)
                {
                    int count = 250;
                    //writeEndpoint.Write(CancelVibration(), 5, out var bytesWritten0);
                        writeEndpoint.Write(BuildCommand(intensity, delay, count), 3000, out var bytesWritten1);
                        //System.Threading.Thread.Sleep(5);

                        var readBuffer = new byte[4];
                        readEnpoint.Read(readBuffer, 20, out var readBytes);
                        Console.Clear();
                        Console.WriteLine("readBuffer:");
                        foreach (var v in readBuffer)
                        {
                             Console.Write(Pad(v));
                             Console.Write(",");
                        }
                        Console.WriteLine();

                        UInt16 RelX = UInt16.Parse(readBuffer[1].ToString());
                        UInt16 RelY = UInt16.Parse(readBuffer[2].ToString());

                        //Higher than 128 is Positive
                        //Below 128 is Negative
                        
                        
                        
                        Console.WriteLine("X: " + RelX);
                        Console.WriteLine("Y: " + RelY);
                        Console.WriteLine();
                        Console.Write("Left Click: ");
                        Console.WriteLine(IsBitSet(readBuffer[0], 7) ? "True" : "False");
                        Console.Write("Right Click: ");
                        Console.WriteLine(IsBitSet(readBuffer[0], 6) ? "True" : "False");
                        Console.WriteLine("Intensity: " + intensity);
                        Console.WriteLine("Delay: " + delay);
                        Console.WriteLine("Count: " + count);
                        Console.WriteLine("Bytes read from Mouse: " + readBytes);
                        //System.Threading.Thread.Sleep(500);
                }
            }
            writeEndpoint.Write(CancelVibration(),3000, out var bytesWritten);
            MyUsbDevice.ResetDevice();
            MyUsbDevice.Close();
            info.Arguments = "usbhid";
            p = Process.Start(info);
            p.WaitForExit();
        }
 
        static string Pad(byte b)
        {
            return Convert.ToString(b, 2).PadLeft(8, '0');
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
        static bool IsBitSet(byte b, int pos)
        {
            string p = Pad(b);
            //Console.WriteLine(p);
            List<char> lis = p.ToCharArray().ToList();
            if (lis[pos] == '0')
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
    