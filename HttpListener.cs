using SimpleHttp;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BardAfar
{
    /// <summary>
    /// A class to wrap the Simple-HTTP NuGet package.
    /// </summary>
    /// <remarks>
    /// More info about this package here: 
    /// https://github.com/dajuric/simple-http
    /// </remarks>
    internal class HttpListener
    {
        public Task Task { get; private set; }

        /// <summary>
        /// Create and start the HttpListener.
        /// </summary>
        /// <param name="host">The host name or IP address to listen on.</param>
        /// <param name="port">The port to listen on.</param>
        /// <param name="soundFilesPath">The full path to the directory that will be served by the Listener.</param>
        /// <param name="cancellationToken">A cancellation token to stop the Listner task.</param>
        /// <param name="clientPageContent">The full HTML of the interactive page served to clients.</param>
        public HttpListener(string host, ushort port, string soundFilesPath, string clientPageContent, CancellationToken cancellationToken)
        {
            // Check inputs:
            if (clientPageContent == null) throw new ArgumentNullException(nameof(clientPageContent));
            if (host == null) throw new ArgumentNullException(nameof(host));
            // Check host, may as well use our existing validator:
            var hostOrIpvalidator = new ValidationRuleHostOrIp();
            var validationResult = hostOrIpvalidator.Validate(host, CultureInfo.InvariantCulture);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException(nameof(host));
            }

            // Create routes.
            Route.Add((request, args) =>
            {
                // Accept all requests.
                return true;
            },
            (request, response, args) =>
            {
                // Do different things based on the URL in the request.
                // Check both with and without trailing slash:
                if (request.Url.AbsolutePath.TrimEnd('/').Equals(Settings.Default.HttpListenerServerPathBase.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                {
                    // URL is simply the server base, so serve interactive HTML page:
                    response.AsText(clientPageContent);
                }
                else if (request.Url.AbsolutePath.StartsWith(Settings.Default.HttpListenerServerPathBase, StringComparison.OrdinalIgnoreCase))
                {
                    // URL is more than the server base, so try to serve file:
                    string soundFilePath = request.Url.AbsolutePath.Substring(Settings.Default.HttpListenerServerPathBase.Length);
                    soundFilePath = Path.Combine(soundFilesPath, soundFilePath);
                    soundFilePath = HttpUtility.UrlDecode(soundFilePath);
                    if (File.Exists(soundFilePath))
                    {
                        response.AsFile(request, soundFilePath);
                    }
                }
            });

            // Build the prefix. Should end up like http://myhost:9999/
            string prefix = String.Format(Settings.Default.HttpListenerPrefixFormat, host, port);

            // Start the task:
            Task = HttpServer.ListenAsync(prefix, cancellationToken, Route.OnHttpRequestAsync, Settings.Default.HttpListenerMaxConnections);
        }
    }
}
