using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class OptionUI_ExecuteItem : OptionUI_MenuItem<ExecuteMenu>
    {
        public GameObject ExpandFlag;

        protected override void OnSetData(OptionMenu menuData)
        {
            base.OnSetData(menuData);

            ExpandFlag.SetActiveEx(IsExpandMenu);
        }

        public override void OnExecute(OptionUI optionUI, ref bool cancelHide)
        {
            MenuData.OnExcute(optionUI, ref cancelHide);
        }
    }
}
