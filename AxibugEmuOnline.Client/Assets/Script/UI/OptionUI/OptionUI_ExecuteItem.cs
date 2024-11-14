namespace AxibugEmuOnline.Client
{
    public class OptionUI_ExecuteItem : OptionUI_MenuItem<ExecuteMenu>
    {
        public override void OnExecute()
        {
            MenuData.OnExcute();
        }
    }
}
