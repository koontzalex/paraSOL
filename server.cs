// Filename:  HttpServer.cs        
// Author:    Benjamin N. Summerton <define-private-public>        
// License:   Unlicense (http://unlicense.org/)

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace HttpListenerExample
{
    class HttpServer
    {
        public static HttpListener listener;
        public static string url = "http://127.11.11.11:80/";
        public static int pageViews = 0;
        public static int requestCount = 0;
        public static string hostFileLocation = "C:\\Windows\\System32\\drivers\\etc\\hosts";
        public static string HostfileSource = "geoplugin.net";
        public static string HostfileTag = "# Kura5 Weathersync Reroute";
        public static string HostfileDestination = "127.11.11.11";
        public static string pageData = 
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "    <title>HttpListener Example</title>" +
            "  </head>" +
            "  <body>" +
            "    <p>Page Views: {0}</p>" +
            "    <form method=\"post\" action=\"shutdown\">" +
            "      <input type=\"submit\" value=\"Shutdown\" {1}>" +
            "    </form>" +
            "  </body>" +
            "</html>";

        public static string RerouteHostFile = "RerouteHostFile";
        public static string CleanupHostFile = "CleanupHostFile";
        public static string SpinupServer = "SpinupServer";

        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                ++requestCount;
                PrintSeparator();
                Print("Request #: " + requestCount);
                Print(req.Url.ToString());
                Print(req.HttpMethod);
                Print(req.UserHostName);
                Print(req.UserAgent);
                PrintSeparator();

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
                {
                    PrintSeparator();
                    Print("Shutdown requested");
                    runServer = false;
                }
                else if ((req.HttpMethod == "GET") && (req.RawUrl.Contains("/json.gp")))
                {
                    PrintSeparator();
                    Print("We made it");
                }
                // Make sure we don't increment the page views counter if `favicon.ico` is requested
                if (req.Url.AbsolutePath != "/favicon.ico")
                    pageViews += 1;

                // Write the response info
                string disableSubmit = !runServer ? "disabled" : "";
                byte[] data = Encoding.UTF8.GetBytes(String.Format(pageData, pageViews, disableSubmit));
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }

        public static void RerouteWeathersync()
        {
            // string text = System.IO.File.ReadAllText(hostFileLocation);
            string[] lines = File.ReadAllLines(hostFileLocation);
            Print("Here is the host file:");
            PrintSeparator();
            Print(string.Join("\n", lines));
            PrintSeparator();

            string hostRewriteChunk = HostfileDestination + " " + HostfileSource + " " + HostfileTag;
            string hostRewriteChunk2 = HostfileDestination + " www." + HostfileSource + " " + HostfileTag;
            Print("Here is the chunk to be added:");
            Print(hostRewriteChunk);
            PrintSeparator();

            string[] newLines = new string[lines.Length + 2];
            lines.CopyTo(newLines, 0);
            newLines[newLines.Length -1] = hostRewriteChunk;
            newLines[newLines.Length -2] = hostRewriteChunk2;
            File.WriteAllText(hostFileLocation, string.Join("\n", newLines));
            Print("Hostfile updated!");
        }

        public static void UnrouteWeathersync()
        {

            string text = System.IO.File.ReadAllText(hostFileLocation);
            bool hasReroute = text.Contains(HostfileTag);
            if(!hasReroute)
            {
                Print("No reroute detected, skipping...");
                return;
            }
            Print("Reroute detected in hostfile, removing...");

            string tempFile = Path.GetTempFileName();

            using(var sr = new StreamReader(hostFileLocation))
            using(var sw = new StreamWriter(tempFile))
            {
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    if(!line.Contains(HostfileTag)) {
                        sw.WriteLine(line);
                    }
                }
            }

            Print("Deleting old host file...");
            File.Delete(hostFileLocation);
            Print("Creating new host file replacement...");
            File.Move(tempFile, hostFileLocation);
            Print("Done!");

        }


        public static void Main(string[] args)
        {
            // Console.WriteLine(args[0]);
            Print("Hello World!");
            if( should(RerouteHostFile, args)) {
                Print("RerouteWeathersync!");
                RerouteWeathersync();
            }

            if( should(SpinupServer, args)) {
                Print("SpinupServer!");
                // Create a Http server and start listening for incoming connections
                listener = new HttpListener();
                listener.Prefixes.Add(url);
                listener.Start();
                Print("Listening for connections on " + url);

                // Handle requests
                Task listenTask = HandleIncomingConnections();
                listenTask.GetAwaiter().GetResult();

                // Close the listener
                listener.Close();
            }

            if( should(CleanupHostFile, args)) {
                Print("UnrouteWeathersync!");
                UnrouteWeathersync();
            }
        }

        public static bool should(string search, string[] args) {
            return (args.Length == 0) || (Array.IndexOf(args, search) != -1);
        }

        public static void Print(string message)
        {
            Console.WriteLine(message);
        }

        public static void PrintSeparator()
        {
            string separator = "################################";
            Print(separator);
        }
    }
}