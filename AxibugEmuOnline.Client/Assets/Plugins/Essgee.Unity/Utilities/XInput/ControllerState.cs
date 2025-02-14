//namespace Essgee.Utilities.XInput
//{
//	public class ControllerState
//	{
//		public Buttons Buttons { get; set; }
//		public ThumbstickPosition LeftThumbstick { get; set; }
//		public ThumbstickPosition RightThumbstick { get; set; }
//		public float LeftTrigger { get; set; }
//		public float RightTrigger { get; set; }

//		public bool IsConnected { get; set; }
//		public int UserIndex { get; set; }

//		public ControllerState()
//		{
//			Buttons = Buttons.None;
//			LeftThumbstick = new ThumbstickPosition(0.0f, 0.0f);
//			RightThumbstick = new ThumbstickPosition(0.0f, 0.0f);
//			LeftTrigger = 0.0f;
//			RightTrigger = 0.0f;

//			IsConnected = false;
//			UserIndex = -1;
//		}

//		public bool IsAnyUpDirectionPressed()
//		{
//			return IsDPadUpPressed() || LeftThumbstick.Y > 0.5f;
//		}

//		public bool IsAnyDownDirectionPressed()
//		{
//			return IsDPadDownPressed() || LeftThumbstick.Y < -0.5f;
//		}

//		public bool IsAnyLeftDirectionPressed()
//		{
//			return IsDPadLeftPressed() || LeftThumbstick.X < -0.5f;
//		}

//		public bool IsAnyRightDirectionPressed()
//		{
//			return IsDPadRightPressed() || LeftThumbstick.X > 0.5f;
//		}

//		public bool IsDPadUpPressed()
//		{
//			return Buttons.HasFlag(Buttons.DPadUp);
//		}

//		public bool IsDPadDownPressed()
//		{
//			return Buttons.HasFlag(Buttons.DPadDown);
//		}

//		public bool IsDPadLeftPressed()
//		{
//			return Buttons.HasFlag(Buttons.DPadLeft);
//		}

//		public bool IsDPadRightPressed()
//		{
//			return Buttons.HasFlag(Buttons.DPadRight);
//		}

//		public bool IsStartPressed()
//		{
//			return Buttons.HasFlag(Buttons.Start);
//		}

//		public bool IsBackPressed()
//		{
//			return Buttons.HasFlag(Buttons.Back);
//		}

//		public bool IsLeftThumbPressed()
//		{
//			return Buttons.HasFlag(Buttons.LeftThumb);
//		}

//		public bool IsRightThumbPressed()
//		{
//			return Buttons.HasFlag(Buttons.RightThumb);
//		}

//		public bool IsLeftShoulderPressed()
//		{
//			return Buttons.HasFlag(Buttons.LeftShoulder);
//		}

//		public bool IsRightShoulderPressed()
//		{
//			return Buttons.HasFlag(Buttons.RightShoulder);
//		}

//		public bool IsAPressed()
//		{
//			return Buttons.HasFlag(Buttons.A);
//		}

//		public bool IsBPressed()
//		{
//			return Buttons.HasFlag(Buttons.B);
//		}

//		public bool IsXPressed()
//		{
//			return Buttons.HasFlag(Buttons.X);
//		}

//		public bool IsYPressed()
//		{
//			return Buttons.HasFlag(Buttons.Y);
//		}
//	}
//}
