using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using WatsonWebserver;
using WatsonWebserver.Core;
using WatsonWebserver.Core.WebSockets;

namespace BardAfar
{
    /// <summary>
    /// Create a web server that does three things:
    /// 1) Accepts websocket connections and broadcasts to connected clients.
    /// 2) Serves a static HTML page.
    /// 3) Serves up static files.
    /// </summary>
    /// <remarks>
    /// This class makes heavy use of Watson Web Server: 
    /// https://github.com/dotnet/watsonwebserver
    /// </remarks>
    internal class WatsonServerManager
    {
        private Webserver? ServerPrimary;
        private Webserver? ServerSecondary;
        private ConcurrentDictionary<Guid, WebSocketSession> SessionsPrimary = new();
        private ConcurrentDictionary<Guid, WebSocketSession> SessionsSecondary = new();

        /// <summary>
        /// Returns whether the server is running.
        /// </summary>
        public bool IsListening
        {
            get
            {
                return (ServerPrimary != null && ServerPrimary.IsListening)
                    || (ServerSecondary != null && ServerSecondary.IsListening);
            }
        }

        /// <summary>
        /// Create and start the server.
        /// </summary>
        /// <param name="host">
        /// The host name or IP address to listen on.
        /// Use can use prefixes like +, *, or ::
        /// Or leave empty or null to listen on all addresses, IPv4 and IPv6.</param>
        /// <param name="port">The port to listen on.</param>
        /// <param name="clientPageConent">The HTML of the client page.</param>
        /// <param name="soundFilePath">The full path to the directory containing sound files.</param>
        public WatsonServerManager(string host, ushort port, string clientPageConent, string soundFilePath, CancellationToken cancellationToken)
        {
            // Check inputs:
            if (clientPageConent == null) throw new ArgumentNullException(nameof(clientPageConent));
            if (soundFilePath == null) throw new ArgumentNullException(nameof(soundFilePath));
            if (String.IsNullOrWhiteSpace(clientPageConent)) throw new ArgumentException(nameof(clientPageConent));
            if (!Directory.Exists(soundFilePath)) throw new DirectoryNotFoundException(nameof(soundFilePath));
            // Check host, may as well use our existing validator:
            var hostOrIpvalidator = new ValidationRuleHostOrIp();
            var validationResult = hostOrIpvalidator.Validate(host, CultureInfo.InvariantCulture);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException(nameof(host));
            }

            // Create server:
            if (String.IsNullOrEmpty(host))
            {
                // Blank host name? Then create two servers to cover IPv4 and IPv6:
                ServerPrimary = SetupServer("*", port, HandleWebSocketPrimary, clientPageConent, soundFilePath);
                ServerPrimary.Start();
                ServerSecondary = SetupServer("::", port, HandleWebSocketSecondary, clientPageConent, soundFilePath);
                ServerSecondary.Start();
            }
            else
            {
                // Host name specified? Then create as per that host:
                ServerPrimary = SetupServer(host, port, HandleWebSocketPrimary, clientPageConent, soundFilePath);
                ServerPrimary.Start();
                ServerSecondary = null;
            }
        }

        /// <summary>
        /// Broadcasts a message to all connected clients.
        /// </summary>
        public async void Broadcast(string message)
        {
            if (IsListening)
            {
                foreach (var session in SessionsPrimary.Values)
                {
                    if (session.IsConnected)
                    {
                        await session.SendTextAsync(message, CancellationToken.None);
                    }
                }
                foreach (var session in SessionsSecondary.Values)
                {
                    if (session.IsConnected)
                    {
                        await session.SendTextAsync(message, CancellationToken.None);
                    }
                }
            }
        }

        /// <summary>
        /// Closes the WebSocket server(s).
        /// </summary>
        public void Close()
        {
            if (ServerPrimary != null)
            {
                ServerPrimary.Stop();
                ServerPrimary.Dispose();
            }
        }

        // Setup a server.
        private Webserver SetupServer(string host, ushort port, Func<HttpContextBase, WebSocketSession, Task> webSocketFunc, string clientPageConent, string soundFilePath)
        {
            WebserverSettings settings = new WebserverSettings(host, port);
            settings.WebSockets.Enable = true;
            Webserver server = new Webserver(settings, DefaultRoute);
            server.WebSocket(Settings.Default.UrlPathWebSocket, webSocketFunc);
            server.Routes.PreAuthentication.Content.BaseDirectory = soundFilePath;
            server.Routes.PreAuthentication.Content.Add(Settings.Default.UrlPathFiles, true);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, Settings.Default.UrlPathPage, async (HttpContextBase ctx) =>
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "text/html";
                await ctx.Response.Send(clientPageConent);
            });
            return server;
        }

        // Default route, for when others fail.
        private async Task DefaultRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 404;
            await ctx.Response.Send("Not found.");
        }

        // Handle socket connections on primary connection.
        private async Task HandleWebSocketPrimary(HttpContextBase context, WebSocketSession session)
        {
            await HandleWebSocket(SessionsPrimary, context, session);
        }

        // Handle socket connections on secondary connection.
        private async Task HandleWebSocketSecondary(HttpContextBase context, WebSocketSession session)
        {
            await HandleWebSocket(SessionsSecondary, context, session);
        }

        // Generic session-handling method:
        private async Task HandleWebSocket(ConcurrentDictionary<Guid, WebSocketSession> sessions, HttpContextBase context, WebSocketSession session)
        {
            // Put IPv6 addresses in square brackets for logging purposes:
            string remoteIp = session.RemoteIp;
            if (IPAddress.TryParse(session.RemoteIp, out var address))
            {
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    remoteIp = "[" + session.RemoteIp + "]";
                }
            }

            sessions.TryAdd(session.Id, session);
            StaticLogger.AppendToLog(String.Format(Settings.Default.LogWebSocketConnectionOpenFormat, remoteIp, session.RemotePort));

            /*
            try
            {
                // Use a TaskCompletionSource to keep the connection open.
                // Do NOT read messages from the session. Ignore incoming data entirely.
                var tcs = new TaskCompletionSource<bool>();

                // Unbind the socket if the HTTP request/connection aborts
                using (context.Token.Register(() => tcs.TrySetResult(true)))
                {
                    await tcs.Task;
                }
            }
            */

            try
            {
                await foreach (var message in session.ReadMessagesAsync(context.Token))
                {
                    // We don't care about incoming messages.
                    // Reading them is nevertheless important because it allows
                    // Watson to observe WebSocket close/disconnect events.
                }
            }
            catch (OperationCanceledException)
            {
                // Server/request cancellation.
            }
            catch (Exception ex)
            {
                // An abnormal connection termination can arrive here.
                StaticLogger.AppendToLog(
                    $"WebSocket error from {remoteIp}:{session.RemotePort}: {ex.Message}");
            }
            finally
            {
                sessions.TryRemove(session.Id, out _);
                StaticLogger.AppendToLog(String.Format(Settings.Default.LogWebSocketConnectionCloseFormat, remoteIp, session.RemotePort));
            }
        }
    }
}
