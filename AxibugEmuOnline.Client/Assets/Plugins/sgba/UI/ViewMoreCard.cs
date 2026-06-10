using Sandbox.UI;

namespace sGBA;

public sealed class ViewMoreCard : Panel
{
	private static readonly Color FaceColor = new( 0.98f, 0.985f, 0.995f, 0.98f );
	private static readonly Color IconColor = new( 0.22f, 0.24f, 0.28f, 1f );

	public override void Tick()
	{
		base.Tick();
		MarkRenderDirty();
	}

	public override void OnDraw()
	{
		base.OnDraw();

		float width = Box.Rect.Width;
		float height = Box.Rect.Height;
		if ( width <= 0f || height <= 0f ) return;

		float scale = MathF.Max( 0.001f, ScaleToScreen );
		float diameter = MathF.Min( width, height );
		var center = new Vector2( Box.Rect.Left + width * 0.5f, Box.Rect.Top + height * 0.5f );
		Draw.Circle( center, diameter * 0.5f, FaceColor );

		DrawGridIcon( center, diameter, scale );
	}

	private static void DrawGridIcon( Vector2 center, float diameter, float scale )
	{
		float iconSize = diameter * 0.42f;
		float gap = iconSize * 0.1f;
		float cellSize = (iconSize - gap) * 0.5f;
		float stroke = MathF.Max( 5f * scale, diameter * 0.032f );
		float radius = MathF.Max( 2f * scale, diameter * 0.018f );
		float startX = center.x - iconSize * 0.5f;
		float startY = center.y - iconSize * 0.5f;
		float step = cellSize + gap;

		DrawCell( startX, startY, cellSize, stroke, radius );
		DrawCell( startX + step, startY, cellSize, stroke, radius );
		DrawCell( startX, startY + step, cellSize, stroke, radius );
		DrawCell( startX + step, startY + step, cellSize, stroke, radius );
	}

	private static void DrawCell( float left, float top, float size, float stroke, float radius )
	{
		Draw.Rect( new Rect( left, top, size, stroke ), IconColor, radius );
		Draw.Rect( new Rect( left, top + size - stroke, size, stroke ), IconColor, radius );
		Draw.Rect( new Rect( left, top, stroke, size ), IconColor, radius );
		Draw.Rect( new Rect( left + size - stroke, top, stroke, size ), IconColor, radius );
	}
}
