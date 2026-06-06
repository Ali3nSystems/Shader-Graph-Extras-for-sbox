namespace Editor.ShaderGraphExtras.Nodes;

[Title( "SGE - Scene Normal" ), Category( "Shader Graph Extras - Universal" ), Icon( "explore" )]
public sealed class SGESceneNormalNode : ShaderNode
{
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coordinates { get; set; }

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Output => ( GraphCompiler compiler ) =>
	{
		var coordinates = compiler.Result( Coordinates ).Cast( 2 );
		return new NodeResult( NodeResultType.Vector3, $"Normals::Sample( {coordinates} )" );
	};
}
