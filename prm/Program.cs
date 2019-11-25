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
            //Console.TreatControlCAsInput = true;
            Console.CancelKeyPress += StopUpdate;
            Console.Title = "Command Line Process Manager 1.1";
            Prompt("Process Manager\r\nCommand Line Task Manager v1.1", "", true);
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
            Process[] processes = Process.GetProcesses();
            int maxlength = 0;
            foreach (var process in processes)
            {
                if (process.ProcessName.Length > maxlength) maxlength = process.ProcessName.Length;
            }
            Prompt($"List of processes, {DateTime.Now.ToShortTimeString()} ", "", true, ConsoleStyle.Warning);
            Console.WriteLine($"{Format("| Name", maxlength + 4)} | {Format("ID", 6)} | {Format("Description", 35)} |");
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
                    Console.WriteLine($"| {Format(process.ProcessName, maxlength + 2)} | {Format(process.Id, 6)} | {Format(desc, 35)} |");
                else
                    if (process.ProcessName.ToUpper().Contains(namecategory.ToUpper()) || desc.ToUpper().Contains(namecategory.ToUpper()))
                    Console.WriteLine($"| {Format(process.ProcessName, maxlength + 2)} | {Format(process.Id, 6)} | {Format(desc, 35)} |");
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
                            Environment.Exit(0);
                            break;
                        case "STATUS":
                        case "S":
                        case "USAGE":
                            Task.Run(GetStatus);
                            
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
                            Prompt("Launching as shell command", "", true, ConsoleStyle.Warning);
                            try
                            {
                                ProcessStartInfo ps = new ProcessStartInfo();
                                ps.FileName = "cmd.exe";
                                ps.Arguments = $"/c {cmds[0]}";
                                ps.UseShellExecute = false;
                                Process p = Process.Start(ps);
                                p.WaitForExit();
                            }
                            catch (Exception)
                            {
                            
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
                            ps.FileName = cmds[1];
                            ps.UseShellExecute = true;
                            
                            if (cmds[0].ToUpper() == "SUDO")
                            {
                                ps.Verb = "runas";
                                ps.UseShellExecute = true;
                            }
                            else
                            {
                                ps.Verb = "";
                                ps.UseShellExecute = false;
                            }
                            try
                            {
                                Process p = Process.Start(ps);
                                if (cmds[0].ToUpper() == "START")
                                    p.WaitForExit();
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
                                    if (cmds[0].ToUpper() != "SUDO")
                                        p.WaitForExit();
                                }
                                catch (Exception ex_)
                                {
                                    Prompt("Cannot launch as shell command. " + ex_.Message, "", true, ConsoleStyle.Error);
                                }
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
                                p.WaitForExit();
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
                                    if (cmds[0].ToUpper() != "SUDO")
                                        p.WaitForExit();
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
                            ps.UseShellExecute = false;
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
                                if (cmds[0].ToUpper() != "SUDO");
                                    p.WaitForExit();
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
                                if (cmds[0].ToUpper() != "SUDO");
                                    p.WaitForExit();
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
            Prompt("Terminates all processes with specified ID or specified process name. [ID or Name] is required.\nAlias: H\n", "", true);
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
            Prompt("Stop showing resource usage.\nHotkey: Ctrl + C\n", "", true);
            Prompt("HELP", "", true, ConsoleStyle.Warning);
            Prompt("Shows this help and available commands.\nAlias: H\n", "", true);
            Prompt("CONTR", "", true, ConsoleStyle.Warning);
            Prompt("Opens github page on https://github.com/seikosantana/process-manager", "", true);
        }

        private static float GetCPUTime(PerformanceCounter Counter)
        {
            Counter.CategoryName = "Processor";
            Counter.CounterName = "% Processor Time";
            Counter.InstanceName = "_Total";
            float res = Counter.NextValue();
            Thread.Sleep(200);
            return Counter.NextValue();
        }

        private static float GetRAMFree(PerformanceCounter Counter)
        {
            Counter.CategoryName = "Memory";
            Counter.CounterName = "Available MBytes";
            Counter.InstanceName = "";
            float res = Counter.NextValue();
            Thread.Sleep(200);
            return Counter.NextValue();
        }

        private static float GetRAMCommited(PerformanceCounter Counter)
        {
            Counter.CategoryName = "Memory";
            Counter.CounterName = "% Committed Bytes In Use";
            Counter.InstanceName = "";
            float res = Counter.NextValue();
            Thread.Sleep(200);
            return Counter.NextValue();
        }

        private static float GetDiskActivity(PerformanceCounter Counter)
        {
            Counter.CategoryName = "PhysicalDisk";
            Counter.CounterName = "% Disk Time";
            Counter.InstanceName = "_Total";
            float res = Counter.NextValue();
            Thread.Sleep(200);
            return Counter.NextValue();
        }

        private static float GetNetworkActivity(PerformanceCounter Counter)
        {
            Counter.CategoryName = "Network Interface";
            Counter.CounterName = "Bytes Total/sec";
            PerformanceCounterCategory category = new PerformanceCounterCategory(Counter.CategoryName);
            Counter.InstanceName = category.GetInstanceNames()[1];
            float res = Counter.NextValue();
            Thread.Sleep(200);
            return Counter.NextValue();

        }

        private static void GetStatus()
        {
            PerformanceCounter counter = new PerformanceCounter();
            loopUsage = true;
            while (loopUsage)
            {
                if (!isListing)
                {
                    string CPUTime = $"{Format("CPU Time", 18)}: {GetCPUTime(counter):0}%";
                    string RAMInUse = $"{Format("RAM in Use", 18)}: {GetRAMCommited(counter):0}%";
                    string RAMFree = $"{Format("Available", 18)}: {GetRAMFree(counter):0} MB";
                    string DiskAct = $"{Format("Disk Time", 18)}: {GetDiskActivity(counter):0}%";
                    string NetworkAct = $"{Format("Network Activity", 18)}: {GetNetworkActivity(counter):0} bytes";
                    LastX = Console.CursorLeft;
                    LastY = Console.CursorTop;
                    Console.CursorLeft = Console.WindowWidth - 30;
                    Prompt(Format(CPUTime, 29), "", true);
                    Console.CursorLeft = Console.WindowWidth - 30;
                    Prompt(Format(RAMInUse, 29), "", true);
                    Console.CursorLeft = Console.WindowWidth - 30;
                    Prompt(Format(RAMFree, 29), "", true);
                    Console.CursorLeft = Console.WindowWidth - 30;
                    Prompt(Format(DiskAct, 29), "", true);
                    Console.CursorLeft = Console.WindowWidth - 30;
                    Prompt(Format(NetworkAct, 29), "", true);
                    Console.CursorLeft = LastX;
                    Console.CursorTop = LastY;
                }
                else
                    continue;
            }
            counter.Dispose();
        }

        static void LocateProcess(Process p, bool useCMD = false)
        {
            string path = "";
            try
            {
                path = GetProcessPath(p.Id);
                if (!string.IsNullOrEmpty(path))
                {
                    if (!useCMD)
                        Process.Start("explorer.exe", $"/select,\"{path}\"");
                    else
                        Process.Start("cmd.exe", $"/k cd \"{Path.GetDirectoryName(path)}\"");
                }
            }
            catch
            {
                Prompt($"Process \"{p.ProcessName}\" cannot be found.", "", true, ConsoleStyle.Error);
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

