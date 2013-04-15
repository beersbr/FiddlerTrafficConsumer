using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Fiddler;
using System.IO;

namespace Logger
{
	public struct RequestAggregate
	{
		public double time;
		public string host;
		public long data_size;
	}

	class Program
	{
		public static List<Fiddler.Session> allSessions = new List<Session>();
		public static Dictionary<string, List<RequestAggregate>> sdata = new Dictionary<string, List<RequestAggregate>>();

		static ConsoleColor defaultColor = Console.ForegroundColor;

		const string filter = "10.11.4.60"; //String.Empty;

		

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

			using (StreamWriter file = new StreamWriter("./output.csv"))
			{
				file.Write("URL, Time (ms), size (b)\r\n");
				foreach (string key in sdata.Keys)
				{
					for (int i = 0; i < sdata[key].Count; i++)
					{
						file.Write(string.Format("{0},{1},{2}\r\n", key, sdata[key][i].time, sdata[key][i].data_size));
					}
				}
			}

			Console.ReadKey();

		}

		static void FiddlerApplication_BeforeResponse(Session oSession)
		{
			string url = oSession.url;
			DateTime start = oSession.Timers.ClientBeginRequest;
			DateTime end = oSession.Timers.ClientDoneResponse;
			TimeSpan t = end - start;

			if (oSession.host != filter)
				return;

			if(oSession.Timers.DNSTime > 0)
				Console.WriteLine("DNS TIME: {0}", oSession.Timers.DNSTime);

			if (!sdata.Keys.Contains(url))
				sdata[url] = new List<RequestAggregate>();

			RequestAggregate rq = new RequestAggregate()
			{
				data_size = oSession.GetResponseBodyAsString().Length,
				host = oSession.host,
				time = t.TotalMilliseconds
			};

			Monitor.Enter(sdata[url]);
			sdata[url].Add(rq);
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
