float2 SGEBumpOffsetIterative(float2 coordinates, Texture2D texture, SamplerState sampler, float3 tangentSpaceViewDirection, float amplitude, float minimumIterations,float maximumIterations, float levelOfDetail, float offset, int channel, int zDivision)
{
        float layerNumbers = lerp(minimumIterations, maximumIterations, levelOfDetail);

        float ratioPerPass = (amplitude / max(ceil(layerNumbers), 1));
        offset = offset - 1;

        float2 currentCoordinates = coordinates;

        // zDivision: 0 = Enabled (perspective-correct), 1 = Disabled (no grazing-angle blowup)
        float2 viewOffset = zDivision == 0
            ? tangentSpaceViewDirection.xy / tangentSpaceViewDirection.z
            : -tangentSpaceViewDirection.xy;

        [loop]
        for (int i = 0; i < ceil(layerNumbers); i++)
        {
            float currentDepthValue = texture.Sample(sampler, currentCoordinates)[channel];
            currentCoordinates += ((currentDepthValue + offset) * ratioPerPass * viewOffset);
        }

        return lerp(coordinates, currentCoordinates, saturate(layerNumbers));
}

float2 SGEBumpOffsetStandard(float2 coordinates, Texture2D texture, SamplerState sampler, float3 tangentSpaceViewDirection, float amplitude, float levelOfDetail, float offset, int channel, int zDivision)
{
    float currentDepthValue = texture.Sample(sampler, coordinates)[channel];
    float layerNumbers = lerp(1, currentDepthValue,levelOfDetail);

    // zDivision: 0 = Enabled (perspective-correct), 1 = Disabled (no grazing-angle blowup)
    // Dividing by .z flips the effective direction, so the amplitude is negated
    // to keep the bump pushing the same way in both modes.
    float2 viewOffset = zDivision == 0
        ? tangentSpaceViewDirection.xy / tangentSpaceViewDirection.z
        : tangentSpaceViewDirection.xy;
    float signedAmplitude = zDivision == 0 ? -amplitude : amplitude;

    return coordinates + viewOffset * (-offset + 1 - layerNumbers) * signedAmplitude;
}