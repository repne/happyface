using System;
using System.Runtime.InteropServices;
using HappyFace.Domain;
using HappyFace.Units;

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
                var seeds = new[]
                {
                    "http://www.microsoft.com",
                    "http://www.theguardian.com/",
                    "http://www.reddit.com",
                };

                foreach (var seed in seeds)
                {
                    _frontier.Set(seed, new FetchTarget
                    {
                        Level = 1,
                        Uri = new Uri(seed)
                    });
                }

                var listener = new Listener<FetchResult>(x => System.Console.WriteLine("[FETCHED]: {0}", x.ResponseUri));

                var crawler = Crawler.Get(new HtmlAgilityPack.DocumentFactory(), _store, _frontier);

                crawler.Fetcher.SendTo(listener);

                SetConsoleCtrlHandler(x => Handler(crawler, x), true);

                crawler.Start();
            }
        }

        [DllImport("Kernel32", SetLastError = true)]
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
