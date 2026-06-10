using Sandbox.UI;

namespace sGBA;

public class InputHint : Panel
{
	private Texture _lastGlyph;

	[Parameter] public string Action { get; set; }
	[Parameter] public InputGlyphSize GlyphSize { get; set; } = InputGlyphSize.Small;

	public override void Tick()
	{
		if ( string.IsNullOrEmpty( Action ) )
			return;

		var glyph = Input.GetGlyph( Action, GlyphSize, false );
		if ( !glyph.IsValid() || glyph == _lastGlyph )
			return;

		_lastGlyph = glyph;
		Style.SetBackgroundImage( glyph );
		Style.AspectRatio = (float)glyph.Width / glyph.Height;
	}
}
