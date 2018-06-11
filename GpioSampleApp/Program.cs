using Mono.Unix.Native;
using System;
using System.IO;
using System.Threading;

namespace GpioSampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (GpioController c = new GpioController())
            {
                GpioPin p = c.GetPin(11, PinDirection.Out);

                for (int i = 0; i < 10; i++)
                {
                    p.Write(true);
                    Thread.Sleep(100);
                    p.Write(false);
                    Thread.Sleep(100);
                }
            }
        }
    }

    public class GpioController : IDisposable
    {
        private const int BCM2708_PERI_BASE = 0x20000000;
        private const int GPIO_BASE = (BCM2708_PERI_BASE + 0x200000);
        private const int FSEL_OFFSET = 0;  // 0x0000
        private const int SET_OFFSET = 7;   // 0x001c / 4
        private const int CLR_OFFSET = 10;  // 0x0028 / 4
        private const int PULLUPDN_OFFSET = 37; // 0x0094 / 4
        private const int PULLUPDNCLK_OFFSET = 38;  // 0x0098 / 4

        private const int PAGE_SIZE = (4 * 1024);
        private const int BLOCK_SIZE = (4 * 1024);

        private const int PUD_CONST_OFFSET = 20;

        private const int PUD_OFF = 0;
        private const int PUD_DOWN = 1;
        private const int PUD_UP = 2;
        enum GpioMode
        {
            Board,
            Bcm
        }

        private GpioMode _gpioMode = GpioMode.Board;
        private IntPtr _gpioMMap;
        private int[] _pinToGpio;

        public GpioController()
        {
            int memFileDescriptor;
            if ((memFileDescriptor = Syscall.open("/dev/gpiomem", OpenFlags.O_RDWR | OpenFlags.O_SYNC)) < 0)
            {
                throw new IOException($"Could not open '/dev/gpiomem'. Received '{memFileDescriptor}' error.");
            }

            _gpioMMap = Syscall.mmap(IntPtr.Zero, BLOCK_SIZE, MmapProts.PROT_READ | MmapProts.PROT_WRITE, MmapFlags.MAP_SHARED, memFileDescriptor, 0);

            if ((uint)_gpioMMap < 0)
            {
                throw new IOException($"Could not create a memory map. Received '{_gpioMMap}' error.");
            }

            // TODO: revision 1
            _pinToGpio = new[] { -1, -1, -1, 2, -1, 3, -1, 4, 14, -1, 15, 17, 18, 27, -1, 22, 23, -1, 24, 10, -1, 9, 25, 11, 8, -1, 7 };
        }

        public GpioPin GetPin(int channel, PinDirection direction)
        {
            GpioPin pin = new GpioPin(this, GetGpio(channel), direction);
            int offset = FSEL_OFFSET + (pin.Gpio / 10);
            int shift = (pin.Gpio % 10) * 3;
            int pud = PUD_OFF + PUD_CONST_OFFSET;
            if (direction == PinDirection.Out)
                pud = PUD_OFF + PUD_CONST_OFFSET;

            pud -= PUD_CONST_OFFSET;

            unsafe
            {
                int* gpioPointer = (int*)_gpioMMap.ToPointer();
                set_pullupdn(pin.Gpio, pud, gpioPointer);
                if (direction == PinDirection.Out)
                    *(gpioPointer + offset) = (*(gpioPointer + offset) & ~(7 << shift)) | (1 << shift);
                else  // direction == INPUT
                    *(gpioPointer + offset) = (*(gpioPointer + offset) & ~(7 << shift));
            }
            return pin;
        }

        private unsafe void set_pullupdn(int gpio, int pud, int* gpioPointer)
        {
            int clk_offset = PULLUPDNCLK_OFFSET + (gpio / 32);
            int shift = (gpio % 32);

            if (pud == PUD_DOWN)
                *(gpioPointer + PULLUPDN_OFFSET) = (*(gpioPointer + PULLUPDN_OFFSET) & ~3) | PUD_DOWN;
            else if (pud == PUD_UP)
                *(gpioPointer + PULLUPDN_OFFSET) = (*(gpioPointer + PULLUPDN_OFFSET) & ~3) | PUD_UP;
            else  // pud == PUD_OFF
                *(gpioPointer + PULLUPDN_OFFSET) &= ~3;

            short_wait();
            *(gpioPointer + clk_offset) = 1 << shift;
            short_wait();
            *(gpioPointer + PULLUPDN_OFFSET) &= ~3;
            *(gpioPointer + clk_offset) = 0;
        }

        private void short_wait()
        {
            int count = 0;
            for (int i = 0; i < 150; i++)     // wait 150 cycles
            {
                count++;
            }
        }

        private int GetGpio(int channel)
        {
            // TODO: range check for channel

            if (_gpioMode == GpioMode.Board)
            {
                return _pinToGpio[channel];
            }
            else if (_gpioMode == GpioMode.Bcm)
            {
                return channel;
            }

            throw new NotImplementedException();
        }

        internal unsafe void Write(int gpio, bool value)
        {
            int offset = value ? 
                SET_OFFSET + (gpio / 32) :
                CLR_OFFSET + (gpio / 32);

            int shift = (gpio % 32);

            uint* gpioPointer = (uint*)_gpioMMap.ToPointer();
            *(gpioPointer + offset) = (uint)1 << shift;
        }

        public void Dispose()
        {
            if (_gpioMMap != IntPtr.Zero)
            {
                Syscall.munmap(_gpioMMap, BLOCK_SIZE);
            }
        }
    }

    public enum PinDirection
    {
        In,
        Out
    }

    public class GpioPin
    {
        private readonly GpioController _controller;
        private readonly int _gpio;
        private readonly PinDirection _direction;

        internal GpioPin(GpioController controller, int gpio, PinDirection direction)
        {
            _controller = controller;
            _gpio = gpio;
            _direction = direction;
        }

        public int Gpio => _gpio;

        public void Write(bool value)
        {
            _controller.Write(_gpio, value);
        }
    }
}
