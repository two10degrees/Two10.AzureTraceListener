using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Two10.AzureTraceListener.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true) 
            {
                Trace.WriteLine("Trace message at " + DateTime.UtcNow.ToString());
                Thread.Sleep(1000);
            }
        }
    }
}
