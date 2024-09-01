using Sandbox.Diagnostics;

namespace Ironsim;

public sealed class LightDetector : Component, Component.ITriggerListener
{
	public bool IsInShadow { get; private set; }

	[Property] private PrefabFile _lightPlatePrefab = null;
	[Property] private Vector3 _offset = 0;
	[Property, Range( 0, 255 )] private int _brightnessThreshold = 165;
	[Property, Description( "Display the light detection debug viewer." )] private bool _debug = false;

	private CameraComponent _lightcamera;
	private GameObject _lightPlane;
	private Collider? _currentShadowVolume;
	private int _bufferTextureWidth = 2;
	private int _bufferTextureHeight = 2;
	private Color32[] _lightBuffer;
	private LightDebugView? _debugView;

	protected override void OnStart()
	{
		_lightPlane = GameObject.Clone( _lightPlatePrefab );
		_lightPlane.BreakFromPrefab();

		_lightcamera = _lightPlane.Components.Get<CameraComponent>( FindMode.EverythingInSelfAndChildren );
		Assert.NotNull( _lightcamera );

		_lightcamera.AddHookAfterOpaque( "lightDetection", int.MinValue, LightCameraRenderEffect );
		_lightBuffer = new Color32[_bufferTextureWidth * _bufferTextureHeight];
	}

	protected override void OnPreRender()
	{
		_lightPlane.Transform.Position = Transform.Position + _offset;

		// Assume we aren't in the shadows by default.
		IsInShadow = false;

		for ( int i = 0; i < _lightBuffer.Length; i++ )
		{
			var col = _lightBuffer[i];

			// 255 actually means "black" in this case, ignore it.
			if ( col.RgbaInt <= 255 )
				continue;

			var average = (col.r + col.g + col.b) / 3;
			IsInShadow = average <= _brightnessThreshold || _currentShadowVolume is not null;
		}

		if ( _debug && _debugView is null )
			_debugView = Scene.GetAllComponents<LightDebugView>().First();

		if ( _debug && _debugView is not null )
			_debugView.InShadow = IsInShadow;

		_debugView?.SetClass( "enabled", _debug );
	}

	private void LightCameraRenderEffect( SceneCamera sc )
	{
		var attr = new RenderAttributes();
		Graphics.GrabFrameTexture( "ColorBuffer", renderAttributes: attr );
		var texture = attr.GetTexture( "ColorBuffer" );

		if ( _debug && _debugView is not null )
		{
			_debugView.Image.Style.BackgroundImage = texture;
			_debugView.Image.Style.Width = texture.Width;
			_debugView.Image.Style.Height = texture.Height;
		}

		var x = (texture.Width / 2) - _bufferTextureWidth / 2;
		var y = (texture.Height / 2) - _bufferTextureHeight / 2;
		texture.GetPixels<Color32>( srcRect: (x, y, _bufferTextureWidth, _bufferTextureHeight), slice: 0, mip: 0, dstData: _lightBuffer, ImageFormat.RGBA8888 );
	}

	public void OnTriggerEnter( Collider other )
	{
		if ( other.Tags.Has( Tag.ShadowVolume ) )
			_currentShadowVolume = other;
	}

	public void OnTriggerExit( Collider other )
	{
		if ( other == _currentShadowVolume )
			_currentShadowVolume = null;
	}
}
