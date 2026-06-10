using Sandbox.UI;

namespace sGBA;

public sealed class SelectionRing : Panel
{
	private const float RoundedStrokeWidth = 10f;
	private const float CirclePadding = 3f;
	private const float RoundedGap = 5f;
	private const float RoundedCornerRadius = 17f;
	private const float SampleStep = 2f;
	private const float AnimationDuration = 5.2f;
	private static readonly float[] GradientStops = [0f, 0.14f, 0.28f, 0.45f, 0.62f, 0.78f, 1f];
	private static readonly Color[] GradientColors =
	[
		new Color( 0.075f, 0.592f, 1f ),
		new Color( 0.373f, 0.82f, 1f ),
		new Color( 0.075f, 0.592f, 1f ),
		new Color( 0f, 0.357f, 1f ),
		new Color( 0.075f, 0.592f, 1f ),
		new Color( 0.373f, 0.82f, 1f ),
		new Color( 0.075f, 0.592f, 1f )
	];

	[Parameter]
	public bool Active { get; set; } = true;

	[Parameter]
	public bool Circle { get; set; }

	[Parameter]
	public float StrokeWidth { get; set; } = RoundedStrokeWidth;

	[Parameter]
	public float CornerRadius { get; set; } = RoundedCornerRadius;

	[Parameter]
	public float Gap { get; set; } = RoundedGap;

	[Parameter]
	public bool Outset { get; set; }

	[Parameter]
	public bool FlatTop { get; set; }

	[Parameter]
	public float Alpha { get; set; } = 1f;

	public override void Tick()
	{
		base.Tick();
		MarkRenderDirty();
	}

	public override void OnDraw()
	{
		base.OnDraw();

		if ( !Active ) return;

		float width = Box.Rect.Width;
		float height = Box.Rect.Height;
		if ( width <= 0f || height <= 0f ) return;
		float alpha = Math.Clamp( Alpha, 0f, 1f );
		if ( alpha <= 0f ) return;

		float scale = MathF.Max( 0.001f, ScaleToScreen );
		if ( Circle )
		{
			DrawCircleRing( width, height, scale, alpha );
			return;
		}

		if ( FlatTop )
		{
			DrawFlatTopRing( width, height, scale, alpha );
			return;
		}

		DrawRoundedRing( width, height, scale, alpha );
	}

	private void DrawFlatTopRing( float width, float height, float scale, float alpha )
	{
		float originX = Box.Rect.Left;
		float originY = Box.Rect.Top;
		float centerX = originX + width * 0.5f;
		float centerY = originY + height * 0.5f;
		float halfDot = MathF.Max( 1f, StrokeWidth ) * scale * 0.5f;
		float centerOffset = Outset ? halfDot + MathF.Max( 0f, Gap ) * scale : halfDot;
		float radius = Outset
			? MathF.Max( 0f, CornerRadius ) * scale + centerOffset
			: (MathF.Max( 0f, CornerRadius ) + MathF.Max( 0f, Gap )) * scale;
		var center = new Vector2( centerX, centerY );
		float phase = RealTime.Now / AnimationDuration;

		float left = Outset ? originX - centerOffset : originX + centerOffset;
		float top = Outset ? originY - centerOffset : originY + centerOffset;
		float right = Outset ? originX + width + centerOffset : originX + width - centerOffset;
		float bottom = Outset ? originY + height + centerOffset : originY + height - centerOffset;
		float bottomRadius = MathF.Min( radius, MathF.Min( MathF.Abs( right - left ) * 0.5f, MathF.Abs( bottom - top ) ) );

		float sampleStep = SampleStep * scale;
		DrawHorizontal( left, right, top, center, phase, halfDot, sampleStep, alpha );
		DrawVertical( right, top, bottom - bottomRadius, center, phase, halfDot, sampleStep, alpha );
		DrawCorner( right - bottomRadius, bottom - bottomRadius, bottomRadius, 0f, 90f, center, phase, halfDot, sampleStep, alpha );
		DrawHorizontal( right - bottomRadius, left + bottomRadius, bottom, center, phase, halfDot, sampleStep, alpha );
		DrawCorner( left + bottomRadius, bottom - bottomRadius, bottomRadius, 90f, 180f, center, phase, halfDot, sampleStep, alpha );
		DrawVertical( left, bottom - bottomRadius, top, center, phase, halfDot, sampleStep, alpha );
	}

