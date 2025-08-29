# Introduction

Write-Log is a performent Powershell module written in C#. This module has as primary function to create a logfile and write loglines to it. A logline would look something like:

`28-08-2025 23:17:41 - Information:  This is the message written to the logfile`

Although the module has as primary function to write to the log, there are some nice-to-have functions that are added to this module that are asociated with actions that need to be done while or after logging.

# Syntax

Here is a basic syntax of how the command would look like in the shell

``` powershell
Write-Log "My message" -Type Information -LogPath "C:\My\Path\File.log
```

If the message also needs to appear on the Powershell console, this can be added with the `-OutHost` parameter:

``` powershell
Write-Log "My message" -Type Information -LogPath "C:\My\Path\File.log -OutHost
```

## Parameters

- **Message**
    - **Purpose**: the message that is written to the log and optionally to the shell
    - **Value**: *string*
- **Type**
    - **Purpose**: defines what type of logline is added to the logfile. This will add more information to what was happening at the time of the logmessage
    - **Value**: *string* - "Information", "Warning", "Attention", "Error", "Success", "Verbose", "Debug", "Fatal"
    - **Default value**: "Information"
- **LogPath**
    - **Purpose**: keeps the LogPath consistent for all the logmessages and won't ask again for inputting the LogPath
    - **Value**: *string* - a valid path with a filename that contains a .log or .txt extention
- **OutHost**
    - **Purpose**: will show the logmessage also on the console screen
    - **Value**: *switch* - only need to call it, doesn't have a value
- **LoggingMode**
    - **Purpose**: when LoggingMode is set to KeepAlive, it will keep the logfile open until manually closed. This is great for large loggingentries because it doesn't need to open the logfile every time a logline is written. Do keep in mind that you need to close the file by setting the LoggingMode to "Close" when logging is finished
    - **Value**: *string* - "Default", "KeepAlive", "Close"
    - **Default value**: "Default"
- **HideLogDir**
    - **Purpose**: hide the folder where the logfile is written in
    - **Value**: *switch* - only need to call it, doesn't have a value
- **NoDetails**
    - **Purpose**: doesn't add the date/time and type information to the logline. The date, computername and username will always be logged, regardless of this setting
    - **Value**: *switch* - only need to call it, doesn't have a value
- **ReplaceLog**
    - **Purpose**: empties the logfile and add the logline to a clean file
    - **Value**: *switch* - only need to call it, doesn't have a value
- **Terminate**
    - **Purpose**: terminate the script after logging
    - **Value**: *switch* - only need to call it, doesn't have a value
- **TerminateIn**
    - **Purpose**: terminate the script after logging in x amount of seconds
    - **Value**: *int* - integer

## Environmental variables

- **LogPath**
    - **Purpose**: keeps the LogPath consistent for all the logmessages and won't ask again for inputting the LogPath
    - **Value**: *string* - a valid path with a filename that contains a .log or .txt extention
- **OutHost**
    - **Purpose**: will show the logmessage also on the console screen
    - **Value**: *boolean* - true or false
- **LoggingMode**
    - **Purpose**: when LoggingMode is set to KeepAlive, it will keep the logfile open until manually closed. This is great for large loggingentries because it doesn't need to open the logfile every time a logline is written. Do keep in mind that you need to close the file by setting the LoggingMode to "Close" when logging is finished
    - **Value**: *string* - "Default", "KeepAlive", "Close"

### Preference variables

Preference variables are by default available in Powershell. They allow to alter the flow of information in the module. This is an extra way to show or hide the information that's shown in the shell.

The preference variables are only controlling the information written to the shell and not to the logs, EXCEPT for verbose and debug. This is because, if verbose or debug is added to a script, it's usually only for development purposes and you don't want every verbose or debugline in the log.

Every preference variable should act the same. See more about preference variables in the [Microsoft documentation](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_preference_variables?view=powershell-7.5).

There is no preference variable for *Fatal* because it exits the script immediately.

<strong style="color: red;">If `-OutHost` is not used while a preference variable is used, the preference variable doesn't have any effect.</strong>

#### Default in Powershell

- DebugPreference
- ErrorActionPreference
- VerbosePreference
- WarningPreference

#### Specific to this module

- AttentionPreference
- InformationActionPreference
- SuccessPreference

# Usage

1. Download the Write-Log [module](src/bin/Debug/netstandard2.0/WriteLog.dll)
2. Open Powershell
3. Import the module: `Import-Module $HOME\Downloads\WriteLog.dll`

<strong style="color: red;">Important!</strong> The module needs to be imported every time Powershell opens again. To make this more permanent, consider adding step three to your Powershell profile:
- **For all users**: $PSHOME\Profile.ps1
- **For your current user**: $HOME\Documents\PowerShell\Profile.ps1

Go to [Microsoft's documentation](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_profiles?view=powershell-7.5&viewFallbackFrom=powershell-7) for more information.

# Contribution

If you have an idea, issue or want to improve the code, please create a pull request and I will look if I can add this.