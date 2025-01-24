using UnityEngine;


public class UniMAMESetting
{
    public static UniMAMESetting instance
    {
        get
        {
            if (mInstance == null)
                mInstance = new UniMAMESetting();
            return mInstance;
        }
    }
    private static UniMAMESetting mInstance;
}