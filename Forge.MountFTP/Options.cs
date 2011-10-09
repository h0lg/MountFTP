using Dokan;

namespace Forge.MountFTP
{
    /// <summary>
    /// Settings for the <see cref="Forge.MountFTP.Drive" />.
    /// </summary>
    public class Options
    {
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