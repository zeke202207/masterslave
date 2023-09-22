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
            using (var client = factory.CreateClient())
            {
                byte[] byteArray = new byte[] { 0x01, 0x02, 0x03 };
                ByteString byteString = ByteString.CopyFrom(byteArray);

                var result = await client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString });

                var r = await client.GetJobResultAsync(new MasterSDKService.GetJobResultRequest() { JobId = result.JobId });

                Console.WriteLine(Encoding.Default.GetString(r.Result.ToByteArray()));
                Console.ReadLine();
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
                    ByteString byteString = ByteString.CopyFrom(byteArray);

                    var id = await client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString });


                    byte[] byteArray1 = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
                    ByteString byteString1 = ByteString.CopyFrom(byteArray1);
                    var id1 = await client.ExecuteTaskAsync(new MasterSDKService.ExecuteTaskRequest() { Data = byteString1 });

                    Console.WriteLine(id);
                }
            });
        }
    }
}