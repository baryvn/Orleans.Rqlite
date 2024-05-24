using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Clustering.Rqlite
{
    public class MemberModel
    {
        public string SiloAddress { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;

    }
}
