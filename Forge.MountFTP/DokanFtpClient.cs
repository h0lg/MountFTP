using System;
using System.Collections;
using System.IO;
using Dokan;

namespace Forge.MountFTP
{
    /// <summary>
    /// An FTP client implementing <see cref="Dokan.DokanOperations"/>,
    /// thus enabling it to be mounted using Dokan.
    /// </summary>
    class DokanFtpClient : DokanOperations
    {
        internal event LogEventHandler MethodCall, Debug;

        #region DokanOperations

        public int Cleanup(string filename, DokanFileInfo info)
        {
            RaiseMethodCall("Cleanup " + filename);
            return -1;
        }

        public int CloseFile(string filename, DokanFileInfo info)
        {
            RaiseMethodCall("CloseFile " + filename);
            return -1;
        }

        public int CreateDirectory(string filename, DokanFileInfo info)
        {
            RaiseMethodCall("CreateDirectory " + filename);
            return -1;
        }

        public int CreateFile(string filename, FileAccess access, FileShare share, FileMode mode, FileOptions options, DokanFileInfo info)
        {
            RaiseMethodCall("CreateFile " + filename);
            return -1;
        }

        public int DeleteDirectory(string filename, DokanFileInfo info)
        {
            RaiseMethodCall("DeleteDirectory " + filename);
            return -1;
        }

        public int DeleteFile(string filename, DokanFileInfo info)
        {
            RaiseMethodCall("DeleteFile " + filename);
            return -1;
        }

        public int FindFiles(string filename, ArrayList files, DokanFileInfo info)
        {
            RaiseMethodCall("FindFiles " + filename);
            return -1;
        }

        public int FlushFileBuffers(string filename, DokanFileInfo info)
        {
            RaiseMethodCall("FlushFileBuffers " + filename);
            return -1;
        }

        public int GetDiskFreeSpace(ref ulong freeBytesAvailable, ref ulong totalBytes, ref ulong totalFreeBytes, DokanFileInfo info)
        {
            RaiseMethodCall("GetDiskFreeSpace");
            return -1;
        }

        public int GetFileInformation(string filename, FileInformation fileinfo, DokanFileInfo info)
        {
            RaiseMethodCall("GetFileInformation " + filename);
            return -1;
        }

        public int LockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            RaiseMethodCall("LockFile " + filename);
            return -1;
        }

        public int MoveFile(string filename, string newname, bool replace, DokanFileInfo info)
        {
            RaiseMethodCall("MoveFile " + filename + " to " + newname);
            return -1;
        }

        public int OpenDirectory(string filename, DokanFileInfo info)
        {
            RaiseMethodCall("OpenDirectory " + filename);
            return -1;
        }

        public int ReadFile(string filename, byte[] buffer, ref uint readBytes, long offset, DokanFileInfo info)
        {
            RaiseMethodCall("ReadFile " + filename);
            return -1;
        }

        public int SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            RaiseMethodCall("SetAllocationSize " + filename);
            return -1;
        }

        public int SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            RaiseMethodCall("SetEndOfFile " + filename);
            return -1;
        }

        public int SetFileAttributes(string filename, FileAttributes attr, DokanFileInfo info)
        {
            RaiseMethodCall("SetFileAttributes " + filename);
            return -1;
        }

        public int SetFileTime(string filename, DateTime ctime, DateTime atime, DateTime mtime, DokanFileInfo info)
        {
            RaiseMethodCall("SetFileTime " + filename);
            return -1;
        }

        public int UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            RaiseMethodCall("UnlockFile " + filename);
            return -1;
        }

        public int Unmount(DokanFileInfo info)
        {
            RaiseMethodCall("Unmount");
            return -1;
        }

        public int WriteFile(string filename, byte[] buffer, ref uint writtenBytes, long offset, DokanFileInfo info)
        {
            RaiseMethodCall("WriteFile " + filename);
            return -1;
        }

        #endregion

        void RaiseMethodCall(string message)
        {
            if (MethodCall != null)
            {
                MethodCall(this, new LogEventArgs(message));
            }
        }

        void RaiseDebug(string message)
        {
            if (Debug != null)
            {
                Debug(this, new LogEventArgs(message));
            }
        }
    }
}