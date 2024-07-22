using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;

namespace MyNes.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal class BankInfoSorter : IComparer<BankInfo>
    {
    	public int Compare(BankInfo x, BankInfo y)
    	{
    		int result = 0;
    		int result2 = 0;
    		int.TryParse(x.ID, out result);
    		int.TryParse(y.ID, out result2);
    		return result2 - result;
    	}
    }
}
