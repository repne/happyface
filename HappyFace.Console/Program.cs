using System;
using System.Runtime.InteropServices;
using HappyFace.Domain;

namespace HappyFace.Console
{
    class Program
    {
        private static AsyncStore<Result> _store;
        private static AsyncStore<FetchTarget> _frontier;

        static void Main(string[] args)
        {
            using (_store = new AsyncStore<Result>("results"))
            using (_frontier = new AsyncStore<FetchTarget>("frontier"))
            {
                const string seed = "http://www.theguardian.com/world/2013/sep/04/putin-warns-military-action-syria";

                _frontier.Set(seed, new FetchTarget
                {
                    Level = 1,
                    Uri = new Uri(seed)
                });

                var crawler = Crawler.Create(_store, _frontier);

                SetConsoleCtrlHandler((x) => Handler(crawler, x), true);

                crawler.Start();
            }
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);

        // ReSharper disable InconsistentNaming
        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
        // ReSharper restore InconsistentNaming

        private static bool Handler(Crawler crawler, CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    System.Console.WriteLine("Closing...");
                    _store.Dispose();
                    _frontier.Dispose();
                    crawler.Stop();
                    return false;
                default:
                    return true;
            }
        }
    }
}
