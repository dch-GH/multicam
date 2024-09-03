using Ironsim.AI;

namespace Ironsim.Editor;

public sealed class PatrolPathEditor : EditorTool<PatrolPathComponent>
{
	public static PatrolPathEditor Instance;
	public PatrolPathComponent Selected;
	private WidgetWindow _window;

	public PatrolPathEditor() : base()
	{
		Instance = this;
	}

	public override void OnSelectionChanged()
	{
		Selected = GetSelectedComponent<PatrolPathComponent>();
		if ( Selected is null )
			return;

		_window = new WidgetWindow( SceneOverlay, "Patrol Path Editor" );
		_window.MinimumWidth = 600;
		_window.MinimumHeight = 400;

		var layout = Layout.Column();
		layout.Margin = 6f;
		layout.Alignment = TextFlag.Top;
		_window.Layout = layout;
		AddOverlay( _window, TextFlag.RightCenter );

		var x = layout.Add( new BoolProperty( _window ) );
		x.Clicked += () =>
		{
			Log.Info( "hi" );
		};

	}

	public override void OnUpdate()
	{
		if ( Selected is null || !Selected.IsBeingEdited )
			return;

	}
}
