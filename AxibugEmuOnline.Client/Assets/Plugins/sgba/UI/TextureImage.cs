using Sandbox.UI;

namespace sGBA;

public sealed class TextureImage : Panel
{
	private Texture _texture;

	[Parameter]
	public Texture Texture
	{
		get => _texture;
		set
		{
			if ( _texture == value ) return;
			_texture = value;
			Style.SetBackgroundImage( _texture );
		}
	}

	protected override void OnParametersSet()
	{
		base.OnParametersSet();
		Style.SetBackgroundImage( _texture );
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( _texture );
	}
}
