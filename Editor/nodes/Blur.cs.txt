namespace Editor.ShaderGraph.Nodes;

[Title( "SGE - Blur" ), Category( "Shader Graph Extras" ), Icon( "blur_on" )]
public sealed class SGEBlurNode : ShaderNode
{
	[Hide]
	public string Blur => @"
		float3 Blur( float2 coordinates, Texture2D texture, SamplerState sampler, float value )
		{
			float twoPi = 6.28318530718f;
			float directions = 16.0f;
			float quality = 4.0f;
			float taps = 1.0f;

			float3 color = texture.Sample( sampler, coordinates ).rgb;

			[unroll]
			for( float d=0.0; d<twoPi; d+=twoPi/directions)
			{
				[unroll]
				for(float j=1.0/quality; j<=1.0; j+=1.0/quality)
				{
					taps += 1;
					color += texture.Sample( sampler, coordinates + float2( cos(d), sin(d) ) * value * 0.1f * j ).rgb;
				}
			}
			return color / taps;
		}
		";

	[Input( typeof( Vector2 ) )] 
	[Hide]
	public NodeInput Coordinates { get; set; }
	
	[Input(typeof(Texture2DObject))]
	[Hide]
	public NodeInput Texture { get; set; }
	
	[Input(typeof(Sampler))]
	[Hide]
	public NodeInput Sampler { get; set; }

	[Input( typeof( float ) )] 
	[Hide]
	public NodeInput Value { get; set; }
	
	[InputDefault( nameof( Value ) )]
	public float DefaultValue { get; set; } = 1f;

	[Output( typeof( Vector3 ) )] 
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var coordinates = compiler.Result(Coordinates);
		var texture = compiler.Result(Texture);
		var sampler = compiler.Result(Sampler);
		var value = compiler.ResultOrDefault(Value, DefaultValue);

		return new NodeResult(NodeResultType.Vector3, compiler.ResultFunction(compiler.RegisterFunction(Blur), $"{coordinates}, {texture}, {sampler}, {value}"));
	};
}