namespace AxibugEmuOnline.Client
{
    public class OptionUI_ExecuteItem : OptionUI_MenuItem<ExecuteMenu>
    {
        public override void OnExecute(OptionUI optionUI, ref bool cancelHide)
        {
            MenuData.OnExcute(optionUI, ref cancelHide);
        }
    }
}
