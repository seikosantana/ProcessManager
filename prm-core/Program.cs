using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;

namespace prm
{
    static class Program
    {
        static bool isListing = false;
        static bool loopUsage = false;
        static int LastX, LastY;
        static string WorkDir = "";
        enum ConsoleStyle
        {
            Normal, Error, Warning, Success, Input
        }
        static void Prompt(string Message, string sep = " > ", bool endl = false, ConsoleStyle style = ConsoleStyle.Normal)
        {
            switch (style)
            {
                case ConsoleStyle.Input:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case ConsoleStyle.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case ConsoleStyle.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case ConsoleStyle.Success:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case ConsoleStyle.Normal:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                default:
                    break;
            }
            Console.Write(Message + sep);
            Console.ForegroundColor = ConsoleColor.White;
            if (endl) Console.Write(Environment.NewLine);
        }

        static string GetLine()
        {
            return Console.ReadLine();
        }

        static bool loop;
        static void Main(string[] args)
        {
            WorkDir = Environment.CurrentDirectory;
            Console.Title = "Command Line Process Manager 1.1";
            Prompt("Process Manager\r\nCommand Line Task Manager v1.1", "", true);
            Prompt($"On {RuntimeInformation.OSDescription}", "", true, ConsoleStyle.Normal);
            if (args.Length == 0)
            {
                do
                {
                    loop = Root();
                } while (loop);

            }
            else
            {
                string cmds = "";
                for (int i = 0; i < args.Length; i++)
                {
                    cmds += args[i] + " ";
                }
                cmds = cmds.Trim();
                ProcessCommand(cmds);
                
            }
            Console.ResetColor();
        }

        private static void StopUpdate(object sender, ConsoleCancelEventArgs e)
        {
            loopUsage = false;
            Root();
        }



        static bool Root()
        {
            Prompt("\r\nPRM");
            string cmd = GetLine().Trim();
            if (cmd != string.Empty)
                ProcessCommand(cmd);
            return true;
        }

        static void ListProcesses(String namecategory = "")
        {
            isListing = true;
            List<Process> processes = new List<Process>();
            int MaxLength = 0;
            foreach (Process p in Process.GetProcesses()) {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (p.ProcessName.Length > MaxLength)
                        MaxLength = p.ProcessName.Length;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                    if (p.ProcessName.Split(" ")[0].Length > MaxLength)
                        MaxLength = p.ProcessName.Length;
                }
                processes.Add(p);
            }
            
            Prompt($"List of processes, {DateTime.Now.ToShortTimeString()} ", "", true, ConsoleStyle.Warning);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                
                Console.WriteLine($"{Format("| Name ", 26)} | {Format("ID", 6)} | {Format("Description", 35)} |");
                foreach (var process in processes)
                {
                    string desc = "";
                    try
                    {
                        desc = FileVersionInfo.GetVersionInfo(GetProcessPath(process.Id)).FileDescription;
                    }
                    catch
                    {
                        desc = "";
                    }
                    if (string.IsNullOrEmpty(desc))
                        desc = "";
                    if (namecategory == "")
                        Console.WriteLine($"| {Format(process.ProcessName, 24)} | {Format(process.Id, 6)} | {Format(desc, 35)} |");
                    else
                        if (process.ProcessName.ToUpper().Contains(namecategory.ToUpper()) || desc.ToUpper().Contains(namecategory.ToUpper()))
                        Console.WriteLine($"| {Format(process.ProcessName, 24)} | {Format(process.Id, 6)} | {Format(desc, 35)} |");    
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine($"{Format("| Name", 35)}   | {Format("ID", 6)} | {Format("Command", 40)}");
                foreach (var process in processes) {
                    string ProcessName, ProcessArgs;
                    ProcessName = process.ProcessName.Split(" ")[0];
                    ProcessArgs = process.ProcessName.Substring(ProcessName.Length, process.ProcessName.Length - ProcessName.Length);
                    if (namecategory == "")
                    {
                        Console.WriteLine($"| {Format(ProcessName, 35)} | {Format(process.Id, 6)} | {Format(ProcessArgs, 40)}");
                    }
                    else 
                        if (process.ProcessName.ToUpper().Contains(namecategory.ToUpper()))
                            Console.WriteLine($"| {Format(ProcessName, 35)} | {Format(process.Id, 6)} | {Format(ProcessArgs, 40)}");
                }
            }
            isListing = false;
        }

