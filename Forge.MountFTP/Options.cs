using Dokan;

namespace Forge.MountFTP
{
    /// <summary>
    /// Settings for the <see cref="Forge.MountFTP.Drive" />.
    /// </summary>
    public class Options : IFtpOptions
    {
        /// <summary>
        /// Gets or sets the name of the host to establish an FTP connection to.
        /// </summary>
        /// <value>
        /// The name of the host.
        /// </value>
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the name of the user to use for logging onto the FTP server.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password to use for logging onto the FTP server.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the drive letter of the virtual drive to mount the FTP server in.
        /// </summary>
        /// <value>
        /// The drive letter.
        /// </value>
        public char DriveLetter { get; set; }

        internal DokanOptions GetDokanOptions()
        {
            return new DokanOptions
            {
                MountPoint = DriveLetter + ":\\"
            };
        }
    }
}