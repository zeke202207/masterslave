using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    [MemoryDiagnoser]
    public class MergeArraysBenchmark
    {
        [Params(100000, 1000000, 10000000)]
        public int ArraySize { get; set; }

        private byte[][] arrays;
        private Random random;

        [GlobalSetup]
        public void GlobalSetup()
        {
            random = new Random();
            // 在这里初始化数组，可以根据 ArraySize 参数来设置数组的大小和内容
            arrays = new byte[3][];
            for (int i = 0; i < 3; i++)
            {
                arrays[i] = new byte[ArraySize];
                // 在这里可以为数组赋值
                FillArrayWithRandomValues(arrays[i]);
            }
        }

        private void FillArrayWithRandomValues(byte[] array)
        {
            random.NextBytes(array);
        }

        [Benchmark]
        public byte[] MergeArrays()
        {
            // 在这里调用 MergeArrays 方法，并传递 arrays 数组作为参数
            return MergeArrays(arrays);
        }

        [Benchmark]
        public byte[] MergeArraysCopy()
        {
            // 在这里调用 MergeArrays 方法，并传递 arrays 数组作为参数
            return MergeArraysCopy(arrays);
        }

        public byte[] MergeArrays(params byte[][] arrays)
        {
            int totalLength = arrays.Sum(a => a.Length);
            byte[] mergedArray = new byte[totalLength];
            int offset = 0;

            foreach (byte[] array in arrays)
            {
                array.AsSpan().CopyTo(mergedArray.AsSpan(offset));
                offset += array.Length;
            }

            return mergedArray;
        }
               
        public byte[] MergeArraysCopy(params byte[][] arrays)
        {
            int totalLength = arrays.Sum(a => a.Length);
            byte[] mergedArray = new byte[totalLength];
            int offset = 0;

            foreach (byte[] array in arrays)
            {
                array.CopyTo(mergedArray, offset);
                offset += array.Length;
            }

            return mergedArray;
        }
    }
}
