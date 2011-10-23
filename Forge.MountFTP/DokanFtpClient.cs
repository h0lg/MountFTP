using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AlexPilotti.FTPS.Client;
using AlexPilotti.FTPS.Common;
using Dokan;

namespace Forge.MountFTP
{
    /// <summary>
    /// An FTP client implementing <see cref="Dokan.DokanOperations"/>,
    /// thus enabling it to be mounted using Dokan.
    /// </summary>
    class DokanFtpClient : DokanOperations
    {
        string bufferedFileName;
        byte[] bufferedFile;
        int bufferedBytes;
        readonly FTPSClient fTPSClient;
        readonly IFtpOptions ftpOptions;
        readonly Dictionary<string, DirectoryFileInformation> cachedDirectoryFileInformation = new Dictionary<string, DirectoryFileInformation>();
        readonly BlockingCollection<Task> fTPSClientTaskQueue = new BlockingCollection<Task>();

        internal event LogEventHandler MethodCall, Debug;

        internal DokanFtpClient(FTPSClient fTPSClient, IFtpOptions ftpOptions)
        {
            this.fTPSClient = fTPSClient;
            this.ftpOptions = ftpOptions;

            const string ROOT = "\\";
            cachedDirectoryFileInformation.Add(
                ROOT,
                new DirectoryFileInformation(true)
                {
                    FileName = ROOT
                });

            this.fTPSClient.Connect(
                ftpOptions.HostName,
                new NetworkCredential(ftpOptions.UserName, ftpOptions.Password),
                ESSLSupportMode.ClearText);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    fTPSClientTaskQueue.Take().RunSynchronously();
                }
            });
        }

        #region DokanOperations

        public int Cleanup(string filename, DokanFileInfo info)
        {
            RaiseMethodCall("Cleanup " + filename);
            return 0;
        }

        public int CloseFile(string filename, DokanFileInfo info)
        {
            RaiseMethodCall("CloseFile " + filename);
            return 0;
        }

        public int CreateDirectory(string filename, DokanFileInfo info)
        {
            RaiseMethodCall("CreateDirectory " + filename);

            EnqueueTask(() => fTPSClient.MakeDir(filename)).Wait();

            return 0;
        }

        public int CreateFile(string filename, FileAccess access, FileShare share, FileMode mode, FileOptions options, DokanFileInfo info)
        {
            RaiseMethodCall(string.Format("CreateFile {0} FileMode: {1}", filename, mode));

            switch (mode)
            {
                case FileMode.Append:
                    break;
                case FileMode.Create:
                    break;
                case FileMode.CreateNew:
                    EnqueueTask(() => fTPSClient.PutFile(filename).Dispose()).Wait();
                    return 0;
                case FileMode.Open:
                    if (cachedDirectoryFileInformation.ContainsKey(filename))
                    {
                        info.IsDirectory = cachedDirectoryFileInformation[filename].IsDirectory;
                        return 0;
                    }
                    else
                    {
                        RaiseDebug("CreateFile not cached: " + filename);
                        return -DokanNet.ERROR_FILE_NOT_FOUND;
                    }
                case FileMode.OpenOrCreate:
                    break;
                case FileMode.Truncate:
                    break;
                default:
                    break;
            }

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

            EnqueueTask(() => fTPSClient.DeleteFile(filename)).Wait();

            return 0;
        }

        public int FindFiles(string filename, ArrayList files, DokanFileInfo info)
        {
            RaiseMethodCall("FindFiles " + filename);

            var parentDirectory = filename;
            const string BACKSLASH = "\\";
            if (!filename.EndsWith(BACKSLASH))
            {
                parentDirectory += BACKSLASH;
            }

            IList<DirectoryListItem> directoryList = null;
            EnqueueTask(() => directoryList = fTPSClient.GetDirectoryList(GetUrl(filename))).Wait();

            directoryList
                .Select(dli => GetDirectoryFileInformation(parentDirectory, dli))
                .ForEach(dfi =>
                {
                    cachedDirectoryFileInformation[parentDirectory + dfi.FileName] = dfi;
                    files.Add(dfi);
                });

            return 0;
        }

        public int FlushFileBuffers(string filename, DokanFileInfo info)
        {
            RaiseMethodCall("FlushFileBuffers " + filename);
            return -1;
        }

        public int GetDiskFreeSpace(ref ulong freeBytesAvailable, ref ulong totalBytes, ref ulong totalFreeBytes, DokanFileInfo info)
        {
            RaiseMethodCall("GetDiskFreeSpace");

            totalFreeBytes =
            freeBytesAvailable = 1073741824; // == 1GB
            totalBytes = (ulong)cachedDirectoryFileInformation
                .Select(dfi => dfi.Value.Length)
                .Aggregate((sum, length) => sum += length)
                + totalFreeBytes;

            return 0;
        }

        public int GetFileInformation(string filename, FileInformation fileinfo, DokanFileInfo info)
        {
            RaiseMethodCall("GetFileInformation " + filename);

            if (cachedDirectoryFileInformation.ContainsKey(filename))
            {
                var cachedFileInfo = cachedDirectoryFileInformation[filename];

                fileinfo.FileName = cachedFileInfo.FileName;
                fileinfo.CreationTime = cachedFileInfo.CreationTime;
                fileinfo.LastAccessTime = cachedFileInfo.LastAccessTime;
                fileinfo.LastWriteTime = cachedFileInfo.LastWriteTime;
                fileinfo.Length = cachedFileInfo.Length;
                fileinfo.Attributes = cachedFileInfo.Attributes;

                return 0;
            }
            else
            {
                RaiseDebug("GetFileInformation not cached: " + filename);
                return -DokanNet.ERROR_FILE_NOT_FOUND;
            }
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
            return 0;
        }

        public int ReadFile(string filename, byte[] buffer, ref uint readBytes, long offset, DokanFileInfo info)
        {
            RaiseMethodCall("ReadFile " + filename);

            int read = 0;

            // is the request for the file that is currently downloading?
            if (filename == bufferedFileName)
            {
                // read from buffer
                if ((int)offset < bufferedBytes) // are requested bytes buffered?
                {
                    var availableBytes = bufferedBytes - (int)offset;
                    read = buffer.Length < availableBytes ? // if less bytes than available are requested
                        buffer.Length : // return the requested bytes
                        availableBytes; // else return all available bytes
                    // copy requested bytes to buffer
                    Array.Copy(
                        bufferedFile, // source
                        (int)offset, // source start index
                        buffer, // target
                        0, // target start index
                        read); // number of bytes to copy
                }
                else
                {
                    // requested bytes are not yet buffered
                    return -1;
                }
            }
            else
            {
                // enqueue new task
                EnqueueTask(() =>
                {
                    using (var ftpStream = fTPSClient.GetFile(filename))
                    {
                        bufferedFileName = filename;
                        bufferedFile = new byte[GetCachedLength(filename)];
                        int chunkSize;
                        while ((chunkSize = ftpStream.Read(bufferedFile, read, bufferedFile.Length - read)) != 0)
                        {
                            bufferedBytes = read += chunkSize;
                        }
                    }
                }).Wait();
            }
            readBytes = (uint)read;
            return 0;
        }

        public int SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            RaiseMethodCall("SetAllocationSize " + filename);
            return -1;
        }

        public int SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            RaiseMethodCall("SetEndOfFile " + filename);

            cachedDirectoryFileInformation[filename] = new DirectoryFileInformation(false)
            {
                FileName = filename,
                Length = length
            };

            return 0;
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

            long cachedLength = GetCachedLength(filename);

            if (buffer.Length < cachedLength) // only part of file is in the buffer
            {
                // no part of file has been cached before
                if (bufferedFileName != filename)
                {
                    bufferedFileName = filename;
                    bufferedFile = new byte[cachedLength];
                }

                Array.Copy(
                    buffer,
                    0,
                    bufferedFile,
                    (int)offset,
                    buffer.Length);

                bufferedBytes = (int)offset + buffer.Length;

                if (bufferedBytes == cachedLength)
                {
                    EnqueueUploadTask(filename, bufferedFile);
                }
            }
            else
            {
                EnqueueUploadTask(filename, buffer);
            }

            writtenBytes = (uint)buffer.Length;

            return 0;
        }

        #endregion

        Task EnqueueTask(Action action)
        {
            var task = new Task(action);
            fTPSClientTaskQueue.Add(task);
            return task;
        }

        void EnqueueUploadTask(string filename, byte[] buffer)
        {
            EnqueueTask(() =>
            {
                using (var ftpStream = fTPSClient.PutFile(filename))
                {
                    ftpStream.Write(buffer, 0, buffer.Length);
                }
            }).Wait();
        }

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

        static string GetUrl(string filename)
        {
            return filename.Replace('\\', '/');
        }

        DirectoryFileInformation GetDirectoryFileInformation(string parentDirectory, DirectoryListItem directoryListItem)
        {
            var path = parentDirectory + directoryListItem.Name;
            var lastWriteTime = directoryListItem.IsDirectory ?
                directoryListItem.CreationTime :
                GetCachedLastWriteTime(path) ?? directoryListItem.CreationTime;

            return new DirectoryFileInformation(directoryListItem)
            {
                LastAccessTime = lastWriteTime,
                LastWriteTime = lastWriteTime,
                Length = directoryListItem.IsDirectory ? default(long) : GetCachedLength(path),
            };
        }

        DateTime? GetLastWriteTime(string fileName)
        {
            DateTime? fileModificationTime = null;
            EnqueueTask(() => fileModificationTime = fTPSClient.GetFileModificationTime(fileName)).Wait();
            return fileModificationTime;
        }

        DateTime? GetCachedLastWriteTime(string fileName)
        {
            return cachedDirectoryFileInformation.ContainsKey(fileName) ?
                cachedDirectoryFileInformation[fileName].LastWriteTime :
                GetLastWriteTime(fileName);
        }

        long GetLength(string fileName)
        {
            long fileTransferSize = default(long);
            EnqueueTask(() => fileTransferSize = (long)(fTPSClient.GetFileTransferSize(fileName) ?? default(ulong))).Wait();
            return fileTransferSize;
        }

        long GetCachedLength(string fileName)
        {
            return cachedDirectoryFileInformation.ContainsKey(fileName) ?
                cachedDirectoryFileInformation[fileName].Length :
                GetLength(fileName);
        }

        class DirectoryFileInformation : FileInformation
        {
            bool isDirectory;
            internal bool IsDirectory
            {
                get
                {
                    return isDirectory;
                }
                private set
                {
                    isDirectory = value;
                    Attributes = value ?
                        FileAttributes.Directory :
                        FileAttributes.Normal;
                }
            }

            internal DirectoryFileInformation(bool isDirectory)
            {
                CreationTime = DateTime.Now;
                LastAccessTime = DateTime.Now;
                LastWriteTime = DateTime.Now;
                IsDirectory = isDirectory;
            }

            internal DirectoryFileInformation(DirectoryListItem directoryListItem)
            {
                FileName = directoryListItem.Name;
                CreationTime = directoryListItem.CreationTime;
                LastAccessTime = directoryListItem.CreationTime;
                LastWriteTime = directoryListItem.CreationTime;
                IsDirectory = directoryListItem.IsDirectory;
            }
        }
    }
}