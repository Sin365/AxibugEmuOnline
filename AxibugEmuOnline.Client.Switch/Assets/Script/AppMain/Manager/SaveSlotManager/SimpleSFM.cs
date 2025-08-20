using System;
using System.Collections.Generic;
using System.Linq;

namespace AxibugEmuOnline.Client.Tools
{
    public partial class SimpleFSM<HOST>
    {
        public event Action OnStateChanged;
        private Dictionary<Type, State> m_states = new Dictionary<Type, State>();

        public HOST Host { get; private set; }

        public SimpleFSM(HOST host)
        {
            Host = host;
        }

        private State m_current;
        public State CurrentState
        {
            get => m_current;
            set
            {
                if (m_current == value) return;

                m_current = value;
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

        public void BackToLast()
        {
            if (m_current == null) return;
            if (m_current.LastState == null) return;

            if (m_states.Values.FirstOrDefault(s => s == m_current.LastState) is State lastState)
            {
                m_current.LastState = null;
                m_current.OnExit(lastState);

                lastState.OnEnter(m_current);
                m_current = lastState;
            }
        }

        public void Stop()
        {
            if (m_current != null)
            {
                m_current.OnExit(null);
                m_current = null;
            }

            foreach (var state in m_states.Values)
                state.LastState = null;
        }

        public void ChangeState<T>() where T : State, new()
        {
            var stateType = typeof(T);
            m_states.TryGetValue(stateType, out State nextState);

            if (nextState == null) return;

            if (m_current != null) m_current.OnExit(nextState);
            nextState.LastState = m_current;
            nextState.OnEnter(m_current);
            m_current = nextState;
        }

        public T GetState<T>() where T : State, new()
        {
            m_states.TryGetValue(typeof(T), out var value);
            return value as T;
        }

        public void Update()
        {
            m_current?.OnUpdate();
            foreach (var state in m_states.Values)
            {
                if (state == m_current) continue;
                state.AnyStateUpdate(m_current);
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
