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
            public string StationId { get; set; }
            public Guid gid = Guid.NewGuid();
        }
        internal static void Do()
        {
            var path = @"C:\z";
            var patt = "log_*.arch";
            var files = Directory.GetFiles(path, patt, SearchOption.AllDirectories).ToList().OrderBy(x => new FileInfo(x).LastWriteTime).ToList();

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var erroror_0001 = Path.Combine(path, "erroror_0001.txt");
            File.WriteAllText(erroror_0001, "");
            var StationErr = Path.Combine(path, "StationErr000.txt");
            File.WriteAllText(StationErr, "");

            var quote = "\\" + '"';
            var dateformat = @"\d{4}-\d{2}-\d{2}";
            var timeformat = @"\d{2}:\d{2}:\d{2}";
            var number = @"\d+";
            var m05a = new[] { "05", dateformat, timeformat, dateformat, timeformat, ".*", number, ".*", number, ".*", number, number, number, number, number, number };
            var m05p = string.Join(",", m05a.Select(x => quote + "(" + x + ")" + quote));

            var stationDict = new Dictionary<object, object>();
            var ModuleDict = new Dictionary<object, object>();
            var cache = new List<object>();
            foreach (var f in files)
            {
                var fi = new FileInfo(f);
                File.AppendAllText(erroror_0001,Environment.NewLine + Environment.NewLine + "========" + fi.Name + "========="  + Environment.NewLine);
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

                    
                    var l = new List<Et00>();
                    var mm5 = Regex.Matches(thisblock, m05p, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    if (mm5.Count > 0)
                    {
                        var los = new List<Et00>();
                        foreach (Match m5 in mm5)
                        {
                            var e = new Et00();
                            e.StartTime = m5.Groups[2].Value + " " + m5.Groups[3].Value;
                            e.EndTime = m5.Groups[4].Value + " " + m5.Groups[5].Value;
                            e.ModuleName = m5.Groups[6].Value.Trim();
                            e.StationId = m5.Groups[8].Value.Trim();
                            los.Add(e);
                        }
                        var rmg = new List<Guid>();
                        var losByTime = los.OrderBy(x => x.StartTime).ToList();
                        var exps = losByTime.Where(x => x.ModuleName == "Export").ToList();
                        var lastexport =  exps.LastOrDefault();
                        //if (exps.Count>1)
                        //{
                        //    cache.Add("zzzzzzz");
                        //    var multiExp = true;
                        //}
                        if (lastexport!=null)
                        {
                            var markerid = lastexport.gid;
                            foreach (var e in losByTime)
                            {
                                rmg.Add(e.gid);
                                if (e.gid == markerid) break;
                            }
                            var issues = losByTime.Where(x => !rmg.Contains(x.gid)).ToList();
                            if (issues.Count > 0)
                            {
                                var mraw = "^" + quote + number + quote + ".+$";
                                var raws = Regex.Matches(thisblock, mraw, RegexOptions.Multiline);

                                var rep = new List<object>();
                                foreach (Match raw in raws)
                                {
                                    var b = false;
                                    foreach (var i in issues)
                                    {
                                        if (raw.Value.Contains(i.StationId))
                                        {
                                            b = true;
                                            break;
                                        }
                                    }
                                    rep.Add((b ? "-- " : "") + raw.Value.Trim());
                                }
                                if (rep.Count > 1) rep.Add(Environment.NewLine);
                                cache.Add(string.Join(Environment.NewLine, rep));

                                foreach (var ii in issues)
                                {
                                    stationDict[Regex.Replace(ii.StationId, ":.+", "")] = 1;
                                    ModuleDict[ii.ModuleName] = 1;
                                }

                            }
                            else
                            {
                                //cache.Add(thisblock.Trim() + Environment.NewLine);
                            }
                        }
                        
                        if (cache.Count >= 500)
                        {
                            File.AppendAllText(erroror_0001, string.Join(Environment.NewLine, cache));
                            cache.Clear();
                            File.WriteAllText(StationErr,
                           "Stations: " +
                           string.Join(", ", stationDict.Keys.OrderBy(x => x)) +
                           Environment.NewLine +
                           "Modules: " +
                           string.Join(", ", ModuleDict.Keys.OrderBy(x => x)));

                        }
                    }
                }
            }
            if (cache.Count > 0)
            {
                File.AppendAllText(erroror_0001, string.Join(Environment.NewLine, cache));
                cache.Clear();
            }

            File.WriteAllText(StationErr,
                "Stations: " + 
                string.Join(", ", stationDict.Keys.OrderBy(x=>x))+
                Environment.NewLine+
                "Modules: "+
                string.Join(", ", ModuleDict.Keys.OrderBy(x=>x)));


        }
    }
}