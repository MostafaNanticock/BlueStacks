using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace BlueStacks.hyperDroid.Common.HTTP
{
    public class Server
    {
        public delegate void RequestHandler(HttpListenerRequest req, HttpListenerResponse res);

        public class ENoPortAvailable : Exception
        {
            public ENoPortAvailable(string reason)
                : base(reason)
            {
            }
        }

        private class Worker
        {
            private Dictionary<string, RequestHandler> m_Routes;

            private HttpListenerContext m_Ctx;

            private string m_RootDir;

            public Worker(HttpListenerContext ctx, Dictionary<string, RequestHandler> routes, string rootDir)
            {
                this.m_Ctx = ctx;
                this.m_Routes = routes;
                this.m_RootDir = rootDir;
            }

            [STAThread]
            public void ProcessRequest()
            {
                try
                {
                    if (this.m_Ctx.Request.Url.AbsolutePath.StartsWith("/static/"))
                    {
                        this.StaticFileHandler(this.m_Ctx.Request, this.m_Ctx.Response);
                    }
                    else
                    {
                        RequestHandler requestHandler = this.m_Routes[this.m_Ctx.Request.Url.AbsolutePath];
                        requestHandler.Invoke(this.m_Ctx.Request, this.m_Ctx.Response);
                    }
                }
                catch (KeyNotFoundException)
                {
                    Logger.Error("Exception: No Handler registered for " + this.m_Ctx.Request.Url.AbsolutePath);
                    this.m_Ctx.Response.StatusCode = 404;
                }
                catch (Exception ex2)
                {
                    Logger.Error("Exception while processing HTTP handler: " + ex2.ToString());
                    this.m_Ctx.Response.StatusCode = 500;
                }
                finally
                {
                    Logger.Debug("Closing Response Stream");
                    try
                    {
                        this.m_Ctx.Response.OutputStream.Close();
                    }
                    catch (Exception ex3)
                    {
                        Logger.Error("Exception during m_Ctx.Response.OutputStream.Close(): " + ex3.ToString());
                    }
                }
            }

            public void StaticFileHandler(HttpListenerRequest req, HttpListenerResponse res)
            {
                string text = Path.Combine(this.m_RootDir, req.Url.AbsolutePath.Substring(1).Replace("/", "\\"));
                Logger.Warning("StaticFileHandler : serving " + req.Url + " from " + text);
                if (File.Exists(text))
                {
                    byte[] array = File.ReadAllBytes(text);
                    if (text.EndsWith(".png") || text.EndsWith(".jpg") || text.EndsWith(".jpeg") || text.EndsWith(".gif") || text.EndsWith(".js") || text.EndsWith(".css") || text.EndsWith(".json"))
                    {
                        res.Headers.Add("Cache-Control: max-age=2592000");
                    }
                    res.OutputStream.Write(array, 0, array.Length);
                }
                else
                {
                    Logger.Error("File " + text + " doesn't exist");
                    res.StatusCode = 404;
                    res.StatusDescription = "Not Found.";
                }
            }
        }

        private HttpListener m_Listener;

        private int m_Port;

        private Dictionary<string, RequestHandler> m_Routes;

        private string m_RootDir;

        public int Port
        {
            get
            {
                return this.m_Port;
            }
        }

        public Dictionary<string, RequestHandler> Routes
        {
            get
            {
                return this.m_Routes;
            }
        }

        public string RootDir
        {
            get
            {
                return this.m_RootDir;
            }
        }

        public Server(int port, Dictionary<string, RequestHandler> routes, string rootDir)
        {
            this.m_Port = port;
            this.m_Routes = routes;
            this.m_RootDir = rootDir;
        }

        public void Start()
        {
            string uriPrefix = string.Format("http://{0}:{1}/", "*", this.m_Port);
            this.m_Listener = new HttpListener();
            this.m_Listener.Prefixes.Add(uriPrefix);
            try
            {
                this.m_Listener.Start();
            }
            catch (HttpListenerException ex)
            {
                Logger.Error("Failed to start listener. err: " + ex.ToString());
                throw new ENoPortAvailable(string.Format("No free port available"));
            }
        }

        public void Run()
        {
            while (true)
            {
                HttpListenerContext httpListenerContext = null;
                try
                {
                    httpListenerContext = this.m_Listener.GetContext();
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception while processing HTTP context: " + ex.ToString());
                    continue;
                }
                Worker @object = new Worker(httpListenerContext, this.Routes, this.RootDir);
                Thread thread = new Thread(@object.ProcessRequest);
                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
            }
        }
    }
}
