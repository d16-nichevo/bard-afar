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
            string errorMessage = string.Format("An unhandled exception occurred: {0}\n\nWould you like to see a full stack trace?", e.Exception.Message);
            if (MessageBox.Show(errorMessage, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
            {
                MessageBox.Show(string.Format("Stack trace follows. Press Ctrl+C to copy to clipboard.\n\n{0}", e.Exception.ToString()), "Stack Tracks", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
