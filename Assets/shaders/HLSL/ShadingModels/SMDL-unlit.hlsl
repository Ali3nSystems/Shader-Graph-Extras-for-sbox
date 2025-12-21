class ShadingModelUnlit
{
    static float4 Shade( Material m )
    {
        return float4( m.Albedo, m.Opacity );
    }
};
