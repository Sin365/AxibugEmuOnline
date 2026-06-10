namespace sGBA;

internal sealed class HomeToastState
{
	private const float Duration = 2.8f;
	private const float FadeDuration = 0.45f;

	public string Title { get; private set; }
	public string Message { get; private set; }
	public string Icon { get; private set; } = "info";
	public string ColorClass { get; private set; } = "blue";
	public float Age { get; private set; }
	public float Remaining { get; private set; }

	public bool IsRunning => Remaining > 0f;

	public int RenderHash => HashCode.Combine( Title, Message, Icon, ColorClass, (int)(Age * 100f), (int)(Remaining * 100f) );

	public void Show( string title, string message, string icon, string colorClass )
	{
		Title = title;
		Message = message;
		Icon = icon;
		ColorClass = colorClass;
		Age = 0f;
		Remaining = Duration;
	}

	public void Tick( float delta )
	{
		if ( Remaining <= 0f )
			return;

		Age += delta;
		Remaining = MathF.Max( 0f, Remaining - delta );
	}

	public bool IsVisible( bool blocked )
	{
		return IsRunning && !blocked;
	}

	public string GetStyle( bool visible )
	{
		float fadeIn = MathF.Min( 1f, Age / FadeDuration );
		float fadeOut = MathF.Min( 1f, Remaining / FadeDuration );
		float enterProgress = 1f - MathF.Pow( 1f - fadeIn, 3f );
		float exitProgress = 1f - fadeOut;
		float opacity = visible ? MathF.Min( enterProgress, fadeOut ) : 0f;
		float offsetY = -110f * (1f - enterProgress) - 18f * exitProgress;

		return $"opacity: {opacity}; transform: translateY({offsetY}px);";
	}
}
