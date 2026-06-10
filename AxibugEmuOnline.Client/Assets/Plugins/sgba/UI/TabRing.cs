using Sandbox.UI;

namespace sGBA;

public sealed class TabRing : Panel
{
	private const float BorderWidth = 4f;
	private const float CornerRadius = 8f;
	private const float SampleStep = 1.2f;
	private const float AnimationDuration = 5.2f;
	private static readonly float[] GradientStops = [0f, 0.14f, 0.28f, 0.45f, 0.62f, 0.78f, 1f];
	private static readonly Color[] GradientColors =
	[
		new Color( 0.094f, 0.596f, 1f ),
		new Color( 0.188f, 0.651f, 1f ),
		new Color( 0.094f, 0.596f, 1f ),
		new Color( 0f, 0.365f, 1f ),
		new Color( 0.094f, 0.596f, 1f ),
		new Color( 0.188f, 0.651f, 1f ),
		new Color( 0.094f, 0.596f, 1f )
	];

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
		DrawRoundedRing( width, height, scale );
	}

	private void DrawRoundedRing( float width, float height, float scale )
	{
		float originX = Box.Rect.Left;
		float originY = Box.Rect.Top;
		float centerX = originX + width * 0.5f;
		float centerY = originY + height * 0.5f;
		float halfDot = BorderWidth * scale * 0.5f;
		float radius = CornerRadius * scale + halfDot;
		var center = new Vector2( centerX, centerY );
		float phase = RealTime.Now / AnimationDuration;

		float left = originX + halfDot;
		float top = originY + halfDot;
		float right = originX + width - halfDot;
		float bottom = originY + height - halfDot;
		float sampleStep = SampleStep * scale;

		DrawHorizontal( left + radius, right - radius, top, center, phase, halfDot, sampleStep );
		DrawCorner( right - radius, top + radius, radius, -90f, 0f, center, phase, halfDot, sampleStep );
		DrawVertical( right, top + radius, bottom - radius, center, phase, halfDot, sampleStep );
		DrawCorner( right - radius, bottom - radius, radius, 0f, 90f, center, phase, halfDot, sampleStep );
		DrawHorizontal( right - radius, left + radius, bottom, center, phase, halfDot, sampleStep );
		DrawCorner( left + radius, bottom - radius, radius, 90f, 180f, center, phase, halfDot, sampleStep );
		DrawVertical( left, bottom - radius, top + radius, center, phase, halfDot, sampleStep );
		DrawCorner( left + radius, top + radius, radius, 180f, 270f, center, phase, halfDot, sampleStep );
	}

	private static void DrawHorizontal( float startX, float endX, float pointY, Vector2 center, float phase, float halfDot, float sampleStep )
	{
		float direction = MathF.Sign( endX - startX );
		float distance = MathF.Abs( endX - startX );
		int steps = Math.Max( 1, (int)MathF.Ceiling( distance / sampleStep ) );

		for ( int step = 0; step <= steps; step++ )
		{
			float pointX = startX + direction * MathF.Min( step * sampleStep, distance );
			DrawDot( new Vector2( pointX, pointY ), center, phase, halfDot );
		}
	}

	private static void DrawVertical( float pointX, float startY, float endY, Vector2 center, float phase, float halfDot, float sampleStep )
	{
		float direction = MathF.Sign( endY - startY );
		float distance = MathF.Abs( endY - startY );
		int steps = Math.Max( 1, (int)MathF.Ceiling( distance / sampleStep ) );

		for ( int step = 0; step <= steps; step++ )
		{
			float pointY = startY + direction * MathF.Min( step * sampleStep, distance );
			DrawDot( new Vector2( pointX, pointY ), center, phase, halfDot );
		}
	}

	private static void DrawCorner( float centerX, float centerY, float radius, float startDegrees, float endDegrees, Vector2 center, float phase, float halfDot, float sampleStep )
	{
		float arcLength = MathF.Abs( endDegrees - startDegrees ) * MathF.PI / 180f * radius;
		int steps = Math.Max( 1, (int)MathF.Ceiling( arcLength / sampleStep ) );

		for ( int step = 0; step <= steps; step++ )
		{
			float amount = step / (float)steps;
			float degrees = startDegrees + (endDegrees - startDegrees) * amount;
			float radians = degrees * MathF.PI / 180f;
			var point = new Vector2( centerX + MathF.Cos( radians ) * radius, centerY + MathF.Sin( radians ) * radius );
			DrawDot( point, center, phase, halfDot );
		}
	}

	private static void DrawDot( Vector2 point, Vector2 center, float phase, float halfDot )
	{
		Panel.Draw.Circle( point, halfDot, ColorAt( point, center, phase ) );
	}

	private static Color ColorAt( Vector2 point, Vector2 center, float phase )
	{
		float angle = MathF.Atan2( point.y - center.y, point.x - center.x );
		float offset = (angle + MathF.PI) / (MathF.PI * 2f);
		return GradientColor( offset + phase );
	}

	private static Color GradientColor( float offset )
	{
		offset -= MathF.Floor( offset );

		for ( int stopIndex = 0; stopIndex < GradientStops.Length - 1; stopIndex++ )
		{
			if ( offset < GradientStops[stopIndex] || offset > GradientStops[stopIndex + 1] ) continue;

			float amount = (offset - GradientStops[stopIndex]) / (GradientStops[stopIndex + 1] - GradientStops[stopIndex]);
			return Lerp( GradientColors[stopIndex], GradientColors[stopIndex + 1], amount );
		}

		return GradientColors[0];
	}

	private static Color Lerp( Color from, Color to, float amount )
	{
		amount = Math.Clamp( amount, 0f, 1f );
		return new Color(
			from.r + (to.r - from.r) * amount,
			from.g + (to.g - from.g) * amount,
			from.b + (to.b - from.b) * amount,
			1f );
	}
}
