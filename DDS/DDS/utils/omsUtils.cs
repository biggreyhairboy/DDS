using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.utils
{
    enum DDSCommandType
    {
        Subscribe = 0,
        Contribute = 1
    }

    enum DDSCommandMode
    {
        image = 0,
        update = 1,
        both = 2
    }
}
