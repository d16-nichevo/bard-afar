using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Resources;
using System.Windows.Threading;

namespace BardAfar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : Window
    {
        private ViewModel Model
        {
            get
            {
                return (ViewModel)DataContext;
            }
        }

        private HttpListener HttpListener;
        private WebSocketServer WebSocketServer;
        private CancellationTokenSource CancellationTokenSource;
        private DispatcherTimer Timer = new DispatcherTimer(DispatcherPriority.DataBind);
        private Random Random = new Random();

        //////////////////////////////////////////
        //  LIFE CYCLE METHODS
        //////////////////////////////////////////

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // We let PlayerControls be visible in the XAML so we can
            // look at and edit it in the Designer. That means we have
            // to hide it programmatically. We do that here rather than
            // Window_Loaded so there's no "flash" of quick visibility.
            PlayerControls.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Window loading complete. Perform initialisation tasks.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // If there's a newer version of the settings files,
            // upgrade to using it:
            if (Settings.Default.UpdateSettings)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                Settings.Default.Save();
            }

            // Force validation on server setup parameters, in case
            // the default values are bad:
            foreach (var textBox in new System.Windows.Controls.TextBox[] { HostOrIp, PortHttp, PortWs, AudioDir, TrackPadding })
            {
                textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }

            // Prepare the static logger. This is a horrible workaround.
            // See StaticLogger class for more information.
            StaticLogger.Model = Model;

            // Setup the timer that shows time until track ending:
            Timer.Interval = TimeSpan.FromMilliseconds(Settings.Default.TrackTimeRefreshIntervalMs);
            Timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Window is closing, which means application is closing.
        /// Perform clean-up tasks.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            PlayerStop();
            MediaElement.Close();
            StopHosts();
        }

        /// <summary>
        /// Start the WebSocket and HttpListener hosts.
        /// </summary>
        private void StartHosts()
        {
            CancellationTokenSource = new CancellationTokenSource();

            // Save our settings:
            SaveSettings();

            // Lock down controls:
            SetupControls.IsEnabled = false;

            // Pre-load client page HTML:
            Uri uri = new Uri("/ClientPage.html", UriKind.Relative);
            StreamResourceInfo info = System.Windows.Application.GetResourceStream(uri);
            string clientPageText = new StreamReader(info.Stream).ReadToEnd();
            clientPageText = clientPageText.Replace(Settings.Default.ClientPagePortToken, Convert.ToString(Model.PortWebSocket));

            // Start hosts:
            HttpListener = new HttpListener(Model.HostOrIpAddress, Model.PortHttpListener, Model.AudioFilesDirectory, clientPageText, CancellationTokenSource.Token);
            WebSocketServer = new WebSocketServer(Model.HostOrIpAddress, Model.PortWebSocket);

            // Update ListBox:
            UpdateListBox(Model.AudioFilesDirectory);

            // Update log:
            Model.AppendToLog("Server started.");

            // Hide setup controls, show player controls:
            SetupControls.Visibility = Visibility.Collapsed;
            PlayerControls.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Stop the WebSocket and HttpListener hosts.
        /// </summary>
        private async void StopHosts()
        {
            // Stop hosts.
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();

                // Let the Web Socket closing task take the time it needs,
                // as it doesn't seem to misbehave:
                Task taskCloseWebSocketServer = Task.Run(() => WebSocketServer.Close());
                await taskCloseWebSocketServer;

                // But don't wait very long for the HttpListener.
                // It doesn't seem to heed its CancellationToken so
                // we manually kill it. (This is something I might
                // like to investigate and fix in the future.)
                await HttpListener.Task.WaitAsync(TimeSpan.FromMilliseconds(Settings.Default.AppExitTimeoutMs));
            }
        }

        //////////////////////////////////////////
        //  WEBSOCKET BROADCASTS
        //////////////////////////////////////////

        /// <summary>
        /// Broadcast to play a song.
        /// </summary>
        /// <param name="path">A path relative to the server path. E.g. "subDir/myMusic.mp3".</param>
        private void BroadcastPlay(string path)
        {
            if (WebSocketServer != null)
            {
                if (path == null) throw new ArgumentNullException(nameof(path));
                path = path.Replace(@"\", "/");
                path = HttpUtility.UrlPathEncode(path);
                WebSocketServer.Broadcast(Settings.Default.WebSocketServerPath + path);
            }
        }

        /// <summary>
        /// Broadcast to stop playback.
        /// </summary>
        private void BroadcastStop()
        {
            if (WebSocketServer != null)
            {
                WebSocketServer.Broadcast(Settings.Default.BroadcastStop);
            }
        }

        //////////////////////////////////////////
        //  PLAYER METHODS
        //////////////////////////////////////////

        /// <summary>
        /// Play the selected item in the list box, or find one to play if none selected.
        /// </summary>
        private void PlayerPlay(System.Windows.Controls.ListBox listBox)
        {
            // If a file isn't selected, try to select a file:
            if (listBox.SelectedItem == null || !((ListBoxItem)listBox.SelectedItem).isFile)
            {
                PlayerNext(listBox);
            }

            // Did we find a file?
            if (listBox.SelectedItem != null && ((ListBoxItem)listBox.SelectedItem).isFile)
            {
                // Found a file. Play it:
                BroadcastPlay(((ListBoxItem)listBox.SelectedItem).GetServerPath(Model.AudioFilesDirectory));
                // Temporarily set MediaElement source to null.
                // We do this so it sees a change even if we repeat the same track.
                MediaElement.Source = null;
                // Setting the source will kick off the event MediaElement_MediaOpened
                // where the timer is set:
                MediaElement.Source = new Uri(((ListBoxItem)listBox.SelectedItem).FullPath);
                MediaElement.Play();
                Model.Playing = true;
            }
            else
            {
                // Didn't find a file. Stop playback.
                BroadcastStop();
                Model.Playing = false;
            }
        }

        /// <summary>
        /// Select the next file in the ListBox.
        /// </summary>
        private void PlayerNext(System.Windows.Controls.ListBox listBox)
        {
            int fileCount = listBox.Items.OfType<ListBoxItem>().Count(x => x.isFile);

            if (fileCount == 0)
            {
                // If there aren't any items of type file, we can't do anything,
                // so stop playing:                
                Model.Playing = false;
                listBox.SelectedItem = null;
            }
            else if (fileCount == 1)
            {
                // Just one file? Well, select it:
                listBox.SelectedItem = listBox.Items.OfType<ListBoxItem>().First(x => x.isFile);
            }
            else if (Model.IterationMode == IterationMode.Next)
            {
                // Select next track.
                int i = listBox.SelectedIndex;
                // When no item is selected, i will be -1. But that works well.
                // Iterate through until we find the next file:
                do
                {
                    i = (i + 1) % listBox.Items.Count;
                } while (!((ListBoxItem)listBox.Items[i]).isFile);
                listBox.SelectedIndex = i;
            }
            else
            {
                // Choose randomly. Just shuffle about until we find a new track.
                var nextTrack = listBox.SelectedItem;
                while (nextTrack == listBox.SelectedItem || !((ListBoxItem)nextTrack).isFile)
                {
                    nextTrack = listBox.Items[Random.Next(listBox.Items.Count)];
                }
                listBox.SelectedItem = nextTrack;
            }
            listBox.ScrollIntoView(listBox.SelectedItem);

            // If were playing, then play this new track:
            if (Model.Playing)
            {
                PlayerPlay(DirectoryListBox);
            }
        }

        /// <summary>
        /// Stop any ongoing playback.
        /// </summary>
        private void PlayerStop()
        {
            MediaElement.Stop();
            BroadcastStop();
            Timer.IsEnabled = false;
            Model.TrackEndTime = DateTime.Now;
            Model.Playing = false;
        }

        //////////////////////////////////////////
        //  HELPER METHODS
        //////////////////////////////////////////
        private void SaveSettings()
        {
            Settings.Default.ServerInfoHostOrIpAddress = Model.HostOrIpAddress;
            Settings.Default.ServerInfoPortHttpListener = Model.PortHttpListener;
            Settings.Default.ServerInfoPortWebSocket = Model.PortWebSocket;
            Settings.Default.ServerInfoAudioDirectory = Model.AudioFilesDirectory;
            Settings.Default.Volume = Model.Volume;
            Settings.Default.LoopMode = (int)Model.LoopMode;
            Settings.Default.IterationMode = (int)Model.IterationMode;
            Settings.Default.TrackPaddingSeconds = Model.TrackPaddingSeconds;
            Settings.Default.Save();
        }

        /// <summary>
        /// Update the List Box to show contents of a new directory.
        /// </summary>
        private void UpdateListBox(string directory)
        {
            if (String.IsNullOrEmpty(directory)) throw new ArgumentNullException(nameof(directory));
            if (!Directory.Exists(directory)) throw new DirectoryNotFoundException(nameof(directory));

            // Clear any current items:
            DirectoryListBox.Items.Clear();

            // Add a "parent directory" item, if we're not
            // at the top-most-allowed directory:
            var thisUri = new Uri(directory);
            var topUrl = new Uri(Model.AudioFilesDirectory);
            if (topUrl != thisUri && topUrl.IsBaseOf(thisUri))
            {
                ListBoxItem lbi = new ListBoxItem(Directory.GetParent(directory).FullName, true);
                DirectoryListBox.Items.Add(lbi);
            }

            // Next, add sub-directories in this directory:
            Directory.EnumerateDirectories(directory)
                .Select(System.IO.Path.GetFullPath)
                .ToList()
                .ForEach(x => DirectoryListBox.Items.Add(new ListBoxItem(x)));

            // Next, add files in this directory:
            List<string> supportedExtensions = Settings.Default.SupportedAudioExtensions.Split(',').ToList();
            Directory.EnumerateFiles(directory)
                .Select(System.IO.Path.GetFullPath)
                .Where(x => !String.IsNullOrEmpty(System.IO.Path.GetExtension(x)) && supportedExtensions.Any(y => System.IO.Path.GetExtension(x).Equals("." + y, StringComparison.OrdinalIgnoreCase)))
                .ToList()
                .ForEach(x => DirectoryListBox.Items.Add(new ListBoxItem(x)));
        }

        //////////////////////////////////////////
        //  NON-UI EVENTS
        //////////////////////////////////////////

        /// <summary>
        /// Decrement the time left on the current playing track.
        /// </summary>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            Model.RefreshTrackTime();

            // Did the track end?
            if (Model.TrackRemaining <= TimeSpan.Zero)
            {
                // Stop the timer:
                Timer.IsEnabled = false;

                // What happens next depends on iteration mode:
                if (Model.LoopMode == LoopMode.Stop)
                {
                    PlayerStop();
                }
                else if (Model.LoopMode == LoopMode.RepeatTrack)
                {
                    PlayerPlay(DirectoryListBox);
                }
                else if (Model.LoopMode == LoopMode.RepeatDirectory)
                {
                    PlayerNext(DirectoryListBox);
                }
            }
        }

        /// <summary>
        /// When the MediaElement opens new contect, read the duration
        /// and use it for our track timer.
        /// </summary>
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (MediaElement.NaturalDuration.HasTimeSpan)
            {
                Model.TrackEndTime = DateTime.Now.AddSeconds(MediaElement.NaturalDuration.TimeSpan.TotalSeconds)
                                        .AddSeconds(Settings.Default.TrackPaddingSeconds);
                Timer.IsEnabled = true;
            }
        }

        //////////////////////////////////////////
        //  UI EVENTS
        //////////////////////////////////////////

        /// <summary>
        /// User clicked to start the server.
        /// </summary>
        private void StartHost_Click(object sender, RoutedEventArgs e)
        {
            StartHosts();
        }

        /// <summary>
        /// User clicked to browse for audio directory.
        /// </summary>
        private void BrowseAudioDirectory_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.InitialDirectory = Model.AudioFilesDirectory;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Model.AudioFilesDirectory = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// User is releasing a mouse button while over the ListBox.
        /// We use this to detect clicks.
        /// </summary>
        private void DirectoryListBox_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Only care about left-clicks.
            if (e.ChangedButton != System.Windows.Input.MouseButton.Left)
            {
                e.Handled = true;
                return;
            }

            ListBoxItem selectedItem = (ListBoxItem)((System.Windows.Controls.ListBox)sender).SelectedItem;
            if (selectedItem == null)
            {
                // Nothing clicked. Do nothing.
            }
            else if (selectedItem.isDirectory)
            {
                // Directory clicked. Navigate.
                UpdateListBox(selectedItem.FullPath);
            }
            else
            {
                // File clicked. Play.
                PlayerPlay(DirectoryListBox);
            }
        }

        /// <summary>
        /// User clicked the Play button.
        /// </summary>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerPlay(DirectoryListBox);
        }

        /// <summary>
        /// User clicked the Next Track button.
        /// </summary>
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerNext(DirectoryListBox);
        }

        /// <summary>
        /// User clicked the Stop button.
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerStop();
        }

        /// <summary>
        /// User clicked buttons to select Iteration Modes.
        /// </summary>
        private void IterateNext_Click(object sender, RoutedEventArgs e)
        {
            Model.IterationMode = IterationMode.Next;
        }
        private void IterateRandom_Click(object sender, RoutedEventArgs e)
        {
            Model.IterationMode = IterationMode.Random;
        }

        /// <summary>
        /// User clicked buttons to select Loop Modes.
        /// </summary>
        private void LoopStop_Click(object sender, RoutedEventArgs e)
        {
            Model.LoopMode = LoopMode.Stop;
        }
        private void LoopRepeatTrack_Click(object sender, RoutedEventArgs e)
        {
            Model.LoopMode = LoopMode.RepeatTrack;
        }
        private void LoopRepeatDirectory_Click(object sender, RoutedEventArgs e)
        {
            Model.LoopMode = LoopMode.RepeatDirectory;
        }
    }
}
