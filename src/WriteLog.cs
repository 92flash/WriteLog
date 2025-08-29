using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using static WriteLog.Logging;

namespace WriteLog
{
    [Cmdlet(VerbsCommunications.Write,"Log")]
    public class WriteLogCommand : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "This message will be written to the logfile and optionally to the shell if OutHost is set to true"
        )]
        public string Message;

        [Parameter(
            Position = 1,
            HelpMessage = "Choose what type of message is logged for detailed information. The default type is Information"
        )]
        [ValidateSet("Information", "Warning", "Attention", "Error", "Success", "Verbose", "Debug", "Fatal", IgnoreCase = true)]
        public LogType Type = LogType.Information;

        [Parameter(
            Position = 2,
            HelpMessage = "Set the path for the logfile with the file and extention at the end of the path"
        )]
        public string LogPath
    	{ 
            get 
            {
                return Environment.GetEnvironmentVariable("LogPath");
            } 
            set 
            {
                Environment.SetEnvironmentVariable("LogPath", value);
            }
        }

        [Parameter(
            HelpMessage = "Together with writing the Message to the logfile, it also writes the Message to the shell"
        )]
        public SwitchParameter OutHost;

        [Parameter(
            HelpMessage = "When LoggingMode is set to KeepAlive, it will keep the logfile open until manually closed. This is great for large loggingentries"
        )]
        [ValidateSet("Default", "KeepAlive", "Close", IgnoreCase = true)]
        public string LoggingMode
    	{ 
            get 
            {
                return Environment.GetEnvironmentVariable("LoggingMode");
            } 
            set 
            {
                Environment.SetEnvironmentVariable("LoggingMode", value);
            }
        }


        [Parameter(
            HelpMessage = "Hide the folder where the logfile is written in"
        )]
        public SwitchParameter HideLogDir;

        [Parameter(
            HelpMessage = "Write only the message as logline and forgo the date/time and type"
        )]
        public SwitchParameter NoDetails;

        [Parameter(
            HelpMessage = "Empties the logfile and adds the new logline as content"
        )]
        public SwitchParameter ReplaceLog;

        [Parameter(
            HelpMessage = "Terminate the script"
        )]
        public SwitchParameter Terminate;

        [Parameter(
            HelpMessage = "Terminate the script after x amount of seconds"
        )]
        public int TerminateIn = 0;

        // Create a static field to hold the Logging instance
        private static Logging log;

        public WriteLogCommand()
        {

        }

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {

            // Throw error if LogPath is not set and does not contain a file extention
            if (string.IsNullOrEmpty(LogPath))
            {
                NullReferenceException ex = new("LogPath is not set. Please set the LogPath as parameter or as a environment variable ($env:LogPath)");
                ThrowTerminatingError(new ErrorRecord(ex, "LogPathNotSet", ErrorCategory.InvalidArgument, null));
            }

            // Force the path to contain an extention and that it ends with log or txt
            if (Path.GetExtension(LogPath) == string.Empty && !LogPath.EndsWith(".log") && !LogPath.EndsWith(".txt"))
            {
                ArgumentException ex = new("LogPath is not valid. Please set the LogPath with file extention .log or .txt at the end of the path");
                ThrowTerminatingError(new ErrorRecord(ex, "LogPathNotValid", ErrorCategory.InvalidArgument, null));
            }

            // Set OutHost to true if the environment variable is set and OutHost is not already true
            if (OutHost == false && Environment.GetEnvironmentVariable("OutHost") == "True")
            {
                OutHost = true;
            }

            // If no LoggingMode has been set, set it to default
            LoggingMode ??= "Default";

            // Empty the log file if ReplaceLog is set
            if (ReplaceLog)
            {
                log?.ClearLog();
            }

            // Skip initialization if the module has already been run once in this session
            if (log != null && File.Exists(LogPath))
            {
                return;
            }

            // Create the logging object
            log = new Logging(LogPath);
            log.Open();

            // Hide the log directory if HideLogDir is set
            if (HideLogDir)
            {
                LogPath = log.HideLogDir() ?? LogPath;
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LogPath")))
                {
                    Environment.SetEnvironmentVariable("LogPath", LogPath);
                }
            }

            // Date, computername and username that'll be added to the logfile
            string date = $"Date:".PadRight(16, ' ') + $"{DateTime.Now:dd-MM-yyyy}";
            string computerName = $"Computername:".PadRight(16, ' ') + $"{Environment.MachineName}";
            string username = $"Username:".PadRight(16, ' ') + $"{Environment.UserName}";

            // Only append the extra details if the details haven't been added to the file
            bool getMatchDate = log.FindMatch(date, true);
            bool getMatchComputerName = log.FindMatch(computerName, true);
            bool getMatchUserName = log.FindMatch(username, true);
            if (!getMatchDate || !getMatchComputerName || !getMatchUserName)
            {
                // Add empty line if the log file is not empty to seperate different logging times
                StringBuilder logDetails = new();
                logDetails.Append(!log.IsFileEmpty() ? Environment.NewLine : null);

                // Build the string to add the details to the logfile
                logDetails.Append($"{date}{Environment.NewLine}");
                logDetails.Append($"{computerName}{Environment.NewLine}");
                logDetails.Append($"{username}{Environment.NewLine}");

                // Append the logstring to the logfile
                Log(logDetails.ToString(), LogType.Information, true);
            }
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {

            // Open the logfile
            log.Open();

            // Append exit message if log type is fatal
            if (Type == LogType.Fatal)
            {
                Message += " --> Exited the script";
            }

            // Only log the message if Debug- and Verbosepreference is NOT set to SilentlyContinue or Ignore
            if (!(GetVariableValue("DebugPreference").ToString() == "SilentlyContinue" || GetVariableValue("DebugPreference").ToString() == "Ignore") || !((GetVariableValue("VerbosePreference").ToString() == "SilentlyContinue" || GetVariableValue("VerbosePreference").ToString() == "Ignore") && Type == LogType.Verbose))
            {
                Log(Message, Type, NoDetails);
            }
            
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {

            // Close the logfile only if LoggingMode is set to Default or Close
            if (LoggingMode == "Default" || LoggingMode == "Close")
            {
                // When closing the log, differentiate between when the logging is used
                if (LoggingMode == "Close")
                {
                    Log("", LogType.Information, true);
                }

                log.Close();
            }

            // Write the message to the PowerShell host
            if (OutHost == true)
            {
                Powershell ps = new(this);
                ps.WriteShellMessage(Message, Type);
            }

            // Terminate the script after 7 seconds if the log type is fatal
            if (Type == LogType.Fatal && (Terminate == false || TerminateIn == 0))
            {
                TerminateIn = 7;
            }

            // Terminate the script (with pause) when desired
            if (Terminate || TerminateIn > 0)
            {
                // Pause the script with the message for x amount of seconds if TerminateIn is greater than 0
                if (TerminateIn > 0)
                {
                    string terminateMessage = $"The script will terminate in [seconds] seconds";
                    Powershell.Pause(TerminateIn, terminateMessage, true);
                }

                // Terminate the script
                Environment.Exit(0);
            }
            
        }
    }
}
