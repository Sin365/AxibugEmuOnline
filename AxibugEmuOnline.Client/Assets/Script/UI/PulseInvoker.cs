namespace AxibugEmuOnline.Client.UI
{
    using System;

    public class PulseInvoker
    {
        private Action _action;
        private float _interval;
        private float _delay;
        private float _elapsedTime;
        private bool _isActive;
        private bool _isDelayed;

        public PulseInvoker(Action action, float delay, float interval)
        {
            _action = action;

            _delay = delay;
            _interval = interval;
        }

        public void SetActive()
        {
            _elapsedTime = 0f;
            _isActive = true;
            _isDelayed = true;
        }

        public void DisActive()
        {
            _isActive = false;
        }

        public void Update(float deltaTime)
        {
            if (!_isActive) return;

            _elapsedTime += deltaTime;

            if (_isDelayed)
            {
                if (_elapsedTime >= _delay)
                {
                    _elapsedTime -= _delay;
                    _isDelayed = false;
                    _action?.Invoke();
                }
            }
            else
            {
                if (_elapsedTime >= _interval)
                {
                    _elapsedTime -= _interval;
                    _action?.Invoke();
                }
            }
        }
    }
}
