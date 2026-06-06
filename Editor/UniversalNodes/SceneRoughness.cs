namespace Editor.ShaderGraph.Nodes;

[Title( "SGE - Scene Roughness" ), Category( "Shader Graph Extras - Universal" ), Icon( "blur_on" )]
public sealed class SGESceneRoughnessNode : ShaderNode
{
	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coordinates { get; set; }

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Output => ( GraphCompiler compiler ) =>
	{
		var coordinates = compiler.Result( Coordinates ).Cast( 2 );
		return new NodeResult( NodeResultType.Float, $"Roughness::Sample( {coordinates} )" );
	};
}
