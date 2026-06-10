using Sandbox.UI;

namespace sGBA;

public sealed class MarqueeText : Panel
{
	private static readonly Color DefaultTextColor = new( 0.12f, 0.46f, 0.68f, 1f );
	private const float ExtraTextWidth = 8f;
	private string _lastTitle;
	private float _startedAt;
	private float _lastScale;
	private float _lastClipWidth;
	private float _textWidth;
	private Label _primary;
	private Label _duplicate;

	[Parameter]
	public string Title { get; set; }

	[Parameter]
	public float FontSize { get; set; } = 30f;

	[Parameter]
	public float Speed { get; set; } = 92f;

	[Parameter]
	public float Gap { get; set; } = 88f;

	[Parameter]
	public float StartHold { get; set; } = 0.65f;

	[Parameter]
	public string FontName { get; set; } = "Poppins";

	[Parameter]
	public int FontWeight { get; set; } = 400;

	public override void Tick()
	{
		base.Tick();

		UpdateLabels();
	}

	private void UpdateLabels()
	{
		if ( string.IsNullOrWhiteSpace( Title ) )
		{
			SetLabelVisible( false );
			return;
		}

		EnsureLabels();

		float scale = MathF.Max( 0.001f, ScaleToScreen );
		float width = Box.Rect.Width / scale;
		float height = Box.Rect.Height / scale;
		if ( width <= 0f || height <= 0f )
		{
			SetLabelVisible( false );
			return;
		}

		if ( _lastTitle != Title || MathF.Abs( _lastScale - scale ) > 0.001f || MathF.Abs( _lastClipWidth - width ) > 0.001f )
		{
			_lastTitle = Title;
			_lastScale = scale;
			_lastClipWidth = width;
			_startedAt = RealTime.Now;
			_textWidth = MeasureTitleWidth( scale );
			_primary.Text = Title;
			_duplicate.Text = Title;
			_primary.Style.Width = _textWidth;
			_duplicate.Style.Width = _textWidth;
		}
		float textWidth = MathF.Max( 1f, _textWidth );
		_primary.Style.Width = textWidth;
		_duplicate.Style.Width = textWidth;
		_primary.Style.Top = 0f;
		_duplicate.Style.Top = 0f;
		_primary.Style.Height = height;
		_duplicate.Style.Height = height;

		float spacing = MathF.Max( Gap, width * 0.35f );
		float span = textWidth + spacing;
		float scrollSpeed = MathF.Max( 1f, Speed );
		float scrollDuration = span / scrollSpeed;
		float elapsed = PositiveModulo( RealTime.Now - _startedAt, StartHold + scrollDuration );
		float offset = elapsed <= StartHold ? 0f : (elapsed - StartHold) * scrollSpeed;

		_primary.Style.Left = -offset;
		_duplicate.Style.Left = span - offset;
		SetLabelVisible( true );
	}

	private void EnsureLabels()
	{
		if ( _primary != null && _duplicate != null )
			return;

		_primary = AddChild<Label>();
		_duplicate = AddChild<Label>();
		ConfigureLabel( _primary );
		ConfigureLabel( _duplicate );
	}

	private void ConfigureLabel( Label label )
	{
		label.Selectable = false;
		label.Style.Position = PositionMode.Absolute;
		label.Style.FontFamily = FontName;
		label.Style.FontSize = FontSize;
		label.Style.FontWeight = FontWeight;
		label.Style.LineHeight = 38f;
		label.Style.FontColor = DefaultTextColor;
		label.Style.AlignItems = Align.Center;
		label.Style.JustifyContent = Justify.Center;
		label.Style.WhiteSpace = WhiteSpace.NoWrap;
		label.Style.TextAlign = TextAlign.Left;
	}

	private void SetLabelVisible( bool visible )
	{
		if ( _primary != null )
			_primary.Style.Display = visible ? DisplayMode.Flex : DisplayMode.None;

		if ( _duplicate != null )
			_duplicate.Style.Display = visible ? DisplayMode.Flex : DisplayMode.None;
	}

	private float MeasureTitleWidth( float scale )
	{
		var scope = new TextRendering.Scope( Title, DefaultTextColor, FontSize * scale, FontName, FontWeight );
		return MathF.Max( 1f, scope.Measure().x / scale + ExtraTextWidth );
	}

	private static float PositiveModulo( float value, float length )
	{
		if ( length <= 0f )
			return 0f;

		value %= length;
		return value < 0f ? value + length : value;
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( Title, FontSize, Speed, Gap, StartHold, FontName, FontWeight );
	}
}
