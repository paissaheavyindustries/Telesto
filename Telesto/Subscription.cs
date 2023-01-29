using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telesto
{

    internal abstract class Subscription
    {

        internal Plugin p { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string endpoint { get; set; }
        public bool first { get; set; }

        internal abstract bool Refresh(Context ctx);
        internal abstract void GetRepresentation(Context ctx, out string oldrep, out string newrep);

    }

}
