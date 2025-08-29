using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace WriteLog
{

    public class Logging
    {

        public enum LogType
        {
            Information,
            Warning,
            Attention,
            Error,
            Success,
            Verbose,
            Debug,
            Fatal
        }

        public static string LogPath { get; set; }
        public static bool IsOpen { get; private set; }
        private static FileStream logFile;

        public Logging(string logPath, bool isOpen = false)
        {
            LogPath = logPath;
            IsOpen = isOpen;
        }

        // Write a message to a logfile with the type of message added to it
        public static bool Log(string message, LogType type, bool noDetails = false)
        {

            try
            {
                if (logFile == null)
                {
                    throw new InvalidOperationException("Log file is not open. Call OpenLog() before logging.");
                }

                string logEntry = noDetails ? message : $"{DateTime.Now:dd-MM-yyyy HH:mm:ss} - " + $"{type}:".PadRight(14, ' ') + message;
                var bytes = System.Text.Encoding.UTF8.GetBytes(logEntry + Environment.NewLine);
                logFile.Write(bytes, 0, bytes.Length);
                logFile.Flush();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        // Function to open the logfile
        public void Open()
        {

            // Exit the method if the file is already open and already exists
            if (IsOpen && File.Exists(LogPath)) return;

            // Create the logfile if it doesn't already exist yet
            if (!File.Exists(LogPath))
            {
                try
                {
                    string directory = Path.GetDirectoryName(LogPath);
                    if (directory != null && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    logFile = File.Create(LogPath);
                    logFile.Flush();
                    logFile.Dispose();
                    logFile.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating log file: {ex.Message}");
                    return;
                }
            }

            // Open the logfile
            try
            {
                logFile = new FileStream(LogPath, FileMode.Append, FileAccess.Write);
                IsOpen = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening log file: {ex.Message}");
            }

        }

        // Static method that will also be exposed in Powershell to open the log
        public static void OpenLog(string logPath = null)
        {
            logPath ??= LogPath;
            new Logging(logPath).Open();
        }

        // Close the logfile
        public bool Close()
        {
            if (IsOpen == false) return true;
            
            try
            {
                logFile?.Close();
                IsOpen = false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        // Static method that will also be exposed in Powershell to close the log
        public static bool CloseLog()
        {
            return new Logging(LogPath).Close();
        }

        // Find the first match in the logfile
        public bool FindMatch(string match, bool reverse = false)
        {
            string[] lines = reverse ? File.ReadAllLines(LogPath).Reverse().ToArray() : File.ReadAllLines(LogPath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(match))
                {
                    return true;
                }
            }
            return false;
        }

        // Remove all contents from the logfile
        public void ClearLog()
        {
            try
            {
                if (File.Exists(LogPath))
                {
                    logFile?.Close();
                    File.WriteAllText(LogPath, string.Empty);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error clearing log file: {ex.Message}");
            }
        }

        // Hide the last folder in the logpath
        public string HideLogDir()
        {
            try
            {
                var directory = Path.GetDirectoryName(LogPath);
                if (directory != null && Directory.Exists(directory))
                {
                    var dirInfo = new DirectoryInfo(directory);
                    dirInfo.Attributes |= FileAttributes.Hidden;

                    // Make sure hiding the directory on Linux and MacOS also works
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        int i = directory.LastIndexOfAny(new char[] { '\\', '/' });
                        if (i != -1 && i < directory.Length - 1)
                        {
                            string hiddenPath = directory.Insert(i + 1, ".");
                            Directory.Move(directory, hiddenPath);

                            string hiddenFilePath = LogPath.Insert(i + 1, ".");
                            ShellMessage.Write($"Log path has been hidden and changed location to '{hiddenFilePath}'. Please use this path from now on", Color.Colors.Yellow);
                            OpenLog(hiddenFilePath);
                            return hiddenFilePath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error hiding log directory: {ex.Message}");
            }

            return "";
        }

        // Test if the logfile is empty
        public bool IsFileEmpty()
        {
            try
            {
                return new FileInfo(LogPath).Length == 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking if log file is empty: {ex.Message}");
            }
        }

    }

}