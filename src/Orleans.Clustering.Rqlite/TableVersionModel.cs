using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Clustering.Rqlite
{
    public class TableVersionModel
    {
        public string ClusterId { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;

    }
    public class TableVersionData
    {
        public int Version { get; set; }
        public string VersionEtag { get; set; } = string.Empty;

    }
}
