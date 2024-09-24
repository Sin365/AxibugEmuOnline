using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxibugEmuOnline.Client
{

    public class NormalScheme : ControlScheme
    {
    }

    public static partial class ControlSchemeSetts
    {
        public static NormalScheme Normal { get; private set; } = new NormalScheme();
    }
}
