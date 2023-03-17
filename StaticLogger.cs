namespace BardAfar
{
    /// <summary>
    /// This is a horrible workaround. I could not figure out how
    /// to pass a logging object to the WebSocketServer, so I made
    /// this static class. Hopefully I can pass it properly one day
    /// so I can remove this class.
    /// </summary>
    internal static class StaticLogger
    {
        // A reference to the Model. The MainWindow passes this in.
        public static ViewModel? Model { get; set; }

        public static void AppendToLog(string entry)
        {
            if (Model != null)
            {
                Model.AppendToLog(entry);
            }
        }
    }
}
