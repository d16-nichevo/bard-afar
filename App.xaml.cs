using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace BardAfar
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal partial class App : Application
    {
        public App()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        /// <summary>
        /// Last-chance exception handling.
        /// </summary>
        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Handle a common error where the OS disallows access:
            if (e.Exception is UnauthorizedAccessException
                && e.Exception.InnerException != null
                && e.Exception.InnerException is System.Net.HttpListenerException
                && ((System.Net.HttpListenerException)(e.Exception.InnerException)).ErrorCode == 5)
            {
                string errorMessage = "The HTTP server can not be started, as the namespace reservation does not exist."
                        + System.Environment.NewLine + System.Environment.NewLine
                        + "To use this address, please run Bard Afar as Administrator.";

                Match match = Regex.Match(e.Exception.Message, @"'([^']*)'");
                if (match.Success)
                {
                    errorMessage +=
                        System.Environment.NewLine + System.Environment.NewLine
                        + "Alternatively, run this at an elevated command line, then re-run Bard Afar:"
                        + System.Environment.NewLine + System.Environment.NewLine
                        + match.Groups[1].Value;
                }

                MessageBox.Show(errorMessage, "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // Handle other errors:
            else
            {
                string errorMessage = string.Format("An unhandled exception occurred: {0}\n\nWould you like to see a full stack trace?", e.Exception.Message);
                if (MessageBox.Show(errorMessage, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    MessageBox.Show(string.Format("Stack trace follows. Press Ctrl+C to copy to clipboard.\n\n{0}", e.Exception.ToString()), "Stack Tracks", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
