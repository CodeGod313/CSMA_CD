using System;
using System.Data;
using System.Text;
using System.Threading;

namespace CSMA_CD
{
    public class Station
    {
        private int _attempts;

        private const int CollisionWindowTime = 123;

        private const int SLOT_TIME = 100;

        public string Name { get; }

        public Station(string name)
        {
            Name = name;
            _attempts = 0;
        }


        private string SendPackage(string destination)
        {
            if (Bus.isBusy)
            {
                Bus.MakeCollision();
                return null;
            }
            String dataString = "Hello, " + destination + ", it's me, " + Name + "!!!";
            byte[] dataBytes = Encoding.ASCII.GetBytes(dataString);
            Package package = new Package(Name, destination, dataBytes);
            Bus.isBusy = true;
            Bus.Buffer = package.getBytes();
            return dataString;
        }


        private string GenerateDestinationAddress()
        {
            Random random = new Random();
            String destination;
            do
            {
                int stationNumber = random.Next(1, Bus.QuantityOfConnectedStations + 1);
                destination = stationNumber.ToString();
                while (destination.Length < 4)
                {
                    destination = "0" + destination;
                }
            } while (destination == Name);

            return destination;
        }
        
        private void WaitCollisionWindow()
        {
            Thread.Sleep(CollisionWindowTime);
            Bus.isBusy = false;
        }

        private void Collision()
        {
            Console.WriteLine("|----------------------|");
            Console.WriteLine("|Collision!!!!!!!!!!!!!|");
            Console.WriteLine("|----------------------|");
            _attempts++;
            Random random = new Random();
            int k = Math.Min(10, _attempts);
            int r = random.Next(0, (int)Math.Pow(2, k));
            Thread.Sleep(r*SLOT_TIME);
        }


        private void ReceivePackage()
        {
            while (true)
            {
                if (Bus.isBusy)
                {
                    continue;
                }
                byte[] buffer = Bus.Buffer;
                if (buffer != null)
                {
                    string source = "", destination = "", data;
                    data = Package.getStringFromPackage(buffer, ref source, ref destination);
                    if (data == null)
                    {
                        Bus.Buffer = null;
                    }
                    if (data != null && destination == Name)
                    {
                        Bus.Buffer = null;
                        Console.WriteLine("Package received by station " + Name);
                        Console.WriteLine("Info: " + data);
                        Console.WriteLine("-----------------------------------------------");
                    }
                }
            }
        }
        
        public void Run()
        {
            Thread thread = new Thread(ReceivePackage);
            thread.Start();
            while (true)
            {
                _attempts = 0;
                Random random = new Random();
                Thread.Sleep(random.Next(1, 1000));
                string sentStringData;
                do
                {
                    sentStringData = SendPackage(GenerateDestinationAddress());
                    WaitCollisionWindow();
                    if (sentStringData == null)
                    {
                        Collision();
                    }
                } while (sentStringData == null && _attempts < 20);
            }
        }
        
    }
}