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

        public static bool IsInputing { get; private set; }



        protected override void OnShow(object param)
        {
            (Action<string> callback, string placeHolder, string defaultText) t = ((Action<string> callback, string placeHolder, string defaultText))param;

            OnCommit = t.callback;
            (m_input.placeholder as Text).text = t.placeHolder;
            m_input.text = t.defaultText;
        }

        protected override void Update()
        {
            base.Update();

            IsInputing = m_input.isFocused;

            if (IsInputing && Input.GetButtonDown("Submit"))
            {
                OnCmdEnter();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            IsInputing = false;
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
