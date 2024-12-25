using System;
using UnityEngine;
public class SonyVitaCommonDialog : MonoBehaviour
{
    static Action<string> resultAct = null;
    void Awake()
	{
#if UNITY_PSP2
        Sony.Vita.Dialog.Ime.OnGotIMEDialogResult += OnGotIMEDialogResult;
		Sony.Vita.Dialog.Main.Initialise();
#endif
	}

	public void ShowPSVitaIME(Action<string> callback, string placeHolder, string defaultText)
    {
        resultAct = callback;
#if UNITY_PSP2
        Sony.Vita.Dialog.Ime.ImeDialogParams info = new Sony.Vita.Dialog.Ime.ImeDialogParams();

        // Set supported languages, 'or' flags together or set to 0 to support all languages.
        info.supportedLanguages = Sony.Vita.Dialog.Ime.FlagsSupportedLanguages.LANGUAGE_JAPANESE |
                                    Sony.Vita.Dialog.Ime.FlagsSupportedLanguages.LANGUAGE_ENGLISH_GB |
                                    Sony.Vita.Dialog.Ime.FlagsSupportedLanguages.LANGUAGE_DANISH;
        info.languagesForced = true;

        info.type = Sony.Vita.Dialog.Ime.EnumImeDialogType.TYPE_DEFAULT;
        info.option = 0;
        info.canCancel = true;
        info.textBoxMode = Sony.Vita.Dialog.Ime.FlagsTextBoxMode.TEXTBOX_MODE_WITH_CLEAR;
        info.enterLabel = Sony.Vita.Dialog.Ime.EnumImeDialogEnterLabel.ENTER_LABEL_DEFAULT;
        info.maxTextLength = 128;
        info.title = placeHolder;
        info.initialText = defaultText;
        Sony.Vita.Dialog.Ime.Open(info);
#endif
    }

#if UNITY_PSP2
    void OnGotIMEDialogResult(Sony.Vita.Dialog.Messages.PluginMessage msg)
    {
		Sony.Vita.Dialog.Ime.ImeDialogResult result = Sony.Vita.Dialog.Ime.GetResult();
        Debug.Log("IME result: " + result.result);
        Debug.Log("IME button: " + result.button);
        Debug.Log("IME text: " + result.text);
		if (result.result == Sony.Vita.Dialog.Ime.EnumImeDialogResult.RESULT_OK)
		{
            resultAct.Invoke(result);
        }
    }
#endif


#if UNITY_PSP2
	void Update ()
    {
        Sony.Vita.Dialog.Main.Update();
	}
#endif

}
