using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualNes.Core
{
    // 昤夋曽幃
    public enum EnumRenderMethod
    {
        POST_ALL_RENDER = 0, // 僗僉儍儞儔僀儞暘偺柦椷幚峴屻丆儗儞僟儕儞僌
        PRE_ALL_RENDER = 1, // 儗儞僟儕儞僌偺幚峴屻丆僗僉儍儞儔僀儞暘偺柦椷幚峴
        POST_RENDER = 2, // 昞帵婜娫暘偺柦椷幚峴屻丆儗儞僟儕儞僌
        PRE_RENDER = 3, // 儗儞僟儕儞僌幚峴屻丆昞帵婜娫暘偺柦椷幚峴
        TILE_RENDER = 4  // 僞僀儖儀乕僗儗儞僟儕儞僌
    }
}
