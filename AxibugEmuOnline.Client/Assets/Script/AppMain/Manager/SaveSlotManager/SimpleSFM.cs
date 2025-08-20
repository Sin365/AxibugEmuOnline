using System;
using System.Collections.Generic;
using System.Linq;

namespace AxibugEmuOnline.Client.Tools
{
    public partial class SimpleFSM<HOST>
    {
        public event Action OnStateChanged;

        private Dictionary<Type, State> m_states = new Dictionary<Type, State>();
        private bool isInTransition = false; // 新增：标识是否处于切换过程中
        private Queue<Type> pendingStateQueue = new Queue<Type>(); // 新增：延迟请求的队列

        public HOST Host { get; private set; }

        public SimpleFSM(HOST host)
        {
            Host = host;
        }

        private State m_currentState;
        public State CurrentState
        {
            get => m_currentState;
            set
            {
                if (m_currentState == value) return;

                m_currentState = value;
                OnStateChanged?.Invoke();
            }
        }
        public T AddState<T>() where T : State, new()
        {
            var stateType = typeof(T);

            if (m_states.ContainsKey(stateType)) { return null; }

            var state = State.CreateState<T>(this);
            m_states.Add(typeof(T), state);

            return (T)state;
        }

        
        public void ChangeState<T>() where T : State, new()
        {
            var stateType = typeof(T);
            if (!m_states.ContainsKey(stateType))
                return;

            // 如果处于切换中，加入队列等待后续处理
            if (isInTransition)
            {
                pendingStateQueue.Enqueue(stateType);
                return;
            }

            // 标记开始切换
            isInTransition = true;
            InternalChangeState(stateType);
            isInTransition = false; // 切换结束

            // 处理队列中积累的切换请求
            ProcessPendingQueue();
        }

        // 新增：实际执行状态切换的方法
        private void InternalChangeState(Type stateType)
        {
            State nextState = m_states[stateType];
            if (nextState == null) return;

            State preState = CurrentState;
            // 退出当前状态
            if (preState != null) preState.OnExit(nextState);

            // 更新当前状态
            CurrentState = nextState;
            CurrentState.LastState = preState;

            // 进入新状态
            CurrentState.OnEnter(preState);
        }

        // 新增：处理队列中的切换请求
        private void ProcessPendingQueue()
        {
            while (pendingStateQueue.Count > 0)
            {
                Type nextType = pendingStateQueue.Dequeue();
                if (!m_states.ContainsKey(nextType)) continue;

                isInTransition = true;
                InternalChangeState(nextType);
                isInTransition = false;
            }
        }

        public T GetState<T>() where T : State, new()
        {
            m_states.TryGetValue(typeof(T), out var value);
            return value as T;
        }

        public void Update()
        {
            CurrentState?.OnUpdate();
            foreach (var state in m_states.Values)
            {
                if (state == CurrentState) continue;
                state.AnyStateUpdate(CurrentState);
            }
        }

        public abstract class State
        {
            public SimpleFSM<HOST> FSM { get; private set; }
            public HOST Host => FSM.Host;
            protected virtual void OnInit() { }
            public virtual void OnEnter(State preState) { }
            public virtual void OnExit(State nextState) { }
            public virtual void OnUpdate() { }
            public virtual void AnyStateUpdate(State currentState) { }

            public State LastState { get; set; }

            protected State() { }
            public static State CreateState<T>(SimpleFSM<HOST> fsm) where T : State, new()
            {
                var state = new T()
                {
                    FSM = fsm
                };

                state.OnInit();

                return state;
            }

        }
    }
}
