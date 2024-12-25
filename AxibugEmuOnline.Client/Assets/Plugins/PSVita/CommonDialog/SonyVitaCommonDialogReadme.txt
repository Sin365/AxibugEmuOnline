
Unity SCE Common Dialog Example Project.

This example project demonstrates how to use the Unity SCE Common Dialog API for displaying and retrieving results from user, system, progress, error and text entry (IME) dialogs.

Project Folder Structure

	Plugins/PSVita - Contains the CommonDialog native plugin.
	SonyAssemblies - Contains the SonyVitaCommonDialog managed interface to the CommonDialog plugin.
	SonyExample/CommonDialog - Contains a Unity scene which runs the scripts.
	SonyExample/CommonDialog/Scripts - Contains the Sony NP example scripts.
	SonyExample/Utils - Contains various utility scripts for use by the example.

The SonyVitaCommonDialog managed assembly defines the following namespaces...

Sony.Vita.Dialog.Main		Contains methods for initialising and updating the plugin.
Sony.Vita.Dialog.Common		Contains methods for working with the SCE Common Dialog for user, system, progress, and error messages.
Sony.Vita.Dialog.Common.Ime	Contains methods for working with the SCE IME Dialog for text entry.

Sony.Vita.Dialog.Main

	Methods.

		public static void Initialise()
		Initialises the plugin, call once.

		public static void Update()
		Updates the plugin, call once each frame.

Sony.Vita.Dialog.Common

	Enumerations.

		System message dialog types, these are a one-one match with the values defined by SceMsgDialogSystemMessageType.
		public enum EnumSystemMessageType
		{
			MSG_DIALOG_SYSMSG_TYPE_WAIT								= 1,
			MSG_DIALOG_SYSMSG_TYPE_NOSPACE							= 2,
			MSG_DIALOG_SYSMSG_TYPE_MAGNETIC_CALIBRATION				= 3,
			MSG_DIALOG_SYSMSG_TYPE_WAIT_SMALL						= 5,
			MSG_DIALOG_SYSMSG_TYPE_WAIT_CANCEL						= 6,
			MSG_DIALOG_SYSMSG_TYPE_NOSPACE_CONTINUABLE				= 9,
			MSG_DIALOG_SYSMSG_TYPE_LOCATION_DATA_OBTAINING			= 10,
			MSG_DIALOG_SYSMSG_TYPE_LOCATION_DATA_FAILURE			= 11,
			MSG_DIALOG_SYSMSG_TYPE_LOCATION_DATA_FAILURE_RETRY		= 12,
			MSG_DIALOG_SYSMSG_TYPE_PATCH_FOUND						= 13,
		}

		User message dialog types, these are a one-one match with the values defined by SceMsgDialogButtonType.
		public enum EnumUserMessageType
		{
			MSG_DIALOG_BUTTON_TYPE_OK				= 0,
			MSG_DIALOG_BUTTON_TYPE_YESNO			= 1,
			MSG_DIALOG_BUTTON_TYPE_NONE				= 2,
			MSG_DIALOG_BUTTON_TYPE_OK_CANCEL		= 3,
			MSG_DIALOG_BUTTON_TYPE_CANCEL			= 4,
			MSG_DIALOG_BUTTON_TYPE_3BUTTONS			= 5,
		}

		Dialog result, the button or action that resulted in the dialog closing.
		public enum EnumCommonDialogResult
		{
			RESULT_BUTTON_NOT_SET,
			RESULT_BUTTON_OK,
			RESULT_BUTTON_CANCEL,
			RESULT_BUTTON_YES,
			RESULT_BUTTON_NO,
			RESULT_BUTTON_1,
			RESULT_BUTTON_2,
			RESULT_BUTTON_3,
			RESULT_CANCELED,
			RESULT_ABORTED,
			RESULT_CLOSED,
		}


	Events.

		OnGotDialogResult		Triggered when a dialog has closed and the result is available.

	Properties.

		public static bool IsDialogOpen
		Is a dialog open?

	Methods.

		public static bool ShowErrorMessage(UInt32 errorCode)
		Display an error message.

		public static bool ShowSystemMessage(EnumSystemMessageType type, bool infoBar, int value)
		Display a system message.

		public static bool ShowProgressBar(string message)
		Display a progress bar.

		public static bool SetProgressBarPercent(int percent)
		Set progress bar percentage (0-100).

		public static bool SetProgressBarMessage(string message)
		Set progress bar message string.

		public static bool ShowUserMessage(EnumUserMessageType type, bool infoBar, string str)
		Show a user message.

		public static bool ShowUserMessage3Button(bool infoBar, string str, string button1, string button2, string button3)
		Show a user message with 3 custom buttons.
				
		public static bool Close()
		Close the dialog.

		public static EnumCommonDialogResult GetResult()
		Get the result from the dialog that's just closed.

