using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Fiddler;

namespace Logger
{
	class Program
	{
		public static List<Fiddler.Session> allSessions = new List<Session>();
		public static Dictionary<string, List<double>> sdata = new Dictionary<string, List<double>>();

		static ConsoleColor defaultColor = Console.ForegroundColor;

		static void Main(string[] args)
		{
			Console.WriteLine(String.Format("Starting version: {0}", FiddlerApplication.GetVersionString()));
			Fiddler.CONFIG.IgnoreServerCertErrors = false;
			 
			FiddlerApplication.Prefs.SetBoolPref("Fiddler.network.streaming.abortifclientaborts", true);
			FiddlerCoreStartupFlags oFCSF = FiddlerCoreStartupFlags.Default;

			FiddlerApplication.Startup(0, oFCSF);

			FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
			FiddlerApplication.BeforeResponse += FiddlerApplication_BeforeResponse;

			bool bDone = false;
			do
			{
				Console.Write(">");
				ConsoleKeyInfo cki = Console.ReadKey();
				Console.WriteLine();

				switch (cki.KeyChar)
				{
					case 'q':
						bDone = true;
						break;
				}

			} while (bDone == false);

			FiddlerApplication.Shutdown();
			Thread.Sleep(500);

			Console.WriteLine("ENDING:");

			foreach (string key in sdata.Keys)
			{
				double total = 0.0f;
				total = sdata[key].Sum();
				Console.WriteLine("{0} :: avg={1}", key, total / sdata[key].Count);
			}

			Console.ReadKey();

		}

		static void FiddlerApplication_BeforeResponse(Session oSession)
		{
			string url = oSession.url;
			DateTime start = oSession.Timers.ClientBeginRequest;
			DateTime end = oSession.Timers.ClientDoneResponse;
			TimeSpan t = end - start;

			if(oSession.Timers.DNSTime > 0)
				Console.WriteLine("DNS TIME: {0}", oSession.Timers.DNSTime);

			if (!sdata.Keys.Contains(url))
				sdata[url] = new List<double>();

			Monitor.Enter(sdata[url]);
			sdata[url].Add(t.Milliseconds);
			Monitor.Exit(sdata[url]);

			ConsoleColor c = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine("{0}  ==> {1}", oSession.url, end-start);
			Console.ForegroundColor = defaultColor;
		}

		static void FiddlerApplication_BeforeRequest(Session oSession)
		{
			ConsoleColor c = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine("{0}", oSession.url);
			Console.ForegroundColor = defaultColor;
		}
	}
}
