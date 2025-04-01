namespace Editor.ShaderGraph.Nodes;

[Title( "SGE - Hue Saturation Value" ), Category( "Transform" ), Icon( "settings_brightness" )]
public sealed class SGEHueSaturationValueNode : ShaderNode
{
	[Hide]
	public string HueSaturationValue => @"
		float3 HueSaturationValue(float3 input, float hue, float saturation, float value )
		{
			float4 k = float4( 0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0 );
			float4 p = lerp( float4( input.bg, k.wz ), float4( input.gb, k.xy ), step( input.b, input.g ) );
			float4 q = lerp( float4( p.xyw, input.r ), float4( input.r, p.yzx ), step( p.x, input.r ) );
			
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			
			float3 hsv = float3( abs( q.z + ( q.w - q.y ) / ( 6.0 * d + e ) ), d / ( q.x + e ), q.x );
			
			float3 hsvOffset = float3(hsv.x + hue - 0.5, hsv.y * saturation, hsv.z * value);
			
			float4 m = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			return hsvOffset.z * lerp( m.xxx, saturate(abs( frac( hsvOffset.xxx + m.xyz ) * 6.0 - m.www ) - m.xxx), hsvOffset.y );
		}
		";

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Input { get; set; }
	
	[Input( typeof( float ) ), Title( "Hue" )]
	[Hide, Editor( nameof( DefaultHue ) )]
	public NodeInput Hue {get; set;}
	
	[Input( typeof( float ) ), Title( "Saturation" )]
	[Hide, Editor( nameof( DefaultSaturation ) )]
	public NodeInput Saturation {get; set;}
	
	[Input( typeof( float ) ), Title( "Value" )]
	[Hide, Editor( nameof( DefaultValue ) )]
	public NodeInput Value {get; set;}

	[InputDefault (nameof( Hue ))]
	public float DefaultHue {get; set;} = 0.5f;

	[InputDefault (nameof( Saturation ))]
	public float DefaultSaturation {get; set;} = 1f;
	
	[InputDefault (nameof( Value ))]
	public float DefaultValue {get; set;} = 1f;

	[Output( typeof( Vector3 ))]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = compiler.ResultOrDefault(Input, Vector3.One).Cast(3);
		var hue = compiler.ResultOrDefault(Hue, DefaultHue);
		var saturation = compiler.ResultOrDefault(Saturation, DefaultSaturation);
		var value = compiler.ResultOrDefault(Value, DefaultValue);
		
		return new NodeResult( NodeResultType.Vector3 , compiler.ResultFunction( compiler.RegisterFunction( HueSaturationValue ), $"{input}, {hue}, {saturation}, {value}" ) );
	};
}