        static string Format(object content, int length)
        {
            string result = content.ToString();
            if (result.Length < length)
            {
                var span = length - result.Length;
                for (int i = 0; i < span; i++)
                {
                    result += " ";
                }
            }
            else
            {
                string temp = "";
                for (int i = 0; i < length - 3; i++)
                {
                    temp += result[i];
                }
                temp += "...";
                return temp;
            }
            return result;
        }

        static void ProcessCommand(string command)
        {
            string[] cmds = command.Split(' ');
            switch (cmds.Length)
            {
                case 1:
                    switch (cmds[0].ToUpper())
                    {
                        case "LIST":
                        case "R":
                        case "L":
                            ListProcesses();
                            break;
                        case "CLS":
                        case "CLR":
                        case "C":
                            Console.Clear();
                            break;
                        case "EXIT":
                        case "QUIT":
                            Console.ResetColor();
                            Environment.Exit(0);
                            break;
                        case "STATUS":
                        case "S":
                        case "USAGE":
                            Prompt("Currently unsupported on .NET Core 3.1.", "", true, ConsoleStyle.Warning);
                            break;
                        case "HELP":
                        case "H":
                            ShowHelp();
                            break;
                        case "CONTR":
                            Process.Start("https://github.com/seikosantana/process-manager");
                            break;
                        case "KILL":
                        case "K":
                        case "LOCATE":
                        case "CMD":
                            Prompt("Unspecified ID or process name. Refer to help for command list.", "", true, ConsoleStyle.Error);
                            break;
                        case "START":
                        case "SUDO":
                            Prompt("Unspecified shell command or file path or program name. Refer to help for command list.", "", true, ConsoleStyle.Error);
                            break;
                        case "STOP":
                            loopUsage = false;
                            break;
                        default:
                            Prompt("Launching as shell command.", "", true, ConsoleStyle.Warning);
                            try
                            {
                                ProcessStartInfo ps = new ProcessStartInfo();
                                ps.UseShellExecute = false;
                                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                                    ps.FileName = "cmd.exe";
                                    ps.Arguments = $"/c {cmds[0]}";
                                }
                                else {
                                    ps.FileName = "/bin/bash";
                                    ps.Arguments = $"-c \"{cmds[0]}\"";
                                }
                                Process p = Process.Start(ps);
                                p.WaitForExit();
                            }
                            catch (Exception ex)
                            {
                                Prompt(ex.Message, "", true, ConsoleStyle.Error);
                            }
                            break;
                    }
                    break;
                case 2:
                    int i;
                    switch (cmds[0].ToUpper())
                    {
                        case "LOCATE":
                        case "CMD":
                            if (int.TryParse(cmds[1], out i))
                            {
                                Process p = Process.GetProcessById(i);
                                LocateProcess(p, cmds[0].ToUpper() == "CMD");
                            }
                            else
                            {
                                Process[] processes = Process.GetProcessesByName(cmds[1]);
                                if (processes.Length == 0)
                                    Prompt($"Process \"{cmds[1]}\" not found", "", true, ConsoleStyle.Error);
                                else
                                    LocateProcess(processes[0], cmds[0].ToUpper() == "CMD");
                            }
                            break;
                        case "LIST":
                        case "L":
                        case "R":
                            ListProcesses(cmds[1]);
                            break;
                        case "KILL":
                        case "K":
                            if (int.TryParse(cmds[1], out i))
                            {
                                Process p = Process.GetProcessById(i);
                                try
                                {
                                    p.Kill();
                                }
                                catch (Exception ex)
                                {
                                    Prompt($"Termination failed. {ex.Message}", "", true, ConsoleStyle.Error);
                                }
                            }
                            else
                            {
                                Process[] processes_ = Process.GetProcessesByName(cmds[1]);
                                if (processes_.Length == 0)
                                {
                                    Prompt($"Cannot find process {cmds[1]}", "", true, ConsoleStyle.Error);
                                }
                                foreach (var p in Process.GetProcessesByName(cmds[1]))
                                {
                                    Prompt($"Termination of process {p.ProcessName}, ID: {p.Id.ToString()}", "", true, ConsoleStyle.Warning);
                                    try
                                    {
                                        p.Kill();
                                    }
                                    catch (Exception ex)
                                    {
                                        Prompt($"Termination failed. {ex.Message}", "", true, ConsoleStyle.Error);
                                    }
                                }
                            }
                            break;
                        case "START":
                        case "SUDO":
                            ProcessStartInfo ps = new ProcessStartInfo();
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                                ps.FileName = cmds[1];
                                ps.UseShellExecute = true;
                                
                                if (cmds[0].ToUpper() == "SUDO")
                                {
                                    ps.Verb = "runas";
                                }
                                else
                                {
                                    ps.Verb = "";
                                }
                                try
                                {
                                    Process p = Process.Start(ps);
                                }
                                catch (Exception ex)
                                {
                                    Prompt($"Launch failed. {ex.Message}", "", true, ConsoleStyle.Error);
                                    Prompt("Launching as shell command.", "", true, ConsoleStyle.Warning);
                                    try
                                    {
                                        ps.FileName = "cmd.exe";
                                        if (cmds[0].ToUpper() == "SUDO")
                                            ps.Arguments = $"/c {Directory.GetDirectoryRoot(WorkDir).Replace("\\", "")}&cd {WorkDir}&{cmds[1]}";
                                        else
                                        {
                                            ps.Arguments = "/c " + cmds[1];
                                            ps.UseShellExecute = false;
                                        }
                                        Process p = Process.Start(ps);
                                    }
                                    catch (Exception ex_)
                                    {
                                        Prompt("Cannot launch as shell command. " + ex_.Message, "", true, ConsoleStyle.Error);
                                    }
                                }
                            }
                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                                ps.FileName = "/bin/bash";
                                ps.UseShellExecute = false;
                                ps.Arguments = $"-c \"{cmds[1]}\"";
                                if (cmds[0].ToUpper() == "SUDO") {
                                    ps.Arguments = $"-c \"sudo {cmds[1]}\"";
                                }
                                Process.Start(ps).WaitForExit();
                            }
                            break;