	private void DrawRoundedRing( float width, float height, float scale, float alpha )
	{
		float originX = Box.Rect.Left;
		float originY = Box.Rect.Top;
		float centerX = originX + width * 0.5f;
		float centerY = originY + height * 0.5f;
		float halfDot = MathF.Max( 1f, StrokeWidth ) * scale * 0.5f;
		float centerOffset = Outset ? halfDot + MathF.Max( 0f, Gap ) * scale : halfDot;
		float radius = Outset
			? MathF.Max( 0f, CornerRadius ) * scale + centerOffset
			: (MathF.Max( 0f, CornerRadius ) + MathF.Max( 0f, Gap )) * scale;
		var center = new Vector2( centerX, centerY );
		float phase = RealTime.Now / AnimationDuration;

		float left = Outset ? originX - centerOffset : originX + centerOffset;
		float top = Outset ? originY - centerOffset : originY + centerOffset;
		float right = Outset ? originX + width + centerOffset : originX + width - centerOffset;
		float bottom = Outset ? originY + height + centerOffset : originY + height - centerOffset;

		float sampleStep = SampleStep * scale;
		DrawHorizontal( left + radius, right - radius, top, center, phase, halfDot, sampleStep, alpha );
		DrawCorner( right - radius, top + radius, radius, -90f, 0f, center, phase, halfDot, sampleStep, alpha );
		DrawVertical( right, top + radius, bottom - radius, center, phase, halfDot, sampleStep, alpha );
		DrawCorner( right - radius, bottom - radius, radius, 0f, 90f, center, phase, halfDot, sampleStep, alpha );
		DrawHorizontal( right - radius, left + radius, bottom, center, phase, halfDot, sampleStep, alpha );
		DrawCorner( left + radius, bottom - radius, radius, 90f, 180f, center, phase, halfDot, sampleStep, alpha );
		DrawVertical( left, bottom - radius, top + radius, center, phase, halfDot, sampleStep, alpha );
		DrawCorner( left + radius, top + radius, radius, 180f, 270f, center, phase, halfDot, sampleStep, alpha );
	}

	private void DrawCircleRing( float width, float height, float scale, float alpha )
	{
		float halfDot = MathF.Max( 1f, StrokeWidth ) * scale * 0.5f;
		var center = new Vector2( Box.Rect.Left + width * 0.5f, Box.Rect.Top + height * 0.5f );
		float radius = Outset
			? MathF.Min( width, height ) * 0.5f + MathF.Max( 0f, Gap ) * scale + halfDot
			: MathF.Min( width, height ) * 0.5f - halfDot - CirclePadding * scale;
		float circumference = MathF.PI * 2f * radius;
		float sampleStep = SampleStep * scale;
		int steps = Math.Max( 1, (int)MathF.Ceiling( circumference / sampleStep ) );
		float phase = RealTime.Now / AnimationDuration;

		for ( int step = 0; step <= steps; step++ )
		{
			float amount = step / (float)steps;
			float radians = amount * MathF.PI * 2f;
			var point = new Vector2( center.x + MathF.Cos( radians ) * radius, center.y + MathF.Sin( radians ) * radius );
			Panel.Draw.Circle( point, halfDot, WithAlpha( GradientColor( amount + phase ), alpha ) );
		}
	}

	private static void DrawHorizontal( float startX, float endX, float pointY, Vector2 center, float phase, float halfDot, float sampleStep, float alpha )
	{
		float direction = MathF.Sign( endX - startX );
		float distance = MathF.Abs( endX - startX );
		int steps = Math.Max( 1, (int)MathF.Ceiling( distance / sampleStep ) );

		for ( int step = 0; step <= steps; step++ )
		{
			float pointX = startX + direction * MathF.Min( step * sampleStep, distance );
			DrawDot( new Vector2( pointX, pointY ), center, phase, halfDot, alpha );
		}
	}

	private static void DrawVertical( float pointX, float startY, float endY, Vector2 center, float phase, float halfDot, float sampleStep, float alpha )
	{
		float direction = MathF.Sign( endY - startY );
		float distance = MathF.Abs( endY - startY );
		int steps = Math.Max( 1, (int)MathF.Ceiling( distance / sampleStep ) );

		for ( int step = 0; step <= steps; step++ )
		{
			float pointY = startY + direction * MathF.Min( step * sampleStep, distance );
			DrawDot( new Vector2( pointX, pointY ), center, phase, halfDot, alpha );
		}
	}

	private static void DrawCorner( float centerX, float centerY, float radius, float startDegrees, float endDegrees, Vector2 center, float phase, float halfDot, float sampleStep, float alpha )
	{
		float arcLength = MathF.Abs( endDegrees - startDegrees ) * MathF.PI / 180f * radius;
		int steps = Math.Max( 1, (int)MathF.Ceiling( arcLength / sampleStep ) );

		for ( int step = 0; step <= steps; step++ )
		{
			float amount = step / (float)steps;
			float degrees = startDegrees + (endDegrees - startDegrees) * amount;
			float radians = degrees * MathF.PI / 180f;
			var point = new Vector2( centerX + MathF.Cos( radians ) * radius, centerY + MathF.Sin( radians ) * radius );
			DrawDot( point, center, phase, halfDot, alpha );
		}
	}

	private static void DrawDot( Vector2 point, Vector2 center, float phase, float halfDot, float alpha )
	{
		Panel.Draw.Circle( point, halfDot, ColorAt( point, center, phase, alpha ) );
	}

	private static Color ColorAt( Vector2 point, Vector2 center, float phase, float alpha )
	{
		float angle = MathF.Atan2( point.y - center.y, point.x - center.x );
		float offset = (angle + MathF.PI) / (MathF.PI * 2f);
		return WithAlpha( GradientColor( offset + phase ), alpha );
	}

	private static Color WithAlpha( Color color, float alpha )
	{
		return new Color( color.r, color.g, color.b, color.a * alpha );
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

	protected override int BuildHash()
	{
		return HashCode.Combine( Active, Circle, StrokeWidth, CornerRadius, Gap, Outset, FlatTop, Alpha );
	}
}
