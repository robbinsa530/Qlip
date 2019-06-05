using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qlip
{
    public class QlipConfigChangedArgs: EventArgs
    {
        /// <summary>
        /// New save count. Need to know because if it shrunk below the
        /// number of clips that are currently saved, extras must be removed
        /// </summary>
        public int NewSaveCount { get; set; }
    }
}
