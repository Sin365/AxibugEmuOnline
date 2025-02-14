using System;

namespace Essgee.Utilities.XInput
{
    public class Controller
    {
        XInputState inputStatesCurrent, inputStatesPrev;
        bool timedVibrationEnabled;
        DateTime vibrationStopTime;

        public bool IsConnected { get; private set; }
        public int UserIndex { get; private set; }

        public Controller(int index)
        {
            inputStatesCurrent = inputStatesPrev = new XInputState();
            timedVibrationEnabled = false;
            vibrationStopTime = DateTime.Now;

            IsConnected = false;
            UserIndex = index;
        }

        public void Update()
        {
            XInputState newInputState = new XInputState();
            Errors result = (Errors)NativeMethods.GetState(UserIndex, ref newInputState);
            if (result == Errors.Success)
            {
                IsConnected = true;
                inputStatesPrev = inputStatesCurrent;
                inputStatesCurrent = newInputState;

                if ((inputStatesCurrent.Gamepad.sThumbLX < XInputGamepad.LeftThumbDeadzone && inputStatesCurrent.Gamepad.sThumbLX > -XInputGamepad.LeftThumbDeadzone) &&
                    (inputStatesCurrent.Gamepad.sThumbLY < XInputGamepad.LeftThumbDeadzone && inputStatesCurrent.Gamepad.sThumbLY > -XInputGamepad.LeftThumbDeadzone))
                {
                    inputStatesCurrent.Gamepad.sThumbLX = inputStatesCurrent.Gamepad.sThumbLY = 0;
                }

                if ((inputStatesCurrent.Gamepad.sThumbRX < XInputGamepad.RightThumbDeadzone && inputStatesCurrent.Gamepad.sThumbRX > -XInputGamepad.RightThumbDeadzone) &&
                    (inputStatesCurrent.Gamepad.sThumbRY < XInputGamepad.RightThumbDeadzone && inputStatesCurrent.Gamepad.sThumbRY > -XInputGamepad.RightThumbDeadzone))
                {
                    inputStatesCurrent.Gamepad.sThumbRX = inputStatesCurrent.Gamepad.sThumbRY = 0;
                }

                if (inputStatesCurrent.Gamepad.bLeftTrigger < XInputGamepad.TriggerThreshold) inputStatesCurrent.Gamepad.bLeftTrigger = 0;
                if (inputStatesCurrent.Gamepad.bRightTrigger < XInputGamepad.TriggerThreshold) inputStatesCurrent.Gamepad.bRightTrigger = 0;

                if (timedVibrationEnabled && DateTime.Now >= vibrationStopTime)
                {
                    timedVibrationEnabled = false;
                    Vibrate(0.0f, 0.0f);
                }
            }
            else if (result == Errors.DeviceNotConnected)
            {
                IsConnected = false;
            }
            else
                throw new Exception(string.Format("Error code {0}", (int)result));
        }

        //public ControllerState GetControllerState()
        //{
        //	return new ControllerState
        //	{
        //		Buttons = inputStatesCurrent.Gamepad.Buttons,
        //		LeftThumbstick = new ThumbstickPosition(inputStatesCurrent.Gamepad.sThumbLX / 32768.0f, inputStatesCurrent.Gamepad.sThumbLY / 32768.0f),
        //		RightThumbstick = new ThumbstickPosition(inputStatesCurrent.Gamepad.sThumbRX / 32768.0f, inputStatesCurrent.Gamepad.sThumbRY / 32768.0f),
        //		LeftTrigger = (inputStatesCurrent.Gamepad.bLeftTrigger / 255.0f),
        //		RightTrigger = (inputStatesCurrent.Gamepad.bRightTrigger / 255.0f)
        //	};
        //}

        public bool IsDPadUpPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.DPadUp);
        }

        public bool IsDPadDownPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.DPadDown);
        }

        public bool IsDPadLeftPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.DPadLeft);
        }

        public bool IsDPadRightPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.DPadRight);
        }

        public bool IsStartPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.Start);
        }

        public bool IsBackPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.Back);
        }

        public bool IsLeftThumbPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.LeftThumb);
        }

        public bool IsRightThumbPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.RightThumb);
        }

        public bool IsLeftShoulderPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.LeftShoulder);
        }

        public bool IsRightShoulderPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.RightShoulder);
        }

        public bool IsAPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.A);
        }

        public bool IsBPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.B);
        }

        public bool IsXPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.X);
        }

        public bool IsYPressed()
        {
            return inputStatesCurrent.Gamepad.Buttons.HasFlag(Buttons.Y);
        }

        public void Vibrate(float leftMotor, float rightMotor)
        {
            XInputVibration vibrationState = new XInputVibration();
            vibrationState.wLeftMotorSpeed = (ushort)(leftMotor * 65535.0f);
            vibrationState.wRightMotorSpeed = (ushort)(rightMotor * 65535.0f);
            NativeMethods.SetState(UserIndex, ref vibrationState);
        }

        public void Vibrate(float leftMotor, float rightMotor, TimeSpan duration)
        {
            Vibrate(leftMotor, rightMotor);

            vibrationStopTime = DateTime.Now.Add(duration);
            timedVibrationEnabled = true;
        }
    }

    public class ThumbstickPosition
    {
        public float X { get; private set; }
        public float Y { get; private set; }

        public ThumbstickPosition(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "({0}, {1})", X, Y);
        }
    }
}
