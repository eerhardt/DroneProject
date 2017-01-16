// This code was ported to C# from Symax_nrf24l01.ino
// See https://github.com/pascallanger/DIY-Multiprotocol-TX-Module/blob/master/Multiprotocol/Symax_nrf24l01.ino
// Its header is below

/*
 This project is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 Multiprotocol is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with Multiprotocol.  If not, see <http://www.gnu.org/licenses/>.
 */
// compatible with Syma X5C-1, X11, X11C, X12 and for sub protocol X5C Syma X5C (original), X2
// Last sync with hexfet new_protocols/cx10_nrf24l01.c dated 2015-09-28

using System;
using System.Diagnostics;
using Erhardt.RF24;

namespace Erhardt.Multiprotocol
{
    public class Symax
    {
        private const int SYMAX_BIND_COUNT = 345;
        private const int SYMAX_INITIAL_WAIT = 500;
        private const int SYMAX_FIRST_PACKET_DELAY = 12000;
        private const int SYMAX_PACKET_PERIOD = 4000; // Timeout for callback in uSec

        private Radio radio;
        private byte[] rx_tx_addr;
        private int packet_count;
        private byte[] hopping_frequency;
        private int hopping_frequency_no;

        public Symax()
        {
            // TODO this should be random so multiple devices can communicate at the same time
            rx_tx_addr = new byte[] { 0x07, 0x07, 0x07, 0x07, 0x07 };
        }

        private static void delay(int microseconds)
        {
            long startTicks = Stopwatch.GetTimestamp();
            long endTicks = (long)(startTicks + (Stopwatch.Frequency * microseconds / 1000000));
            while (Stopwatch.GetTimestamp() < endTicks) ;
        }

        private static byte CheckSum(byte[] data)
        {
            unchecked
            {
                byte sum = 0;

                // the last byte of the data packet will be the checksum, so don't use it
                // in the checksum calculation
                for (int i = 0; i < data.Length - 1; i++)
                {
                    sum ^= data[i];
                }

                return (byte)(sum + 0x55);
            }
        }

        private byte[] BuildBindPacket()
        {
            System.Console.WriteLine("BuildBindPacket");
            
            byte[] packet = new byte[10];
            packet[0] = rx_tx_addr[4];
            packet[1] = rx_tx_addr[3];
            packet[2] = rx_tx_addr[2];
            packet[3] = rx_tx_addr[1];
            packet[4] = rx_tx_addr[0];
            packet[5] = 0xaa;
            packet[6] = 0xaa;
            packet[7] = 0xaa;
            packet[8] = 0x00;
            packet[9] = CheckSum(packet);

            return packet;
        }

        private static byte[] BuildDataPacket(
            byte throttle, byte elevator, byte rudder, byte aileron,
            bool video, bool picture, bool flip, bool headless)
        {
            System.Console.WriteLine("BuildDataPacket");
            byte[] packet = new byte[10];
            packet[0] = throttle;
            packet[1] = elevator;
            packet[2] = rudder;
            packet[3] = aileron;
            packet[4] = (byte)((video ? 0x80 : 0x00) | (picture ? 0x40 : 0x00));
            packet[5] = (byte)((elevator >> 2) | 0xc0); //always high rates (bit 7 is rate control)
            packet[6] = (byte)((rudder >> 2) | (flip ? 0x40 : 0x00));
            packet[7] = (byte)((aileron >> 2) | (headless ? 0x80 : 0x00));
            packet[8] = 0x00;
            packet[9] = CheckSum(packet);

            return packet;
        }

        private void SendPacket(byte[] packet)
        {
            System.Console.WriteLine("SendPacket");
            
            radio.Channel = hopping_frequency[hopping_frequency_no];
            if (!radio.Write(packet))
            {
                throw new InvalidOperationException("Unable to write packet");
            }

            if (packet_count++ % 2 == 0) // use each channel twice
                hopping_frequency_no = (hopping_frequency_no + 1) % hopping_frequency.Length; //rf_ch_num;
        }

