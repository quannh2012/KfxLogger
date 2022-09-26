using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KfxLogger
{
    internal class KCLogFindTimezoneIssue
    {
        class Et00
        {
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string ModuleName { get; set; }
        }
        internal static void Do()
        {
            var path = @"C:\z";
            var patt = "log_*.arch";
            var files = Directory.GetFiles(path, patt, SearchOption.AllDirectories).ToList().OrderBy(x=>new FileInfo(x).LastWriteTime).ToList();


            var exportname = Path.Combine(path, "erroror_0001.txt");
            File.WriteAllText(exportname, "");
            var SummaryFileName = Path.Combine(path, "summ_0001.txt");
            File.WriteAllText(SummaryFileName, "");
            var StationErr = Path.Combine(path, "StationErr000.txt");
            File.WriteAllText(StationErr, "");

            var quote = "\\"+'"';
            var dateformat = @"\d{4}-\d{2}-\d{2}";
            var timeformat = @"\d{2}:\d{2}:\d{2}";
            var number = @"\d+";
            var m05a = new[] { "05", dateformat, timeformat, dateformat, timeformat, ".*", number, ".*", number, ".*", number, number, number, number, number, number };
            var m05p = string.Join(",", m05a.Select(x => quote + "("+x +")"+ quote));

            var StationErrorDict = new Dictionary<object, object>();
            foreach (var f in files)
            {
                var bb = File.ReadAllText(f);
                var m = Regex.Match(bb, "^\"01\",", RegexOptions.Multiline | RegexOptions.IgnoreCase);
 
                while (m.Success)
                {
                    var m1 = m; var m2 = m1.NextMatch(); m = m2;
                    string thisblock = null;
                    if (m2.Success)
                        thisblock = bb.Substring(m1.Index, m2.Index - m1.Index);
                    else
                        thisblock = bb.Substring(m1.Index);

                    /* “05”,“queue start date”,
“queue start time”,
“queue end date”,
“queue end time”,
“queue process name
*/

                    //"05","2022-09-09","01:04:55","2022-09-09","01:04:56",
                    var modules = "^\"05\",\"(\\d{4}-\\d{2}-\\d{2})\",\"(\\d{2}:\\d{2}:\\d{2})\",\"(\\d{4}-\\d{2}-\\d{2})\",\"(\\d{2}:\\d{2}:\\d{2})\",\"(.+)";


                    var mm = Regex.Matches(thisblock, modules, RegexOptions.Multiline | RegexOptions.IgnoreCase);

                    var l = new List<Et00>();
                    for(int i=0;i<mm.Count;i++)
                    {
                        var s01b = new[] { mm[i].Groups[1].Value.Trim() , mm[i].Groups[2].Value.Trim() };
                        var s01e = new[] { mm[i].Groups[2].Value.Trim(), mm[i].Groups[4].Value.Trim() };                        
                        var e = new Et00();
                        e.ModuleName = mm[i].Groups[5].Value.Trim();
                        e.ModuleName = Regex.Replace(e.ModuleName, "\",.+", "");
                        e.StartTime = string.Join(" ", s01b);
                        e.EndTime = string.Join(" ", s01e);
                        l.Add(e);
                    }
                    var zz = string.Join(" ", l.OrderBy(x=>x.StartTime).Select(o=>o.ModuleName));
                    if (Regex.IsMatch(zz,"Export\\s+\\w+"))
                    {
                       

                        var rep = new List<string>();

                        rep.Add(thisblock.Trim());
                        var sl = l.OrderBy(o=>o.StartTime).Select(x=>"  "+string.Join (", ",new[] { x.StartTime, x.EndTime,x.ModuleName}));
                        rep.Add(string.Join(Environment.NewLine, sl));
                        rep.Add(Environment.NewLine);
                        File.AppendAllText(exportname, string.Join(Environment.NewLine, rep));
                        var error = 1;

                        if (mm.Count > 0)
                        {
                            var mmm = Regex.Matches(thisblock, m05p, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                            var lms = new List<Match>();
                            foreach (Match mi in mmm) lms.Add(mi);
                            lms = lms.OrderBy(x => "" + x.Groups[2] +" "+ x.Groups[3]).ToList();
                            var sample = String.Join(Environment.NewLine, lms.Select(x => x.Value.Trim()));
                            var last = lms.LastOrDefault();
                            if (last!=null)
                            {
                                
                                var workid = Regex.Replace("" + last.Groups[8], ":Sess .*", "");
                                //var workstation = last.Groups[6] + ";" + last.Groups[8];
                                var workstation = last.Groups[6] + ";" + workid;
                                File.AppendAllText(SummaryFileName, workstation+Environment.NewLine);
                                StationErrorDict[workid] = 1;
                            }

                        }

                    }
                    
                }

               

            }

            File.WriteAllText(StationErr, string.Join(Environment.NewLine, StationErrorDict.Keys));

            

        }
    }
}