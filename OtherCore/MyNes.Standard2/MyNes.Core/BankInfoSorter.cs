using System.Collections.Generic;

namespace MyNes.Core
{
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
