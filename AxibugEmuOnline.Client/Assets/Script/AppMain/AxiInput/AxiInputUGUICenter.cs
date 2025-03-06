using System.Collections.Generic;

namespace Assets.Script.AppMain.AxiInput
{
    public static class AxiInputUGUICenter
    {
        static int handleSeed = 0;
        static Dictionary<int, AxiInputUGUIHandleBase> dictHandle2AxiUgui = new Dictionary<int, AxiInputUGUIHandleBase>();
        static Dictionary<AxiInputUGuiBtnType, List<AxiInputUGUIHandleBase>> dictBtnType2BtnList = new Dictionary<AxiInputUGuiBtnType, List<AxiInputUGUIHandleBase>>();

        public static int GetNextSeed()
        {
            return ++handleSeed;
        }
        public static void RegHandle(AxiInputUGUIHandleBase uiHandle)
        {
            dictHandle2AxiUgui[uiHandle.Handle] = uiHandle;
            List<AxiInputUGUIHandleBase> list;
            if (dictBtnType2BtnList.TryGetValue(uiHandle.UguiBtnType, out list))
                list = dictBtnType2BtnList[uiHandle.UguiBtnType] = new List<AxiInputUGUIHandleBase>();

            if (!list.Contains(uiHandle))
                list.Add(uiHandle);
        }
        public static void UnregHandle(AxiInputUGUIHandleBase uiHandle)
        {
            if (!dictHandle2AxiUgui.ContainsKey(uiHandle.Handle))
                return;
            dictHandle2AxiUgui.Remove(uiHandle.Handle);

            List<AxiInputUGUIHandleBase> list;
            if (dictBtnType2BtnList.TryGetValue(uiHandle.UguiBtnType, out list))
            {
                if (list.Contains(uiHandle))
                    list.Remove(uiHandle);
            }
        }

        public static bool GetKeyUp(AxiInputUGuiBtnType btntype)
        {
            List<AxiInputUGUIHandleBase> list;
            if (!dictBtnType2BtnList.TryGetValue(btntype, out list))
                return false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetKeyUp())
                    return true;
            }
            return false;
        }

        public static bool GetKeyDown(AxiInputUGuiBtnType btntype)
        {
            List<AxiInputUGUIHandleBase> list;
            if (!dictBtnType2BtnList.TryGetValue(btntype, out list))
                return false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetKeyDown())
                    return true;
            }
            return false;
        }

        public static bool GetKey(AxiInputUGuiBtnType btntype)
        {
            List<AxiInputUGUIHandleBase> list;
            if (!dictBtnType2BtnList.TryGetValue(btntype, out list))
                return false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetKey())
                    return true;
            }
            return false;
        }
    }
}
