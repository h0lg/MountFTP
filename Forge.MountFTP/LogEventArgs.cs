using System;

namespace Forge.MountFTP
{
    /// <summary>
    /// A delegate for logging events in MountFTP.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="Forge.MountFTP.LogEventArgs"/> instance containing the event data.</param>
    public delegate void LogEventHandler(object sender, LogEventArgs args);

    /// <summary>
    /// Event data for <see cref="LogEventHandler"/>.
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the event message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventArgs"/> class.
        /// </summary>
        /// <param name="message">The event message.</param>
        public LogEventArgs(string message)
        {
            Message = message;
        }
    }
}