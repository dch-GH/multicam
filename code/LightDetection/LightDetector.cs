namespace Ironsim;

public sealed class LightDetector : Component
{
	public bool IsInShadow { get; private set; }

	[Property] private PrefabFile _lightPlatePrefab;
	[Property] private Vector3 _offset;

	[Property, Range( 0, 255 )]
	private int _brightnessThreshold = 165;
	private CameraComponent _lightcamera;
	private GameObject _lightPlate;

	private int _bufferTextureWidth = 2;
	private int _bufferTextureHeight = 2;
	private Color32[]? _buffer;

	protected override void OnStart()
	{
		_lightPlate = GameObject.Clone( _lightPlatePrefab );
		_lightPlate.BreakFromPrefab();

		_lightcamera = _lightPlate.Components.Get<CameraComponent>( FindMode.EverythingInSelfAndChildren );
		_lightcamera.AddHookAfterOpaque( "lightDetection", int.MinValue, LightCameraRenderEffect );
		_buffer = new Color32[_bufferTextureWidth * _bufferTextureHeight];
	}

	protected override void OnPreRender()
	{
		_lightPlate.Transform.Position = Transform.Position + _offset;
		// _lightcamera.Transform.Rotation = Rotation.Identity;

		// Assume we aren't in shadows first.
		IsInShadow = false;

		for ( int i = 0; i < _buffer.Length; i++ )
		{
			var col = _buffer[i];
			if ( col.RgbaInt <= 255 )
				continue;

			var average = (col.r + col.g + col.b) / 3;
			IsInShadow = average <= _brightnessThreshold;
			DebugTextureView.InShadow = IsInShadow;
		}
	}

	private void LightCameraRenderEffect( SceneCamera sc )
	{
		if ( _buffer is null )
			return;

		var attr = new RenderAttributes();
		Graphics.GrabFrameTexture( "ColorBuffer", renderAttributes: attr );
		var texture = attr.GetTexture( "ColorBuffer" );

		DebugTextureView._img.Style.BackgroundImage = texture;
		DebugTextureView._img.Style.Width = texture.Width;
		DebugTextureView._img.Style.Height = texture.Height;

		texture.GetPixels<Color32>( (texture.Width / 2, texture.Height / 2, _bufferTextureWidth, _bufferTextureHeight), 0, 0, _buffer, ImageFormat.RGBA8888 );
	}
}
