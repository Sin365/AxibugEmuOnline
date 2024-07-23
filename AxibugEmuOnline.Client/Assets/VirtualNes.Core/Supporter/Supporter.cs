using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VirtualNes.Core
{
    public static class Supporter
    {
        private static ISupporterImpl s_support;
        public static void Setup(ISupporterImpl supporter)
        {
            s_support = supporter;
        }

        public static Stream OpenFile(string fname)
        {
            return s_support.OpenFile(fname);
        }
    }

    public interface ISupporterImpl
    {
        Stream OpenFile(string fname);
    }
}
