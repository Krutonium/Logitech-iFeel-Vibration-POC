# Logitech-iFeel-Vibration-POC
This POC activates the Vibration Motor in the Logitech iFeel Mouse (Circa 2001)

On Linux it should *just work*, but it will also disable your Mouse and Keyboard, so uh... Have SSH.

`sudo modprobe usbhid`

On MacOS, you need an empty kext for the mouse

On Windows, you just can't, apparently Mice and Keyboards are special snowflakes that you can't mess with.

Info was gleaned from http://www.moore.cx/out/ifeel/ and https://github.com/LibUsbDotNet/LibUsbDotNet/blob/master/src/Examples/Read.Write/ReadWrite.cs