Sony.Vita.Dialog.Common.Ime

	Enumerations.

		ImeParam enterLabel
		public enum EnumImeDialogEnterLabel
		{
			ENTER_LABEL_DEFAULT,
			ENTER_LABEL_SEND,
			ENTER_LABEL_SEARCH,
			ENTER_LABEL_GO,
		}
				
		ImeParam type
		public enum EnumImeDialogType
		{
			TYPE_DEFAULT,		    UI for regular text input
			TYPE_BASIC_LATIN,	    UI for alphanumeric character input
			TYPE_NUMBER,		    UI for number input
			TYPE_EXTENDED_NUMBER,	UI for extended number input
			TYPE_URL,	        	UI for entering URL
			TYPE_MAIL,		        UI for entering an email address
		}

		Dialog result.
		public enum EnumImeDialogResult
		{
			RESULT_OK,				User selected either close button or Enter button
			RESULT_USER_CANCELED,	User performed cancel operation.
			RESULT_ABORTED,			IME Dialog operation has been aborted.
		}
		
		Dialog result button.
		public enum EnumImeDialogResultButton
		{
			BUTTON_NONE,	IME Dialog operation has been aborted or canceled.
			BUTTON_CLOSE,	User selected close button
			BUTTON_ENTER,	User selected Enter button
		}

	Flags.
				
		ImeParam textBoxMode, can be OR'd together.
		[Flags] public enum FlagsTextBoxMode
		{
			TEXTBOX_MODE_DEFAULT = 0x00,       Text box for regular sentence input
			TEXTBOX_MODE_PASSWORD = 0x01,      Text box for password input
			TEXTBOX_MODE_WITH_CLEAR = 0x02,    Text box with clear button
		};
				
		ImeParam option, can be OR'd together.
		[Flags] public enum FlagsTextBoxOption
		{
			OPTION_DEFAULT = 0x00,
			OPTION_MULTILINE = 0x01,              Multiline input option. This option is not available for libime. This can be used only for the IME Dialog library.
			OPTION_NO_AUTO_CAPITALIZATION = 0x02, Prohibits automatic capitalization
			OPTION_NO_ASSISTANCE = 0x04,          Prohibits input assistance UIs, such as predictive text and conversion candidate
		}

	Structures.

		public class ImeDialogParams
		{
			public EnumImeDialogType type;				Dialog type.
			public FlagsTextBoxOption option;			Option flags.
			public bool canCancel;						Whether or not to add the cancel button.
			public FlagsTextBoxMode textBoxMode;		Text box mode.
			public EnumImeDialogEnterLabel enterLabel;	Type of enter label.
			public int maxTextLength;					Maximum text length.
			public string title;						Dialog title.
			public string initialText;					Initial text for the text entry field.
		};

		public struct ImeDialogResult
		{
			public EnumImeDialogResult result;			Dialog result.
			public EnumImeDialogResultButton button;	Dialog result button.
			public string text;							The text as entered by the user.
		};

	Events.

		OnGotIMEDialogResult	Triggered when the dialog has closed and the result is ready.

	Properties.

		public static bool IsDialogOpen
		Is the IME dialog open?

	Methods.

		public static bool Open(ImeDialogParams info)
		Opens the IME dialog.
				
		public static ImeDialogResult GetResult()
		Gets the IME dialog result.

