using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gijima.IOBM.Infrastructure.Structs
{
    public class DataActivityLog
    {
        public short ActivityProcess { get; set; }
        public string ActivityDescription { get; set; }
        public string ActivityComment { get; set; }
        public int EntityID { get; set; }
        public string ChangedValue { get; set; }
    }
}
