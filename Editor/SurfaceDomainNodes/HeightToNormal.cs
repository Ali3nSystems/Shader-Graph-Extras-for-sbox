namespace Editor.ShaderGraph.Nodes;

public enum SGEHeightToNormalMode
{
	Tangent,
	World,
}

[Title( "SGE - Height To Normal" ), Category( "Transform" ), Icon( "alt_route" )]
public sealed class SGEHeightToNormalNode : ShaderNode
{
	[Hide]
	public string Tangent => @"
		float3 Tangent( float input, float strength, float3 position, float3 normal, float3 tangentU, float3 tangentV )
		{
			float3 worldDerivativeX = ddx(position);
			float3 worldDerivativeY = ddy(position);

			float3 crossX = cross(normal, worldDerivativeX);
			float3 crossY = cross(worldDerivativeY, normal);
			float d = dot(worldDerivativeX, crossY);
			float sgn = d < 0.0 ? (-1.f) : 1.f;
			float surface = sgn / max(0.00000000000001192093f, abs(d));

			float dHdx = ddx(input);
			float dHdy = ddy(input);
			float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
			return Vec3WsToTs(normalize(normal - (strength * surfGrad)), normal, tangentU, tangentV);
		}
		";

	[Hide]
	public string World => @"
		float3 World( float input, float strength, float3 position, float3 normal )
		{
			float3 worldDerivativeX = ddx(position);
			float3 worldDerivativeY = ddy(position);

			float3 crossX = cross(normal, worldDerivativeX);
			float3 crossY = cross(worldDerivativeY, normal);
			float d = dot(worldDerivativeX, crossY);
			float sgn = d < 0.0 ? (-1.f) : 1.f;
			float surface = sgn / max(0.00000000000001192093f, abs(d));

			float dHdx = ddx(input);
			float dHdy = ddy(input);
			float3 surfGrad = surface * (dHdx*crossY + dHdy*crossX);
			return normalize(normal - (strength * surfGrad));
		}
		";

	[Input (typeof( float ))]
	[Hide]
	public NodeInput Input {get; set;}

	[Input (typeof( float ))]
	[Hide]
	public NodeInput Strength {get; set;}

	[InputDefault( nameof( Strength ) )]
	public float DefaultStrength {get; set;} = 1f;

	public SGEHeightToNormalMode Mode {get; set;} = SGEHeightToNormalMode.Tangent;

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = compiler.ResultOrDefault(Input, Vector3.One);
		var strength = compiler.ResultOrDefault(Strength, DefaultStrength);
		var position = "i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz";
		var normal = "i.vNormalWs";
		
		NodeResult nodeResult = new NodeResult();

		switch (Mode)
		{
			case SGEHeightToNormalMode.Tangent:
				var tangentU = "i.vTangentUWs";
				var tangentV = "i.vTangentVWs";
				nodeResult = new NodeResult( NodeResultType.Vector3 , compiler.ResultFunction( compiler.RegisterFunction(Tangent), $"{input}, {strength}, {position}, {normal}, {tangentU}, {tangentV}" ) );
				break;
			
			case SGEHeightToNormalMode.World:
				nodeResult = new NodeResult( NodeResultType.Vector3 , compiler.ResultFunction( compiler.RegisterFunction(World), $"{input}, {strength}, {position}, {normal}" ) );
				break;
		}
		
		return nodeResult;
	};
}