using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class InputUI : OverlayUI
    {
        [SerializeField]
        InputField m_input;

        Action<string> OnCommit;


        protected override void OnShow(object param)
        {
            (Action<string> callback, string placeHolder, string defaultText) t = ((Action<string> callback, string placeHolder, string defaultText))param;

            OnCommit = t.callback;
            (m_input.placeholder as Text).text = t.placeHolder;
            m_input.text = t.defaultText;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            StartCoroutine(ActiveInput());
        }

        private IEnumerator ActiveInput()
        {
            yield return new WaitForEndOfFrame();

            m_input.Select();
            m_input.ActivateInputField();

            yield break;
        }

        protected override bool OnCmdEnter()
        {
            OnCommit?.Invoke(m_input.text);
            Close();
            return true;
        }

        protected override void OnCmdBack()
        {
            Close();
        }
    }
}