        public void Fly(int seconds)
        {
            System.Console.WriteLine("Fly");
            byte throttle = 2;
            Stopwatch watch = new Stopwatch();
            watch.Start();

            while (watch.Elapsed.TotalSeconds < seconds)
            {
                var packet = BuildDataPacket(throttle, 128, 128, 128, false, false, false, false);
                SendPacket(packet);

                throttle = 100;// (byte)(((throttle + 1) % 10) + 50);
                delay(SYMAX_PACKET_PERIOD);
            }
        }

        public void Pair()
        {
            System.Console.WriteLine("Pair");
            // initSymax()
            radio = new Radio(25, 0, 32);
            radio.Begin();
            radio.SetAutoAcknowledge(false);
            // radio.OpenReadingPipe(0, ) // NRF24L01_WriteReg(NRF24L01_02_EN_RXADDR, 0x3F);  // Enable all data pipes (even though not used?)
            radio.SetAddressWidth(AddressWidth.FiveBytes);
            radio.SetRetries(15, 15);
            radio.Channel = 0x08;

            System.Console.WriteLine("SetDataRate");
            radio.SetDataRate(DataRate.TwoHundredFiftyKBPS);

            // not setting any read pipes -- is this a problem?

            radio.PayloadSize = 10;

            radio.OpenWritingPipe(new byte[] { 0xAB, 0xAC, 0xAD, 0xAE, 0xAF });

            delay(SYMAX_INITIAL_WAIT);

            // symax_init1()
            // duplicate stock tx sending strange packet (effect unknown)
            byte[] first_packet = { 0xf9, 0x96, 0x82, 0x1b, 0x20, 0x08, 0x08, 0xf2, 0x7d, 0xef, 0xff, 0x00, 0x00, 0x00, 0x00 };
            byte[] chans_bind = { 0x4b, 0x30, 0x40, 0x20 };

            radio.Write(first_packet);

            rx_tx_addr[4] = 0xa2; // this is constant in ID
            hopping_frequency = chans_bind;

            hopping_frequency_no = 0;
            packet_count = 0;

            delay(SYMAX_FIRST_PACKET_DELAY);

            //case SYMAX_BIND2:
            int counter = SYMAX_BIND_COUNT;

            while (counter > 0)
            {
                SendPacket(BuildBindPacket());
                counter--;
                delay(SYMAX_PACKET_PERIOD);
            }

            // symax_init2()

            symax_set_channels(rx_tx_addr[0]);
            radio.OpenWritingPipe(rx_tx_addr);

            hopping_frequency_no = 0;
            packet_count = 0;

            delay(SYMAX_PACKET_PERIOD);
        }

        private void symax_set_channels(byte address)
        {
            byte[] start_chans_1 = { 0x0a, 0x1a, 0x2a, 0x3a };
            byte[] start_chans_2 = { 0x2a, 0x0a, 0x42, 0x22 };
            byte[] start_chans_3 = { 0x1a, 0x3a, 0x12, 0x32 };

            byte laddress = (byte)(address & 0x1f);

            if (laddress < 0x10)
            {
                if (laddress == 6)
                {
                    laddress = 7;
                }

                for (int i = 0; i < start_chans_1.Length; i++)
                {
                    hopping_frequency[i] = (byte)(start_chans_1[i] + laddress);
                }
            }
            else if (laddress < 0x18)
            {
                for (int i = 0; i < start_chans_2.Length; i++)
                {
                    hopping_frequency[i] = (byte)(start_chans_2[i] + (laddress & 0x07));
                }

                if (laddress == 0x16)
                {
                    hopping_frequency[0]++;
                    hopping_frequency[1]++;
                }
            }
            else if (laddress < 0x1e)
            {
                for (int i = 0; i < start_chans_3.Length; i++)
                {
                    hopping_frequency[i] = (byte)(start_chans_3[i] + (laddress & 0x07));
                }
            }
            else if (laddress == 0x1e)
            {
                hopping_frequency = new byte[] { 0x38, 0x18, 0x41, 0x21 };
            }
            else
            {
                hopping_frequency = new byte[] { 0x39, 0x19, 0x41, 0x21 };
            }
        }
    }
}
