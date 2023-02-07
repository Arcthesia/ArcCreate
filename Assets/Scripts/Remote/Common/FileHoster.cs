using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace ArcCreate.Remote.Common
{
    // Modified version of https://gist.github.com/augustoproiete/0a22dd020045a0fee239
    public class FileHoster : IDisposable
    {
        private static readonly IDictionary<string, string> MimeTypeMappings =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                { ".asf", "video/x-ms-asf" },
                { ".asx", "video/x-ms-asf" },
                { ".avi", "video/x-msvideo" },
                { ".bin", "application/octet-stream" },
                { ".cco", "application/x-cocoa" },
                { ".crt", "application/x-x509-ca-cert" },
                { ".css", "text/css" },
                { ".deb", "application/octet-stream" },
                { ".der", "application/x-x509-ca-cert" },
                { ".dll", "application/octet-stream" },
                { ".dmg", "application/octet-stream" },
                { ".ear", "application/java-archive" },
                { ".eot", "application/octet-stream" },
                { ".exe", "application/octet-stream" },
                { ".flv", "video/x-flv" },
                { ".gif", "image/gif" },
                { ".hqx", "application/mac-binhex40" },
                { ".htc", "text/x-component" },
                { ".htm", "text/html" },
                { ".html", "text/html" },
                { ".ico", "image/x-icon" },
                { ".img", "application/octet-stream" },
                { ".iso", "application/octet-stream" },
                { ".jar", "application/java-archive" },
                { ".jardiff", "application/x-java-archive-diff" },
                { ".jng", "image/x-jng" },
                { ".jnlp", "application/x-java-jnlp-file" },
                { ".jpeg", "image/jpeg" },
                { ".jpg", "image/jpeg" },
                { ".js", "application/x-javascript" },
                { ".mml", "text/mathml" },
                { ".mng", "video/x-mng" },
                { ".mov", "video/quicktime" },
                { ".mp3", "audio/mpeg" },
                { ".mpeg", "video/mpeg" },
                { ".mpg", "video/mpeg" },
                { ".msi", "application/octet-stream" },
                { ".msm", "application/octet-stream" },
                { ".msp", "application/octet-stream" },
                { ".pdb", "application/x-pilot" },
                { ".pdf", "application/pdf" },
                { ".pem", "application/x-x509-ca-cert" },
                { ".pl", "application/x-perl" },
                { ".pm", "application/x-perl" },
                { ".png", "image/png" },
                { ".prc", "application/x-pilot" },
                { ".ra", "audio/x-realaudio" },
                { ".rar", "application/x-rar-compressed" },
                { ".rpm", "application/x-redhat-package-manager" },
                { ".rss", "text/xml" },
                { ".run", "application/x-makeself" },
                { ".sea", "application/x-sea" },
                { ".shtml", "text/html" },
                { ".sit", "application/x-stuffit" },
                { ".swf", "application/x-shockwave-flash" },
                { ".tcl", "application/x-tcl" },
                { ".tk", "application/x-tcl" },
                { ".txt", "text/plain" },
                { ".war", "application/java-archive" },
                { ".wbmp", "image/vnd.wap.wbmp" },
                { ".wmv", "video/x-ms-wmv" },
                { ".xml", "text/xml" },
                { ".xpi", "application/x-xpinstall" },
                { ".zip", "application/zip" },
            };

        private readonly int provideOnPort;
        private HttpListener listener;
        private readonly Thread serverThread;
        private readonly IFileProvider fileProvider;
        private readonly int bufferSize;

        public FileHoster(int provideOnPort, IFileProvider fileProvider, int bufferSize = 1024)
        {
            this.provideOnPort = provideOnPort;
            this.fileProvider = fileProvider;
            this.bufferSize = bufferSize;
            serverThread = new Thread(Listen);
            serverThread.Start();
            IsRunning = true;
        }

        public bool IsRunning { get; private set; }

        public void Dispose()
        {
            serverThread.Abort();
            listener.Stop();
            IsRunning = false;
        }

        private void Listen()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + provideOnPort.ToString() + "/");
            listener.Start();

            while (true)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();
                    Process(context);
                }
                catch (Exception)
                {
                }
            }
        }

        private void Process(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            UnityEngine.Debug.Log("Received HTTP request for: " + filename);
            filename = filename.Substring(1);

            try
            {
                using (Stream input = fileProvider.RespondToFileRequest(filename, out string extension))
                {
                    byte[] buffer = new byte[bufferSize];

                    // Adding permanent http response headers
                    context.Response.ContentType = MimeTypeMappings.TryGetValue(extension, out string mime)
                        ? mime
                        : "application/octet-stream";
                    context.Response.ContentLength64 = input.Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
                    context.Response.StatusCode = (int)HttpStatusCode.OK;

                    int nbytes;
                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        context.Response.OutputStream.Write(buffer, 0, nbytes);
                    }
                }

                context.Response.OutputStream.Flush();
            }
            catch (FileNotFoundException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            context.Response.OutputStream.Close();
        }
    }
}