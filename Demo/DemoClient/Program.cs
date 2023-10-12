using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
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
#if DEBUG
            MyTest test = new MyTest();
            await test.CreateMultilTest();
            //await test.CreateOneTest();
            //await test.CreateMultilParallelTest();
            //await test.CreateOneParallelTest();
#else
            var summary = BenchmarkRunner.Run<MyTest>();
            Console.ReadLine();
#endif
            Console.WriteLine("Hello, World!");
            Console.ReadLine();
        }

       
    }

    public class MyTest
    {
        private int _totalCount = 1000;
        private int _timeout = 60 * 30;

        [Benchmark]
        public async Task CreateMultilTest()
        {
            var factory = new MasterServiceSDKFactory("http://localhost:5600");
            int i = 0;
            while (i++ < _totalCount)
            {
                try
                {
                    using (var client = factory.CreateClient())
                    {
                        byte[] byteArray = new byte[] { 0x01, 0x02, 0x03 };
                        var input = Guid.NewGuid().ToString();
                        ByteString byteString = ByteString.CopyFrom(Encoding.Default.GetBytes(input));
                        Record(() => client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString, Timeout = _timeout }).GetAwaiter().GetResult(), input);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

        }

        [Benchmark]
        public async Task CreateOneTest()
        {
            try
            {
                var factory = new MasterServiceSDKFactory("http://localhost:5600"); using (var client = factory.CreateClient())
                {
                    int i = 0;
                    while (i++ < _totalCount)
                    {
                        byte[] byteArray = new byte[] { 0x01, 0x02, 0x03 };
                        var input = Guid.NewGuid().ToString();
                        ByteString byteString = ByteString.CopyFrom(Encoding.Default.GetBytes(input));
                        Record(() => client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString, Timeout = _timeout }).GetAwaiter().GetResult(), input);

                        //Console.ReadLine();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Benchmark]
        public async Task CreateMultilParallelTest()
        {
            int i = 0;
            while (i++ < _totalCount)
            {
                Parallel.For(0, 5, i =>
                {
                    var factory = new MasterServiceSDKFactory("http://localhost:5600");
                    using (var client = factory.CreateClient())
                    {
                        byte[] byteArray = new byte[] { 0x01, 0x02, 0x03 };
                        var input = Guid.NewGuid().ToString();
                        ByteString byteString = ByteString.CopyFrom(Encoding.Default.GetBytes(input));
                        Record(() => client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString, Timeout = _timeout }).GetAwaiter().GetResult(), input);
                    }
                });
            }
        }

        [Benchmark]
        public async Task CreateOneParallelTest()
        {
            int i = 0;
            while (i++ < _totalCount)
            {
                var factory = new MasterServiceSDKFactory("http://localhost:5600");
                Parallel.For(0, 5, i =>
                {
                    using (var client = factory.CreateClient())
                    {
                        byte[] byteArray = new byte[] { 0x01, 0x02, 0x03 };
                        var input = Guid.NewGuid().ToString();
                        ByteString byteString = ByteString.CopyFrom(Encoding.Default.GetBytes(input));
                        Record(() => client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString, Timeout = _timeout }).GetAwaiter().GetResult(), input);
                    }
                });
            }
        }

        private static void Record(Func<byte[]> func, string input)
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