                        default:
                            ps = new ProcessStartInfo();
                            ps.FileName = cmds[0];
                            ps.UseShellExecute = true;
                            ps.Arguments = cmds[1];
                            try
                            {
                                Process p = Process.Start(ps);
                                //p.WaitForExit();
                            }
                            catch (Exception ex)
                            {
                                Prompt($"Launch failed. {ex.Message}", "", true, ConsoleStyle.Error);
                                Prompt("Launching as shell command.", "", true, ConsoleStyle.Warning);
                                try
                                {
                                    ps.FileName = "cmd.exe";
                                    if (cmds[0].ToUpper() == "SUDO")
                                        ps.Arguments = $"/c {Directory.GetDirectoryRoot(WorkDir).Replace("\\", "")}&cd {WorkDir}&{cmds[1]}";
                                    else
                                    {
                                        ps.Arguments = "/c " + cmds[1];
                                        ps.UseShellExecute = false;
                                    }
                                    Process p = Process.Start(ps);
                                }
                                catch (Exception ex_)
                                {
                                    Prompt("Cannot launch as shell command. " + ex_.Message, "", true, ConsoleStyle.Error);
                                }
                            }
                            break;
                    }
                    break;
                default:
                    switch (cmds[0].ToUpper())
                    {
                        case "START":
                        case "SUDO":
                            ProcessStartInfo ps = new ProcessStartInfo();
                            ps.UseShellExecute = true;
                            ps.WorkingDirectory = Directory.GetCurrentDirectory();
                            string args = "";
                            for (int j = 2; j < cmds.Length; j++)
                            {
                                args += cmds[j] + " ";
                            }
                            if (cmds[0].ToUpper() == "SUDO")
                            {
                                ps.UseShellExecute = true;
                                ps.Verb = "runas";
                            }
                            else
                                ps.Verb = "";
                            args = args.Trim();
                            ps.FileName = cmds[1];
                            ps.Arguments = args;
                            try
                            {
                                Process p = Process.Start(ps);
                            }
                            catch (Exception ex)
                            {
                                Prompt($"Cannot find process. {ex.Message}", "", true, ConsoleStyle.Error);
                                Prompt("Launching as shell command.", "", true, ConsoleStyle.Warning);
                                try
                                {
                                    args = cmds[1] + " " + args;
                                    ps.FileName = "cmd.exe";
                                    ps.WorkingDirectory = Directory.GetCurrentDirectory();
                                    if (cmds[0].ToUpper() == "SUDO")
                                        ps.Arguments = $"/c {Directory.GetDirectoryRoot(WorkDir).Replace("\\", "")}&cd {WorkDir}&{args}";
                                        //ps.Arguments = "/k " + args;
                                    else
                                    {
                                        ps.Arguments = "/c " + args;
                                        ps.UseShellExecute = false;
                                    }
                                    Process p = Process.Start(ps);
                                    if (cmds[0].ToUpper() != "SUDO")
                                        p.WaitForExit();
                                    Root();
                                }
                                catch (Exception ex_)
                                {
                                    Prompt("Cannot launch as shell command. " + ex_.Message, "", true, ConsoleStyle.Error);
                                }
                            }                            
                            break;
                        default:
                            ps = new ProcessStartInfo();
                            ps.UseShellExecute = false;
                            ps.WorkingDirectory = Directory.GetCurrentDirectory();
                            args = "";
                            for (int j = 1; j < cmds.Length; j++)
                            {
                                args += cmds[j] + " ";
                            }
                            
                            ps.Verb = "";
                            args = args.Trim();
                            ps.FileName = cmds[0];
                            ps.Arguments = args;
                            try
                            {
                                Process p = Process.Start(ps);
                            }
                            catch (Exception ex)
                            {
                                Prompt($"Cannot find process. {ex.Message}", "", true, ConsoleStyle.Error);
                                Prompt("Launching as shell command.", "", true, ConsoleStyle.Warning);
                                try
                                {
                                    args = cmds[1] + " " + args;
                                    ps.FileName = "cmd.exe";
                                    ps.WorkingDirectory = Directory.GetCurrentDirectory();
                                    if (cmds[0].ToUpper() == "SUDO")
                                        ps.Arguments = $"/c {Directory.GetDirectoryRoot(WorkDir).Replace("\\", "")}&cd {WorkDir}&{args}";
                                    //ps.Arguments = "/k " + args;
                                    else
                                    {
                                        ps.Arguments = "/c " + args;
                                        ps.UseShellExecute = false;
                                    }
                                    Process p = Process.Start(ps);
                                    Root();
                                }
                                catch (Exception ex_)
                                {
                                    Prompt("Cannot launch as shell command. " + ex_.Message, "", true, ConsoleStyle.Error);
                                }
                            }
                            break;
                    }
                    break;
            }
        }

        private static void ShowHelp()
        {
            Prompt($"AVAILABLE COMMANDS\n", "", true, ConsoleStyle.Warning);
            Prompt("LIST [Keyword]", "", true, ConsoleStyle.Warning);
            Prompt("Lists all running processes which has [Keyword] in its process name or description. [Keyword] is optional.\nAliases: L, R\n", "", true);
            Prompt("KILL [ID or Name]", "", true, ConsoleStyle.Warning);
            Prompt("Terminates all processes with specified ID or specified process name. [ID or Name] is required.\nAlias: K\n", "", true);
            Prompt("START [{Name and Params} or {Shell Command}]", "", true, ConsoleStyle.Warning);
            Prompt("Starts process with specified [Name] as child process and passes [Params] to the new process.\nOR executes the [Shell Command] if specified.\nThe process will share the same console with PRM\n[Name] is required, [Params] are optional.\n", "", true);
            Prompt("SUDO [{Name and Params} or {Shell Command}]", "", true, ConsoleStyle.Warning);
            Prompt("Starts process with specified [Name] as administrator and passes [Params] to the new process.\nOR executes the [Shell Command] with administrator priviledge, if specified.\n`[Name] is required, [Params] are optional\n", "", true);
            Prompt("LOCATE [ID or Name]", "", true, ConsoleStyle.Warning);
            Prompt("Starts File Explorer on the directory where process [ID or Name] is located in the disk.\n[ID or Name] is required.\n", "", true);
            Prompt("CMD [ID or Name]", "", true, ConsoleStyle.Warning);
            Prompt("Starts command prompt and switch to the directory where process [ID or Name] is located in the disk.\n[ID or Name] is required.\n", "", true);
            Prompt("STATUS", "", true, ConsoleStyle.Warning);
            Prompt("Shows resource usage.\nAliases: USAGE, S\n", "", true);
            Prompt("STOP", "", true, ConsoleStyle.Warning);
            Prompt("Stop showing resource usage.\n", "", true);
            Prompt("HELP", "", true, ConsoleStyle.Warning);
            Prompt("Shows this help and available commands.\nAlias: H\n", "", true);
            Prompt("EXIT", "", true, ConsoleStyle.Warning);
            Prompt("Exits PRM.\nAlias: QUIT, HOTKEY: Ctrl + C\n", "", true);
            Prompt("CONTR", "", true, ConsoleStyle.Warning);
            Prompt("Opens github page on https://github.com/seikosantana/process-manager", "", true);
        }
        static void LocateProcess(Process p, bool useCMD = false)
        {
            
            string path = "";
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    path = GetProcessPath(p.Id);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    path = p.MainModule.FileName;
                if (!string.IsNullOrEmpty(path))
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                        if (!useCMD)
                            Process.Start("explorer.exe", $"/select,\"{path}\"");
                        else
                            Process.Start("cmd.exe", $"/k cd \"{Path.GetDirectoryName(path)}\"");
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                        ProcessStartInfo bash = new ProcessStartInfo();
                        bash.CreateNoWindow = false;
                        bash.FileName = "/bin/bash";
                        bash.Arguments = $"-c \"cd //{Path.GetDirectoryName(path)}\"";
                        bash.UseShellExecute = true;
                        Process.Start(bash);
                    }
                }
            }
            catch (Exception ex)
            {
                Prompt($"Process \"{p.ProcessName}\" cannot be found. {ex.Message}", "", true, ConsoleStyle.Error);
            }
            finally
            {
                if (!string.IsNullOrEmpty(path))
                    Prompt($"Locating {p.ProcessName} on {path}", "", true, ConsoleStyle.Success);
                else
                    Prompt($"Cannot locate process \"{p.ProcessName}\"", "", true, ConsoleStyle.Error);
            }
        }

        #region DLLImports
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("psapi.dll")]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static string GetProcessPath(int pid)
        {
            var processHandle = OpenProcess(0x0400 | 0x0010, false, pid);
            if (processHandle == IntPtr.Zero)
                return null;
            const int lengthSb = 4000;
            var sb = new StringBuilder(lengthSb);
            string result = null;
            if (GetModuleFileNameEx(processHandle, IntPtr.Zero, sb, lengthSb) > 0)
                result = sb.ToString();
            CloseHandle(processHandle);
            return result;
        }
        #endregion
    }
}

