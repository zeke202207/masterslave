using Google.Protobuf;
using Grpc.Net.Client.Balancer;
using NetX.MasterSDK;
using System.Diagnostics;
using System.Text;

namespace Demo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Test0();
            Console.ReadLine();
            Console.WriteLine("Hello, World!");
        }

        private static async Task Test()
        {
            var factory = new MasterServiceSDKFactory("http://localhost:5600");
            while (true)
            {
                try
                {
                    using (var client = factory.CreateClient())
                    {
                        byte[] byteArray = new byte[] { 0x01, 0x02, 0x03 };
                        var input = Guid.NewGuid().ToString();
                        ByteString byteString = ByteString.CopyFrom(Encoding.Default.GetBytes(input));
                        Record(() => client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString }).GetAwaiter().GetResult(), input);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

        }

        private static async Task Test0()
        {
            try
            {
                var factory = new MasterServiceSDKFactory("http://localhost:5600"); using (var client = factory.CreateClient())
                {
                    while (true)
                    {
                        byte[] byteArray = new byte[] { 0x01, 0x02, 0x03 };
                        var input = Guid.NewGuid().ToString();
                        ByteString byteString = ByteString.CopyFrom(Encoding.Default.GetBytes(input));
                        Record(() => client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString }).GetAwaiter().GetResult(), input);

                        //Console.ReadLine();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static async Task Test1()
        {
            while (true)
            {
                Parallel.For(0, 10, i =>
                    {
                        var factory = new MasterServiceSDKFactory("http://localhost:5600");
                        using (var client = factory.CreateClient())
                        {
                            byte[] byteArray = new byte[] { 0x01, 0x02, 0x03 };
                            var input = Guid.NewGuid().ToString();
                            ByteString byteString = ByteString.CopyFrom(Encoding.Default.GetBytes(input));
                            Record(() => client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString }).GetAwaiter().GetResult(), input);
                        }
                    });
            }
        }

        private static void Record(Func<byte[]> func ,string input)
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            var result = func.Invoke();

            sw.Stop();

            var output = Encoding.Default.GetString(result);
            var isSame = input == output;
            if(!isSame)
            {
                Console.WriteLine("结果错误");
                Console.ReadLine();
            }

            Console.WriteLine($"结果 {sw.Elapsed.TotalMilliseconds} -> {output}");
        }
    }
}