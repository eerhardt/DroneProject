using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Erhardt.RF24;

namespace ConsoleApplication
{
    public class Program
    {
        private const byte minChannels = 15;
        private const byte maxChannels = 75;
        private const byte numChannels = maxChannels - minChannels;

        private const int num_samples = 100;

        private const int threshold = 5;

        private const long TicksPerMillisecond = TimeSpan.TicksPerMillisecond;
        private const long TicksPerMicrosecond = (TimeSpan.TicksPerMillisecond / 1000);
        private static readonly TimeSpan DelayTime = new TimeSpan(128 * TicksPerMicrosecond);

        public static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            try{
            Symax symax = new Symax();
            Console.WriteLine("pairing");
            symax.Pair();
            Console.WriteLine("flying");
            symax.Fly(2);

            // Console.WriteLine("Hello DoIt!");
            // Console.WriteLine(DoIt());
            System.Console.WriteLine(DelayTime);
            System.Console.WriteLine("TicksPerMicrosecond: " + TicksPerMicrosecond);

            Console.WriteLine("Hello Radio!");
            using (Radio myRF24 = new Radio(25, 0, 32))
            {
                myRF24.Begin();
                myRF24.Channel = 32;
                myRF24.SetAutoAcknowledge(true);
                myRF24.SetAddressWidth(AddressWidth.FiveBytes);
                myRF24.SetRetries(2, 10);
                myRF24.Channel = 60;
                myRF24.PayloadSize = 8;

                myRF24.OpenReadingPipe(0, new byte[] { 0x00, 0x00, 0x55 });
                myRF24.OpenReadingPipe(1, new byte[] { 0x00, 0x00, 0x55 });

                myRF24.PowerAmplifierLevel = PowerLevel.High;
                myRF24.SetDataRate(DataRate.OneMBPS);


                myRF24.OpenWritingPipe(new byte[] { 0xF0, 0xF0, 0xF0, 0xF0, 0xE1 });
                myRF24.PrintDetails();

                System.Console.WriteLine("Now Sending...");
                if (myRF24.Write(new byte[] { 0x55, 0xAA }))
                {
                    System.Console.WriteLine("Send successful");
                }
                else
                    System.Console.WriteLine("Send failed");

                //while (true)
                {
                    System.Console.WriteLine("...");

                    int[] values = new int[numChannels];

                    //for(int i = 0; i < num_samples; i++) 
                    {
                        // switch through the channels
                        for (byte j = 0; j < numChannels; j++)
                        {
                            myRF24.Channel = (byte)(minChannels + j);

                            myRF24.StartListening();

                            await Task.Delay(1);

                            myRF24.StopListening();

                            // store the result
                            if (myRF24.TestCarrier())
                            {
                                values[j]++;
                            }
                        } // for(int j = 0; j < numChannels; j++) {
                    } // for(int i = 0; i < num_samples; i++) {

                    // output of the results
                    for (int channel = 0; channel < numChannels; channel++)
                    {
                        // only print those above the threshold
                        //if (values[channel] > threshold) 
                        {
                            Console.WriteLine($"{minChannels + channel}:{values[channel]}");
                            // "%d:%d\n", minChannels + channel, values[channel]);
                        } // if(values[channel] > 5) {
                    }
                }
            }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }

        [DllImport("Erhardt.RF24Lib.so")]
        private static extern int DoIt();
    }
}
