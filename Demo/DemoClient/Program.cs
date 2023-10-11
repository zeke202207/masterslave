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
            //await CreateMultilTest();
            await CreateOneTest();
            //await CreateMultilParallelTest();
            //await CreateOneParallelTest();
            Console.ReadLine();
            Console.WriteLine("Hello, World!");
        }

        private static async Task CreateMultilTest()
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
                        Record(() => client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString , Timeout = 60 }).GetAwaiter().GetResult(), input);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

        }

        private static async Task CreateOneTest()
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
                        Record(() => client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString, Timeout = 60 }).GetAwaiter().GetResult(), input);

                        //Console.ReadLine();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static async Task CreateMultilParallelTest()
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
                            Record(() => client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString, Timeout = 60 }).GetAwaiter().GetResult(), input);
                        }
                    });
            }
        }

        private static async Task CreateOneParallelTest()
        {
            while (true)
            {
                var factory = new MasterServiceSDKFactory("http://localhost:5600");
                Parallel.For(0, 10, i =>
                {
                    using (var client = factory.CreateClient())
                    {
                        byte[] byteArray = new byte[] { 0x01, 0x02, 0x03 };
                        var input = Guid.NewGuid().ToString();
                        ByteString byteString = ByteString.CopyFrom(Encoding.Default.GetBytes(input));
                        Record(() => client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString, Timeout = 60 }).GetAwaiter().GetResult(), input);
                    }
                });
            }
        }

        private static void Record(Func<byte[]> func ,string input)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                sw.Start();

                var result = func.Invoke();

                sw.Stop();

                var output = Encoding.Default.GetString(result);
                var isSame = input == output;
                if (!isSame)
                {
                    Console.WriteLine("结果错误");
                    Console.ReadLine();
                }

                Console.WriteLine($"结果 {sw.Elapsed.TotalMilliseconds} -> {output} 毫秒");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}