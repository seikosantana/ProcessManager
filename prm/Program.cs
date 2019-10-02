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
        static char GetChar(bool hide = false)
        {
            var key = Console.ReadKey(hide);
            return key.KeyChar;
        }

        static char GetChar(char[] among, bool hide = false)
        {
            char result = ' ';
            while (true)
            {
                var ch = Console.ReadKey(!hide).KeyChar;
                foreach (var chars in among)
                {
                    if (char.ToUpper(ch) == char.ToUpper(chars))
                    {
                        Console.Write(ch);
                        result = ch;
                        break;
                    }
                }
                if (result != ' ') break;
            }
            return result;
        }
        static bool loop;
        static void Main(string[] args)
        {
            Console.Title = "Command Line Process Manager 1.0";
            Prompt("Process Manager\r\nCommand Line Task Manager v1.0, Seiko Santana", "", true);
            if (args.Length == 0)
            {
                do
                {
                    loop = root();
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

        static bool root()
        {
            Prompt("\r\nPRM");
            string cmd = GetLine();
            ProcessCommand(cmd);
            return true;
        }
        
        static void ListProcesses(String namecategory = "")
        {
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
                            Console.Clear();
                            break;
                        case "EXIT":
                        case "QUIT":
                            Environment.Exit(0);
                            break;
                        default:
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
                            ps.UseShellExecute = false;
                            if (cmds[0].ToUpper() == "SUDO")
                            {
                                ps.Verb = "runas";
                                ps.UseShellExecute = true;
                            }
                            else
                                ps.Verb = "";
                            try
                            {
                                Process.Start(ps);
                            }
                            catch (Exception ex)
                            {
                                Prompt($"Launch failed. {ex.Message}", "", true, ConsoleStyle.Error);
                                Prompt("Launching as cmd command.", "", true, ConsoleStyle.Warning);
                                try
                                {
                                    ps.FileName = "cmd.exe";
                                    if (cmds[0].ToUpper() == "SUDO")
                                        ps.Arguments = "/k " + cmds[1];
                                    else
                                    {
                                        ps.Arguments = "/c " + cmds[1];
                                        ps.UseShellExecute = false;
                                        ps.RedirectStandardOutput = true;
                                    }
                                    Process p = Process.Start(ps);
                                    if (cmds[0].ToUpper() != "SUDO")
                                        p.WaitForExit();
                                    root();
                                }
                                catch (Exception ex_)
                                {
                                    Prompt("Cannot launch as cmd command. " + ex_.Message, "", true, ConsoleStyle.Error);
                                }
                            }
                            break;

                        default:
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
                                args += cmds[j];
                            }
                            if (cmds[0].ToUpper() == "SUDO")
                                ps.Verb = "runas";
                            else
                                ps.Verb = "";
                            args = args.Trim();
                            ps.FileName = cmds[1];
                            ps.Arguments = args;
                            try
                            {
                                Process.Start(ps);
                            }
                            catch (Exception ex)
                            {
                                Prompt($"Launch failed. {ex.Message}", "", true, ConsoleStyle.Error);
                                Prompt("Launching as cmd command.", "", true, ConsoleStyle.Warning);
                                try
                                {
                                    args = cmds[1] + " " + args;
                                    ps.FileName = "cmd.exe";
                                    ps.WorkingDirectory = Directory.GetCurrentDirectory();
                                    if (cmds[0].ToUpper() == "SUDO")
                                        ps.Arguments = "/k " + args;
                                    else
                                    {
                                        ps.Arguments = "/c " + args;
                                        ps.UseShellExecute = false;
                                    }
                                    Process p = Process.Start(ps);
                                    if (cmds[0].ToUpper() != "SUDO")
                                        p.WaitForExit();
                                    root();
                                }
                                catch (Exception ex_)
                                {
                                    Prompt("Cannot launch as cmd command. " + ex_.Message, "", true, ConsoleStyle.Error);
                                }
                            }

                            break;
                        default:
                            break;
                    }
                    break;
            }
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

