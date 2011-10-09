using Dokan;

namespace Forge.MountFTP
{
    /// <summary>
    /// (Un)Mounts an FTP server specified in its <see cref="Options"/> as a virtual drive.
    /// This Facade is the entry point for the MountFTP library.
    /// </summary>
    public class Drive
    {
        readonly Options options;

        /// <summary>
        /// Occurs when a method on the FTP client is called.
        /// </summary>
        public event LogEventHandler FtpClientMethodCall;

        /// <summary>
        /// Occurs when the FTP client raises a debug event.
        /// </summary>
        public event LogEventHandler FtpClientDebug;

        /// <summary>
        /// Initializes a new instance of the <see cref="Drive"/> class.
        /// </summary>
        /// <param name="options">The options for the <see cref="Drive"/>.</param>
        public Drive(Options options)
        {
            this.options = options;
        }

        /// <summary>
        /// Mounts the <see cref="Drive"/>.
        /// </summary>
        /// <returns></returns>
        public string Mount()
        {
            string result;

            var dokanFtpClient = new DokanFtpClient();
            dokanFtpClient.MethodCall += new LogEventHandler(OnFtpClientMethodCall);
            dokanFtpClient.Debug += new LogEventHandler(OnFtpClientDebug);

            var status = DokanNet.DokanMain(options.GetDokanOptions(), dokanFtpClient);
            switch (status)
            {
                case DokanNet.DOKAN_DRIVE_LETTER_ERROR:
                    result = "Drive letter error";
                    break;
                case DokanNet.DOKAN_DRIVER_INSTALL_ERROR:
                    result = "Driver install error";
                    break;
                case DokanNet.DOKAN_MOUNT_ERROR:
                    result = "Mount error";
                    break;
                case DokanNet.DOKAN_START_ERROR:
                    result = "Start error";
                    break;
                case DokanNet.DOKAN_ERROR:
                    result = "Unknown error";
                    break;
                case DokanNet.DOKAN_SUCCESS:
                    result = "Success";
                    break;
                default:
                    result = string.Format("Unknown status: %d", status);
                    break;
            }

            return result;
        }

        void OnFtpClientMethodCall(object sender, LogEventArgs args)
        {
            if (FtpClientMethodCall != null)
            {
                FtpClientMethodCall(sender, args);
            }
        }

        void OnFtpClientDebug(object sender, LogEventArgs args)
        {
            if (FtpClientDebug != null)
            {
                FtpClientDebug(sender, args);
            }
        }

        ~Drive()
        {
            DokanNet.DokanUnmount(options.DriveLetter);
        }
    }
}