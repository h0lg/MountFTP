using System;
using Args;

namespace Forge.MountFTP.CommandLine
{
    class Program
    {
        static object writeToken = new { };

        static void Main(string[] args)
        {
            var drive = new Drive(Configuration.Configure<Options>().CreateAndBind(args));
            drive.FtpCommand += new LogEventHandler(OnFtpCommand);
            drive.FtpServerReply += new LogEventHandler(OnFtpServerReply);
            drive.FtpClientMethodCall += new LogEventHandler(OnFtpClientMethodCall);
            drive.FtpClientDebug += new LogEventHandler(OnFtpClientDebug);

            Console.WriteLine(drive.Mount());
            Console.ReadKey();
        }

        static void WriteColoredLine(string message, ConsoleColor foreGroundColor)
        {
            lock (writeToken)
            {
                Console.ForegroundColor = foreGroundColor;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        static void OnFtpCommand(object sender, LogEventArgs args)
        {
            WriteColoredLine(args.Message, ConsoleColor.Red);
        }

        static void OnFtpServerReply(object sender, LogEventArgs args)
        {
            WriteColoredLine(args.Message, ConsoleColor.DarkRed);
        }

        static void OnFtpClientMethodCall(object sender, LogEventArgs args)
        {
            WriteColoredLine(args.Message, ConsoleColor.Yellow);
        }

        static void OnFtpClientDebug(object sender, LogEventArgs args)
        {
            WriteColoredLine(args.Message, ConsoleColor.DarkYellow);
        }
    }
}