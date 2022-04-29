using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;

namespace RemoteConnect
{
    class Program
    {

        static void Main(string[] args)
        {
            var filePath = ConfigurationManager.AppSettings["download"].ToString();
            var filaNamePattern = "Pub-RDPproxy_*.rdp";

            var remoteFile = Directory.GetFiles(filePath, filaNamePattern).OrderByDescending(f => new FileInfo(f).LastWriteTime).First();

            if (String.IsNullOrEmpty(remoteFile))
            {
                Console.WriteLine("No rdp file is found");
                return;
            }

            string str = File.ReadAllText(remoteFile);
            str = str.Replace("audiocapturemode:i:0", "audiocapturemode:i:1");
            str = str.Replace("redirectprinters:i:1", "redirectprinters:i:0"); 
            str += Environment.NewLine + "selectedmonitors:s:" + ConfigurationManager.AppSettings["selectedmonitors"].ToString();
            str += Environment.NewLine + "camerastoredirect:s:*";
            str += Environment.NewLine + "encode redirected video capture:i:1";
            str += Environment.NewLine + "redirected video capture encoding quality:i:1";
            str += Environment.NewLine + "audiomode:i:0";

            File.WriteAllText(remoteFile, str);

            var proc1 = new ProcessStartInfo();
            proc1.UseShellExecute = true;
            proc1.WorkingDirectory = @"C:\Windows\System32";
            proc1.FileName = @"C:\Windows\System32\cmd.exe";
            proc1.Verb = "runas";
            proc1.Arguments = "/c  mstsc " + remoteFile;
            proc1.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(proc1);

            var oldfiles = Directory.GetFiles(filePath, filaNamePattern).Except(new List<string>() { remoteFile });
            if (oldfiles.Any())
            {
                foreach (var file in oldfiles)
                    File.Delete(file);
            }
        }
    }
}
