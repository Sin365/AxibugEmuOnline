using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class OptionUI_ExecuteItem : OptionUI_MenuItem<ExecuteMenu>
    {
        public GameObject ExpandFlag;
        public GameObject ApplyFlag;

        protected override void OnSetData(InternalOptionMenu menuData)
        {
            base.OnSetData(menuData);

            ExpandFlag.SetActiveEx(IsExpandMenu);
            ApplyFlag.SetActiveEx(IsApplied);
        }

        public override void OnExecute(OptionUI optionUI, ref bool cancelHide)
        {
            MenuData.OnExcute(optionUI, ref cancelHide);
        }

        protected override void Update()
        {
            ApplyFlag.SetActiveEx(IsApplied);
        }
    }
}
