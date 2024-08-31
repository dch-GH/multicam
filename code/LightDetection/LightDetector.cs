namespace Ironsim;

public sealed class LightDetector : Component
{
	[Property] private CameraComponent _mainCamera;
	[Property] private CameraComponent _lightcamera;

	[Property, Range( 0, 255 )]
	private int _brightnessThreshold = 165;

	public bool IsInShadow { get; private set; }

	private HashSet<GameObject> _pointLights;
	private bool _dirty;
	private int _bufferTextureWidth = 1;
	private int _bufferTextureHeight = 1;
	private Color32[]? _buffer;
	private RealTimeSince _sincePrint;

	protected override void OnStart()
	{
		_pointLights = Scene.GetAllObjects( true ).Where( x => x.Components.TryGet<PointLight>( out var _ ) ).ToHashSet();
		_lightcamera.AddHookBeforeOverlay( "lightDetection", 0, LightCameraRenderEffect );
		_buffer = new Color32[_bufferTextureWidth * _bufferTextureHeight];
	}

	protected override void OnPreRender()
	{
		_dirty = false;

		// TODO: Find a better way to do this.
		var nearestLight = _pointLights.OrderBy( x => Vector3.DistanceBetween( Transform.Position, x.Transform.Position ) ).First();

		// TODO: With tags: solid, ignore things like glass or other non-light blockers.
		// Also trace to corner of bounds + center. Do trace to center first, if blocked, early bail.
		var tr = Scene.Trace.Ray( nearestLight.Transform.Position, Transform.Position ).IgnoreGameObjectHierarchy( nearestLight ).IgnoreGameObjectHierarchy( GameObject ).Run();

		if ( tr.Hit )
		{
			// The player is probably in a shadow behind something like a wall or a prop.
			_buffer = null;
			IsInShadow = true;
			return;
		}

		// Now we get the light camera's texture.
		_dirty = true;

		if ( _buffer is null )
			return;

		for ( int i = 0; i < _buffer.Length; i++ )
		{
			var col = _buffer[i];
			if ( col.RgbaInt <= 255 )
				continue;

			var average = (col.r + col.g + col.b) / 3;
			IsInShadow = average <= _brightnessThreshold;
		}

		if ( _sincePrint >= 1 )
			Log.Info( $"In shadows: {IsInShadow}" );
	}

	private void LightCameraRenderEffect( SceneCamera sc )
	{
		if ( !_dirty )
			return;

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
