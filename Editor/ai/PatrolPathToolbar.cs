

namespace Ironsim.Editor;

public sealed class PatrolPathToolbar : ToolbarGroup
{
	public PatrolPathToolbar( Widget parent, string title, string icon ) : base( parent, title, icon )
	{
		ToolTip = "Patrol Tools";
	}

	public override void Build()
	{
	}

	[Event( "tools.headerbar.build", Priority = 110 )]
	public static void OnBuildHeaderToolbar( HeadBarEvent e )
	{
		e.RightCenter.Add( new PatrolPathToolbar( null, "Patrol Tools", "box" ) );
		e.RightCenter.AddSpacingCell( 8f );
	}
}
