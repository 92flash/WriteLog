using System;
using System.Management.Automation;
using System.Threading;
using static WriteLog.Color;
using static WriteLog.Logging;

namespace WriteLog
{
    public class Powershell
    {
        private readonly PSCmdlet moduleContext;

        public Powershell(PSCmdlet context = null)
        {
            moduleContext = context;
        }

        // Write a message to the shell
        public void WriteShellMessage(string message, LogType type)
        {

            // Depending on what the LogType is, write the message to the shell differently
            Colors color;
            string actionPreference;
            switch (type)
            {
                case LogType.Information:
                    actionPreference = "InformationActionPreference";
                    color = Colors.Cyan;
                    break;
                case LogType.Warning:
                    moduleContext?.WriteWarning(message);
                    return;
                case LogType.Attention:
                    actionPreference = "AttentionPreference";
                    color = Colors.Yellow;
                    break;
                case LogType.Error:
                    var errorRecord = new ErrorRecord(new Exception(message), "Error", ErrorCategory.NotSpecified, null);
                    moduleContext?.WriteError(errorRecord);
                    return;
                case LogType.Fatal:
                    var fatalErrorRecord = new ErrorRecord(new Exception(message), "FatalError", ErrorCategory.OperationStopped, null);
                    moduleContext?.WriteError(fatalErrorRecord);
                    return;
                case LogType.Success:
                    actionPreference = "SuccessPreference";
                    color = Colors.Green;
                    break;
                case LogType.Verbose:
                    moduleContext?.WriteVerbose(message);
                    return;
                case LogType.Debug:
                    moduleContext?.WriteDebug(message);
                    return;

                default:
                    actionPreference = "ActionPreference";
                    color = Colors.White;
                    break;
            }

            // Add the log type to the message
            message = type.ToString() + ": " + message;

            // Simulate the preference actions of Powershell
            ExecutePreferenceAction(actionPreference, message);

            // Write the message to shell, except if the VariablePreference is set to SilentlyContinue or Ignore
            string psVariablePreference = moduleContext?.GetVariableValue(actionPreference)?.ToString() ?? "Continue";
            if (!(psVariablePreference == "SilentlyContinue" || psVariablePreference == "Ignore"))
            {
                ShellMessage.Write(message, color, context: moduleContext);
            }
        }

        // Pause the script for x amount of seconds
        public static void Pause(int seconds, string message = null, bool newLine = false)
        {
            // Add a blank line to the console
            if (newLine)
            {
                Console.WriteLine();
            }

            // Add a message to the shell when supplied
            if (!string.IsNullOrEmpty(message))
            {
                DateTime timeout = DateTime.Now.AddSeconds(seconds);
                while (DateTime.Now < timeout)
                {
                    int timeDifference = Math.Max(0, (int)Math.Ceiling((timeout - DateTime.Now).TotalSeconds));
                    string displayMessage = message.Replace("[seconds]", timeDifference.ToString());
                    Console.Write($"\r{displayMessage}");
                    Thread.Sleep(100);
                }
                return;
            }

            // Pause the script for x amount of seconds
            Thread.Sleep(seconds * 1000);
        }

        // Simulate the PreferenceAction of Powershell
        private bool ExecutePreferenceAction(string actionPreference, string message)
        {
            string preference = moduleContext?.GetVariableValue(actionPreference)?.ToString() ?? "Continue";
            switch (preference)
            {
                case "Break":
                    if (null == moduleContext?.GetVariableValue("PSDebugContext"))
                    {
                        object previousDebugValue = moduleContext?.GetVariableValue("DebugPreference");
                        new Thread(() =>
                        {
                            while (moduleContext?.GetVariableValue("PSDebugContext") != null)
                            {
                                Thread.Sleep(100);
                            }
                            moduleContext?.SessionState.PSVariable.Set("DebugPreference", previousDebugValue);
                        }).Start();
                        moduleContext?.SessionState.PSVariable.Set("DebugPreference", "Break");
                        moduleContext?.WriteDebug("");
                    }
                    return true;
                case "Stop":
                    moduleContext?.WriteError(new ErrorRecord(new HaltCommandException("The running command stopped because the preference variable " + '"' + actionPreference + '"' + $" or common parameter is set to Stop: {message}"), "Stop", ErrorCategory.OperationStopped, null));
                    Environment.Exit(0);
                    return true;
                case "Inquire":
                    switch (ReadInquire())
                    {
                        case "H":
                            moduleContext?.WriteError(new ErrorRecord(new Exception("The running command stopped because the user selected the Stop option."), "Stop", ErrorCategory.OperationStopped, null));
                            Environment.Exit(0);
                            break;
                        case "S":
                            Environment.Exit(0);
                            break;
                        default: break;
                    }
                    return true;
                default:
                    return false;
            }
        }

        // Method to simulate the inquire of Powershell
        private string ReadInquire()
        {
            ShellMessage.Write("\nConfirm", Colors.White, context: moduleContext);
            ShellMessage.Write("Continue with this operation?", context: moduleContext);
            string continueOperation;
            do
            {
                ShellMessage.Write("[Y] Yes  ", Colors.Yellow, false, context: moduleContext);
                ShellMessage.Write("[A] Yes to All  [H] Halt Command  [S] Suspend  ", Colors.White, false, context: moduleContext);
                ShellMessage.Write("[?] Help (default is \"Y\"): ", newLine: false, context: moduleContext);
                continueOperation = Console.ReadLine().ToUpper();
                if (string.IsNullOrEmpty(continueOperation))
                {
                    continueOperation = "Y";
                }
                else if (continueOperation == "?")
                {
                    Console.WriteLine("Y - Continue with only the next step of the operation.\nA - Continue with all the steps of the operation.\nH - Stop this command.\nS - Pause the current pipeline and return to the command prompt. Type \"exit\" to resume the pipeline.");
                }
            } while (continueOperation != "Y" && continueOperation != "A" && continueOperation != "H" && continueOperation != "S");
            return continueOperation;
        }
    }
}