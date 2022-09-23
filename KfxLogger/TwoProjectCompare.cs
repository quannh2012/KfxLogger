using System;
using System.IO;
using System.Linq;

namespace KfxLogger
{
    internal class TwoProjectCompare
    {
        internal static void Do()
        {
            var prj1 = @"C:\z\Project_KAFTA_09212022 17-40-04-487 STG";
            var prj2 = @"C:\z\Project_KAFTA_09212022 17-42-59-529 TestCBA";
            var files1 = Directory.GetFiles(prj1, "*", SearchOption.AllDirectories).ToList();
            var files2 = Directory.GetFiles(prj2, "*", SearchOption.AllDirectories).ToList();

            var rmr1 = files1.Select(x => x.Replace(prj1, string.Empty)).ToList();
            var rmr2 = files2.Select(x => x.Replace(prj2, string.Empty)).ToList();
            var z = rmr1.Except(rmr2).ToList();
            z = z.Where(x => x.ToLower().Contains("record") || x.ToLower().Contains("metric")).ToList();
        }
    }
}