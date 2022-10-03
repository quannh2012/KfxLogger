using System;
using System.IO;
using System.Text.RegularExpressions;

namespace KfxLogger
{
    internal class InsightLogAuthenticationDetector
    {
        internal static void Do()
        {
            var output = @"C:\temp";
            output = Path.Combine(output, "" + Guid.NewGuid());
            if (!Directory.Exists(output)) Directory.CreateDirectory(output);
            var logdir = @"C:\Users\Administrator\Desktop\26603032 - Server Error in Insight View Application OR Login failed You are not authenticated error\ExportLogArchive";
            var filter = "WcfDataService.*";
            var files = Directory.GetFiles(logdir, filter, SearchOption.AllDirectories);
            //Application: Viewer
            int counter = 1;
            foreach (var f in files)
            {
                var bb = File.ReadAllText(f);
                var m = Regex.Match(bb, @"^\s*Application: (\w+).*", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                while (m.Success)
                {
                    var m1 = m; var m2 = m1.NextMatch(); m = m2;
                    string thisblock = null;
                    if (m2.Success)
                        thisblock = bb.Substring(m1.Index, m2.Index - m1.Index);
                    else
                        thisblock = bb.Substring(m1.Index);
                    var appname = m1.Groups[1].Value;
                    string prefuix = "L_";
                    if (thisblock.Contains("Could not login")||
                        thisblock.Contains("You are not authenticated") 
                        ) prefuix = "Err_";
                    var fn = Path.Combine(output, prefuix + appname+  (counter++)+ ".txt");
                    File.WriteAllText(fn, thisblock);
                    Console.WriteLine(fn);
                }
            }
        }
    }
}