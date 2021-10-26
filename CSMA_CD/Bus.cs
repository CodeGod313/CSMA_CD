using System;

namespace CSMA_CD
{
    public class Bus
    {
        public static bool isBusy = false;
        
        private static Byte[] _buffer = null;

        private static int _QuantityOfConnectedStations = 0;

        public static int QuantityOfConnectedStations => _QuantityOfConnectedStations;

        public static byte[] Buffer
        {
            get => _buffer;
            set => _buffer = value;
        }

        public static void ConnectStation()
        {
            _QuantityOfConnectedStations++;
        }

        public static void DisconnectStation()
        {
            _QuantityOfConnectedStations--;
        }

        public static void MakeCollision()
        {
            Bus._buffer[10] = 244;
        }
    }
}