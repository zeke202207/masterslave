namespace NetX.Common;

public static class ByteSegmentHelper
{
    /// <summary>
    /// 异步分片处理byte数组
    /// </summary>
    /// <param name="result"></param>
    /// <param name="uploadHandler"></param>
    /// <param name="segmentSize">1000000 bit = 1M</param>
    /// <returns></returns>
    public static async Task SegmentHandlerAsync(this byte[] result, Func<Memory<byte>, Task> uploadHandler, int segmentSize = 1000000)
    {
        int numSegments = (result.Length + segmentSize - 1) / segmentSize;
        Memory<byte>[] segments = new Memory<byte>[numSegments];

        for (int i = 0; i < numSegments; i++)
        {
            int offset = i * segmentSize;
            int count = Math.Min(segmentSize, result.Length - offset);
            Memory<byte> segment = result.AsMemory(offset, count);
            segments[i] = segment;
        }
        foreach (var segment in segments)
        {
            await uploadHandler(segment);
        }
    }
}
