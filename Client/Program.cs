using Client.Network;
using System.Globalization;

namespace Client
{
    internal class Program
    {
        static bool running = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            //Set Culture
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            // Init Logger
            LiteNetLib.NetDebug.Logger = new Log();

            // Start ServerNetworkManager
            ServerNetworkManager.Instance.Connect("127.0.0.1", 10000);

            running = true;
            while (running)
            {
                ServerNetworkManager.Instance.Update();

                Thread.Sleep(1);
            }

            OnShutdown();
        }

        public static void Shutdown()
        {
            running = false;
        }

        private static void OnShutdown()
        {
            ServerNetworkManager.Instance.Close();
        }
    }
}
