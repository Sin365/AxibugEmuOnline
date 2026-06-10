namespace sGBA;

public sealed class FocusInput
{
	public const float DefaultStickDeadzone = 0.5f;
	public const float MouseWakeThreshold = 5f;
	public const float NavStickDeadzone = 0.1f;
	public const float DefaultRepeatDelay = 0.4f;
	public const float DefaultRepeatRate = 0.08f;

	public readonly struct StickEdges
	{
		public bool Up { get; init; }
		public bool Down { get; init; }
		public bool Left { get; init; }
		public bool Right { get; init; }

		public bool Any => Up || Down || Left || Right;
	}

	public readonly struct NavTriggers
	{
		public bool Up { get; init; }
		public bool Down { get; init; }
		public bool Left { get; init; }
		public bool Right { get; init; }

		public bool Any => Up || Down || Left || Right;
	}

	public bool UseGamepad { get; private set; }

	private bool _edgeUp, _edgeDown, _edgeLeft, _edgeRight;
	private bool _navUpHeld, _navDownHeld, _navLeftHeld, _navRightHeld;
	private float _upHold, _upNext;
	private float _downHold, _downNext;
	private float _leftHold, _leftNext;
	private float _rightHold, _rightNext;

	public void Begin( bool useGamepad )
	{
		UseGamepad = useGamepad;
		ResetTransientState();
		Mouse.Visibility = useGamepad ? MouseVisibility.Hidden : MouseVisibility.Visible;
	}

	public void End()
	{
		var wasGamepad = UseGamepad;
		ResetTransientState();
		Mouse.Visibility = wasGamepad ? MouseVisibility.Hidden : MouseVisibility.Visible;
	}

	public StickEdges Tick( bool extraGamepadInput = false )
	{
		var sx = Input.GetAnalog( InputAnalog.LeftStickX );
		var sy = Input.GetAnalog( InputAnalog.LeftStickY );

		var up = sy < -DefaultStickDeadzone;
		var down = sy > DefaultStickDeadzone;
		var left = sx < -DefaultStickDeadzone;
		var right = sx > DefaultStickDeadzone;

		var edges = new StickEdges
		{
			Up = up && !_edgeUp,
			Down = down && !_edgeDown,
			Left = left && !_edgeLeft,
			Right = right && !_edgeRight,
		};

		_edgeUp = up; _edgeDown = down; _edgeLeft = left; _edgeRight = right;

		UpdateInputMode( extraGamepadInput || edges.Any );
		return edges;
	}

	public NavTriggers TickRepeating(
		string upAction = "GBA_Up",
		string downAction = "GBA_Down",
		string leftAction = "GBA_Left",
		string rightAction = "GBA_Right",
		float repeatDelay = DefaultRepeatDelay,
		float repeatRate = DefaultRepeatRate )
	{
		var sx = Input.GetAnalog( InputAnalog.LeftStickX );
		var sy = Input.GetAnalog( InputAnalog.LeftStickY );

		var up = sy < -NavStickDeadzone;
		var down = sy > NavStickDeadzone;
		var left = sx < -NavStickDeadzone;
		var right = sx > NavStickDeadzone;

		var upEdge = up && !_navUpHeld;
		var downEdge = down && !_navDownHeld;
		var leftEdge = left && !_navLeftHeld;
		var rightEdge = right && !_navRightHeld;

		_navUpHeld = up; _navDownHeld = down; _navLeftHeld = left; _navRightHeld = right;

		var triggers = new NavTriggers
		{
			Up = ResolveRepeat( upAction, upEdge, up, ref _upHold, ref _upNext, repeatDelay, repeatRate ),
			Down = ResolveRepeat( downAction, downEdge, down, ref _downHold, ref _downNext, repeatDelay, repeatRate ),
			Left = ResolveRepeat( leftAction, leftEdge, left, ref _leftHold, ref _leftNext, repeatDelay, repeatRate ),
			Right = ResolveRepeat( rightAction, rightEdge, right, ref _rightHold, ref _rightNext, repeatDelay, repeatRate ),
		};

		UpdateInputMode( triggers.Any );
		return triggers;
	}

	public void ForceGamepadMode()
	{
		if ( UseGamepad ) return;
		UseGamepad = true;
		Mouse.Visibility = MouseVisibility.Hidden;
	}

	public void ForceMouseMode()
	{
		if ( !UseGamepad ) return;
		UseGamepad = false;
		Mouse.Visibility = MouseVisibility.Visible;
		ResetTransientState();
	}

	private void UpdateInputMode( bool gamepadInputDetected )
	{
		if ( gamepadInputDetected && !UseGamepad )
		{
			UseGamepad = true;
			Mouse.Visibility = MouseVisibility.Hidden;
		}
		else if ( UseGamepad && Mouse.Delta.Length > MouseWakeThreshold )
		{
			UseGamepad = false;
			Mouse.Visibility = MouseVisibility.Visible;
		}
	}

	private void ResetTransientState()
	{
		_edgeUp = _edgeDown = _edgeLeft = _edgeRight = false;
		_navUpHeld = _navDownHeld = _navLeftHeld = _navRightHeld = false;
		_upHold = _upNext = 0f;
		_downHold = _downNext = 0f;
		_leftHold = _leftNext = 0f;
		_rightHold = _rightNext = 0f;
	}

	private static bool ResolveRepeat( string action, bool stickEdge, bool stickHeld,
		ref float holdTime, ref float nextRepeat, float repeatDelay, float repeatRate )
	{
		if ( Input.Pressed( action ) || stickEdge )
		{
			holdTime = 0f;
			nextRepeat = repeatDelay;
			return true;
		}

		if ( Input.Down( action ) || stickHeld )
		{
			holdTime += Time.Delta;
			if ( holdTime >= nextRepeat )
			{
				nextRepeat += repeatRate;
				return true;
			}
			return false;
		}

		holdTime = 0f;
		return false;
	}
}
