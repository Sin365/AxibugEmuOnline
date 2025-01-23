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

    const string KEY_LASTGAMEROM = "MAME_LASTGAMEROM";

    public string LastGameRom
    {
        get
        {
            if (PlayerPrefs.HasKey(KEY_LASTGAMEROM))
                return PlayerPrefs.GetString(KEY_LASTGAMEROM);
            return string.Empty;
        }
        set
        {
            PlayerPrefs.SetString(KEY_LASTGAMEROM, value);
        }
    }

}