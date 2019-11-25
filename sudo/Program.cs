using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
namespace sudo
{
    class Program
    {
        static void Main(string[] args)
        {
            string strargs = string.Join(" ", args);
            if (strargs.Trim() != String.Empty)
            {
                strargs = "sudo " + strargs;
                string StartupPath = Application.StartupPath;
                ProcessStartInfo startInfo = new ProcessStartInfo(StartupPath + "\\prm.exe", strargs);
                startInfo.UseShellExecute = false;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                Process PRM = Process.Start(startInfo);
                PRM.WaitForExit();
            }
        }
    }
}
