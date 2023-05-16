using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;

class Program
{
  public static void Main(string[] args)
  {
    Console.WriteLine("Pinginator is running!");
    string addr = "google.com";	// Address to ping
    string filename = ".\\ping_log.csv";	// File name to write data to
    string filePath = Path.Combine("", filename);	// File path
    string formatTemplate = "{0}, {1}, {2}, {3}";	// Format of the data (CSV)
    string header = "";	// File header (line 1 in the file)
    int lineCnt = File.Exists(filePath) ? File.ReadLines(filename).Count() : 0; // Current number of lines

		// Get a user set address
		string? userAddr;
		Console.Write("Enter an address or return for default ({0}) ", addr);
		userAddr = Console.ReadLine();
		if (!String.IsNullOrEmpty(userAddr)) {
			addr = userAddr;
		}
		Console.WriteLine("Testing by pinging {0}...", addr);

    // Determine if the file needs the header
    if (!File.Exists(filePath) || lineCnt == 0)
    {
      header = String.Format(
        formatTemplate,
        "\"Test Number\"",
        "Address",
        "\"Round Trip Time (ms)\"",
        "Status"
      );
    }

    // Open a file for writing. Append to the file or create if it doesn't exist.
    using (var writer = new StreamWriter(filePath, true))
    {
      // Catch SIGINT
      Console.CancelKeyPress += (sender, e) =>
      {
        Console.WriteLine("Exiting...");
				writer.Close();
        Environment.Exit(0);
      };


      if (!String.IsNullOrEmpty(header))
      {
        writer.WriteLine(header);
      }

      int maxLoops = 10000;																// Max number of data pts to collect
      int currPing = lineCnt > 0 ? lineCnt : 1; // The current data pt.
      const int sleepMS = 15000;
      while (true && maxLoops > 0)
      {
        try
        {
          Ping pinger = new Ping();
          int timeoutMS = 1000;
          PingReply reply = pinger.Send(addr, timeoutMS);
          if (reply != null)
          {
            string pingResult = String.Format(
              formatTemplate,
              currPing,
              reply.Address,
              reply.RoundtripTime,
              reply.Status
            );
            currPing++;
            writer.WriteLine(pingResult);
            Console.WriteLine(pingResult);
          }
        }
        catch
        {
          Console.WriteLine("ERROR: something went very wrong!");
        }
        maxLoops--;
        Thread.Sleep(sleepMS);
      }
    }
  }
}
