using System;
using System.Globalization;

namespace BardAfar
{
    /// <summary>
    /// Create a simple WebSocket server that can broadcast to all connected clients.
    /// </summary>
    /// <remarks>
    /// This class makes heavy use of WebSocketSharp, which is a NuGet library. 
    /// https://github.com/sta/websocket-sharp/
    /// </remarks>
    internal class WebSocketServer
    {
        private WebSocketSharp.Server.WebSocketServer Server;

        /// <summary>
        /// Returns whether the server is running.
        /// </summary>
        public bool IsListening
        {
            get
            {
                return Server != null && Server.IsListening;
            }
        }

        /// <summary>
        /// Create and start the WebSocket server.
        /// </summary>
        /// <param name="host">The host name or IP address to listen on.</param>
        /// <param name="port">The port to listen on.</param>
        public WebSocketServer(string host, ushort port)
        {
            // Check inputs:
            if (host == null) throw new ArgumentNullException(nameof(host));
            // Check host, may as well use our existing validator:
            var hostOrIpvalidator = new ValidationRuleHostOrIp();
            var validationResult = hostOrIpvalidator.Validate(host, CultureInfo.InvariantCulture);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException(nameof(host));
            }

            // Build the URL. Should end up like ws://myhost:9999
            string url = String.Format(Settings.Default.WebSocketServerUrlFormat, host, port);

            // Create the server with our behaviour and start it:
            Server = new WebSocketSharp.Server.WebSocketServer(port);
            Server.AddWebSocketService<WebSocketServerBehaviour>(Settings.Default.WebSocketServerPath);
            Server.Start();
        }

        /// <summary>
        /// Broadcasts a message to all connected clients.
        /// </summary>
        public void Broadcast(string message)
        {
            // Check server state:
            if (!this.IsListening) throw new InvalidOperationException("WebSocket server is inactive.");

            // Broadcast it:
            Server.WebSocketServices.Broadcast(message);
        }

        /// <summary>
        /// Closes the WebSocket server.
        /// </summary>
        public void Close()
        {
            if (Server != null)
            {
                Server.Stop();
            }
        }
    }
}
