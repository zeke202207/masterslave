using Google.Protobuf;
using NetX.MasterSDK;
using System.Text;

namespace Demo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Test();
            Console.ReadLine();
            Console.WriteLine("Hello, World!");
        }

        private static async Task Test()
        {
            var factory = new MasterServiceSDKFactory("http://localhost:5600");
            while (true)
            {
                using (var client = factory.CreateClient())
                {
                    byte[] byteArray = new byte[] { 0x01, 0x02, 0x03 };
                    var input = Guid.NewGuid().ToString();
                    ByteString byteString = ByteString.CopyFrom(Encoding.Default.GetBytes(input));

                    var result = await client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString });
                    var output = Encoding.Default.GetString(result);
                    Console.WriteLine($"结果 {output}：{input == output}");
                }
                Thread.Sleep(500);
            }

        }

        private static async Task Test1()
        {
            Parallel.For(0, 10, async i =>
            {
                var factory = new MasterServiceSDKFactory("http://localhost:5600");
                using (var client = factory.CreateClient())
                {
                    byte[] byteArray = new byte[] { 0x01, 0x02, 0x03 };
                    var input = Guid.NewGuid().ToString();
                    ByteString byteString = ByteString.CopyFrom(Encoding.Default.GetBytes(input));
                    var result = await client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString });
                    var output = Encoding.Default.GetString(result);
                    Console.WriteLine($"结果 {output}：{input == output}");
                }
            });
        }
    }
}