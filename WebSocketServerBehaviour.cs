using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace BardAfar
{
    /// <summary>
    /// A WebSocketBehavior, used in WebSocketServer.
    /// It defines what to do when certain web socket events happen.
    /// </summary>
    internal class WebSocketServerBehaviour : WebSocketBehavior
    {
        // Note: a static logger is used. It's a workaround. See StaticLogger class for more info.

        // I believe an new instance of this class is created per
        // connection. So it's safe to store session variables in here,
        // such as the client's endpoint info:
        private string UserEndPointString = String.Empty;
        protected override void OnOpen()
        {
            // When a client opens a connection, we note this in the log.
            UserEndPointString = this.Context.UserEndPoint.ToString();
            StaticLogger.AppendToLog(String.Format(Settings.Default.LogWebSocketConnectionOpenFormat, UserEndPointString));
            base.OnOpen();
        }

        protected override void OnClose(CloseEventArgs e)
        {
            // When a client's connection closes, we note this in the log.
            StaticLogger.AppendToLog(String.Format(Settings.Default.LogWebSocketConnectionCloseFormat, UserEndPointString));
            base.OnClose(e);
        }
    }
}
