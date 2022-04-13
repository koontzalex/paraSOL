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
        public static bool SkipWeathersyncReroute = false;
        public static string url = "http://127.11.11.11:80/";
        public static int pageViews = 0;
        public static int requestCount = 0;
        public static string hostFileLocation = "C:\\Windows\\System32\\drivers\\etc\\hosts";
        public static string weathersyncOriginalDestination = "api.openweathermap.org";
        public static string weathersyncReroutedDestination = "127.11.11.11:80";
        public static string weathersyncReroutedDestinationHost = "127.11.11.11";
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
            if(SkipWeathersyncReroute)
            {
                Print("Skipping weathersync reroute, please check debug vars");
                return;
            }
            string text = System.IO.File.ReadAllText(hostFileLocation);
            Print("Here is the host file:");
            PrintSeparator();
            Print(text);
            PrintSeparator();

            string hostRewriteChunk = "\n" + weathersyncReroutedDestinationHost + " " + weathersyncOriginalDestination;
            Print("Here is the chunk to be added:");
            Print(hostRewriteChunk);
            PrintSeparator();


            File.WriteAllText(hostFileLocation, text + hostRewriteChunk);
        }


        public static void Main(string[] args)
        {

            RerouteWeathersync();
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