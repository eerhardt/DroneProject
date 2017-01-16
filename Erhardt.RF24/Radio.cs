using System;
using System.Runtime.InteropServices;
using static Erhardt.RF24.Radio.NativeMethods;

namespace Erhardt.RF24
{
    public enum DataRate
    {
        OneMBPS,
        TwoMBPS,
        TwoHundredFiftyKBPS
    }

    public enum AddressWidth
    {
        ThreeBytes,
        FourBytes,
        FiveBytes
    }

    public enum PowerLevel
    {
        Minimum,
        Low,
        High,
        Maximum,
    }

    public class Radio : IDisposable
    {
        private SafeRF24Handle handle;
        public Radio(byte cePin, byte csnPin, int spiSpeed)
        {
            handle = RF24Create(cePin, csnPin, spiSpeed);
        }
        
        public byte Channel
        {
            get { return NativeMethods.GetChannel(handle); }
            set { NativeMethods.SetChannel(handle, value); }
        }

        public byte PayloadSize
        {
            get { return NativeMethods.GetPayloadSize(handle); }
            set { NativeMethods.SetPayloadSize(handle, value); }
        }

        public PowerLevel PowerAmplifierLevel
        {
            get { return (PowerLevel)NativeMethods.GetPowerAmplifierLevel(handle); }
            set { NativeMethods.SetPowerAmplifierLevel(handle, (int)value); }            
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                handle.Dispose();
            }
        }

        public void Begin()
        {
            if (!NativeMethods.Begin(handle))
            {
                throw new InvalidOperationException("Radio was not able to be initialized.");
            }
        }

        public void StartListening()
        {
            NativeMethods.StartListening(handle);
        }

        public void StopListening()
        {
            NativeMethods.StopListening(handle);
        }

        public bool CanRead()
        {
            return NativeMethods.Available(handle) != 0;
        }

        public void Read(byte[] buffer, byte length)
        {
            unsafe
            {
                fixed (byte* bufferStart = buffer)
                {
                    NativeMethods.Read(handle, bufferStart, length);
                }
            }
        }

        // public byte[] Read()
        // {

        // }

        public bool Write(byte[] buffer)
        {
            if (buffer.Length > byte.MaxValue)
            {
                throw new ArgumentException($"Can only write a maximum of {byte.MaxValue} at a time.", nameof(buffer));
            }

            byte bufferLength = (byte)buffer.Length;

            unsafe
            {
                fixed (byte* bufferStart = buffer)
                {
                    return NativeMethods.Write(handle, bufferStart, bufferLength) != 0;
                }
            }
        }

        public void OpenReadingPipe(byte number, byte[] address)
        {
            if (number > 5)
            {
                throw new ArgumentException("Radio only supports 0-5 reading pipes", nameof(number));                
            }

            if (address.Length < 3 || address.Length > 5)
            {
                throw new ArgumentException("Address can only be 3, 4, or 5 bytes long", nameof(address));
            }

            NativeMethods.OpenReadingPipe(handle, number, address);
        }

        public void OpenWritingPipe(byte[] address)
        {
            if (address.Length < 3 || address.Length > 5)
            {
                throw new ArgumentException("Address can only be 3, 4, or 5 bytes long", nameof(address));
            }

            NativeMethods.OpenWritingPipe(handle, address);
        }

        public void PrintDetails()
        {
            NativeMethods.PrintDetails(handle);
        }

        public bool TestCarrier()
        {
            return NativeMethods.TestCarrier(handle) != 0;
        }

        public void DisableCRC()
        {
            NativeMethods.DisableCRC(handle);
        }

        public void EnableDynamicPayloads()
        {
            NativeMethods.EnableDynamicPayloads(handle);            
        }

        public void EnableDynamicAcknowledge()
        {
            NativeMethods.EnableDynamicAcknowledge(handle);                        
        }

        public void EnableAcknowledgePayload()
        {
            NativeMethods.EnableAcknowledgePayload(handle);                                    
        }

