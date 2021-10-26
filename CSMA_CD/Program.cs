using System;
using System.Threading;

namespace CSMA_CD
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Bus.ConnectStation();
            Bus.ConnectStation();
            Bus.ConnectStation();
            Station station1 = new Station("0001");
            Station station2 = new Station("0002");
            Thread thread1 = new Thread(station1.Run);
            thread1.Start();
            Thread thread2 = new Thread(station2.Run);
            thread2.Start();
            Station station3 = new Station("0003");
            Thread thread3 = new Thread(station3.Run);
            thread3.Start();
        }
    }
}