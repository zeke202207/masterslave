using NetX.WorkerPlugin.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test1
{
    public interface IZekeTransient
    {
        bool Test();
    }

    [Transient]
    public class ZekeTransient:IZekeTransient
    {
        private Guid id;

        public ZekeTransient()
        {
            id = Guid.NewGuid();
        }

        public bool Test()
        {
            Console.WriteLine("ZekeTransient.Test");
            return true;
        }
    }

    public interface IZekeSingleton
    {
        bool Test();
    }

    [Singleton]
    public class ZekeSingleton : IZekeSingleton
    {
        private Guid id;

        public ZekeSingleton()
        {
            id = Guid.NewGuid();
        }

        public bool Test()
        {
            Console.WriteLine("ZekeSingleton.Test");
            return true;
        }
    }
}
