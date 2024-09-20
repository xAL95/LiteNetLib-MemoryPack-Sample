using System.Globalization;
using Server.Network;

namespace Server
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

            // Start ClientNetworkManager
            ClientNetworkManager.Instance.Start(10000);

            running = true;
            while(running)
            {
                ClientNetworkManager.Instance.Update();

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
            ClientNetworkManager.Instance.Shutdown();
        }
    }
}
