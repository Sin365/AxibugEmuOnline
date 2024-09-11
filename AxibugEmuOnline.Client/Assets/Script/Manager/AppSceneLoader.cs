using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AxibugEmuOnline.Client
{
    public class AppSceneLoader
    {
        public LoadTask CurrentTask { get; private set; }
        
        Queue<LoadTask> m_tasks = new Queue<LoadTask>();
        Coroutine m_coroutine;
        public void BeginLoad(string scenePath, Action callback)
        {
            m_tasks.Enqueue(new LoadTask(scenePath, callback));
            if (m_coroutine == null)
                m_coroutine = App.StartCoroutine(TaskFlow());
        }

        private IEnumerator TaskFlow()
        {
            while (m_tasks.Count > 0)
            {
                CurrentTask = m_tasks.Dequeue();

                while (CurrentTask.MoveNext()) yield return null;

                CurrentTask.Callback();
            }

            CurrentTask = null;
            m_coroutine = null;
        }

        public class LoadTask
        {
            public EnumTaskState State { get; private set; } = EnumTaskState.Idle;
            public float Progress => m_loadingOp != null ? m_loadingOp.progress : 0;

            private AsyncOperation m_loadingOp;
            private string m_scenePath;
            private Action m_callback;
            public LoadTask(string scenePath, Action callback)
            {
                m_scenePath = scenePath;
                m_callback = callback;
            }

            public bool MoveNext()
            {
                if (State == EnumTaskState.Idle)
                {
                    State = EnumTaskState.Running;
                    m_loadingOp = SceneManager.LoadSceneAsync(m_scenePath, LoadSceneMode.Single);
                    return true;
                }
                else if (State == EnumTaskState.Running)
                {
                    m_loadingOp.allowSceneActivation = true;
                    if (m_loadingOp.isDone)
                    {
                        State = EnumTaskState.Complete;
                    }
                    
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Callback()
            {
                m_callback?.Invoke();
            }
        }

        public enum EnumTaskState
        {
            Idle,
            Running,
            Complete
        }
    }
}