        public void SetAutoAcknowledge(bool enable)
        {
            NativeMethods.SetAutoAcknowledge(handle, enable ? 1 : 0);
        }

        public bool SetDataRate(DataRate speed)
        {
            return NativeMethods.SetDataRate(handle, (int)speed) != 0;
        }

        public void SetAddressWidth(AddressWidth width)
        {
            NativeMethods.SetAddressWidth(handle, (int)width);
        }

        public void SetRetries(byte delay, byte count)
        {
            NativeMethods.SetRetries(handle, delay, count);
        }

        internal sealed class SafeRF24Handle : SafeHandle
        {
            public SafeRF24Handle()
                : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid
            {
                get { return handle == IntPtr.Zero; }
            }

            protected override bool ReleaseHandle()
            {
                RF24Destroy(handle);
                SetHandle(IntPtr.Zero);
                return true;
            }
        }        
        
        internal static class NativeMethods
        {
            private const string NativeLib = "Erhardt.RF24Lib.Native.so";
            private const string NativePrefix = "ErhardtRF24Lib_";

            [DllImport(NativeLib, EntryPoint = NativePrefix + "RF24Create2")]
            public static extern SafeRF24Handle RF24Create(byte cePin, byte csnPin);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "RF24Create3")]
            public static extern SafeRF24Handle RF24Create(byte cePin, byte csnPin, int spiSpeed);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "RF24Destroy")]
            public static extern void RF24Destroy(IntPtr handle);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "Begin")]
            public static extern bool Begin(SafeRF24Handle radio);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "StartListening")]
            public static extern void StartListening(SafeRF24Handle radio);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "StopListening")]
            public static extern void StopListening(SafeRF24Handle radio);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "OpenReadingPipe")]
            public static extern void OpenReadingPipe(SafeRF24Handle radio, byte number, byte[] address);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "OpenWritingPipe")]
            public static extern void OpenWritingPipe(SafeRF24Handle radio, byte[] address);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "Available")]
            public static extern int Available(SafeRF24Handle radio);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "Read")]
            public static unsafe extern void Read(SafeRF24Handle radio, byte* buffer, byte length);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "Write")]
            public static unsafe extern int Write(SafeRF24Handle radio, byte* buffer, byte length);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "GetChannel")]
            public static extern byte GetChannel(SafeRF24Handle radio);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "SetChannel")]
            public static extern void SetChannel(SafeRF24Handle radio, byte channel);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "GetPayloadSize")]
            public static extern byte GetPayloadSize(SafeRF24Handle radio);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "SetPayloadSize")]
            public static extern void SetPayloadSize(SafeRF24Handle radio, byte size);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "PrintDetails")]
            public static extern void PrintDetails(SafeRF24Handle radio);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "TestCarrier")]
            public static extern int TestCarrier(SafeRF24Handle radio);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "DisableCRC")]
            public static extern void DisableCRC(SafeRF24Handle radio);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "EnableAcknowledgePayload")]
            public static extern void EnableAcknowledgePayload(SafeRF24Handle radio);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "EnableDynamicPayloads")]
            public static extern void EnableDynamicPayloads(SafeRF24Handle radio);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "EnableDynamicAcknowledge")]
            public static extern void EnableDynamicAcknowledge(SafeRF24Handle radio);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "SetAutoAcknowledge")]
            public static extern void SetAutoAcknowledge(SafeRF24Handle radio, int enable);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "SetDataRate")]
            public static extern int SetDataRate(SafeRF24Handle radio, int speed);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "SetAddressWidth")]
            public static extern void SetAddressWidth(SafeRF24Handle radio, int width);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "SetRetries")]
            public static extern void SetRetries(SafeRF24Handle radio, byte delay, byte count);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "GetPowerAmplifierLevel")]
            public static extern int GetPowerAmplifierLevel(SafeRF24Handle radio);

            [DllImport(NativeLib, EntryPoint = NativePrefix + "SetPowerAmplifierLevel")]
            public static extern void SetPowerAmplifierLevel(SafeRF24Handle radio, int level);            
        }
    }
}
