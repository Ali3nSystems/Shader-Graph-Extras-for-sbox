float2 SGEParallaxOcclusionMappingContactRefinement(float2 coordinates, Texture2D texture, SamplerState sampler, float3 tangentSpaceViewDirection, float amplitude, float minimumIterations,float maximumIterations, float levelOfDetail, float offset, int channel)
    {
        // Contact Refinement by Andrea Riccardi
        // https://www.artstation.com/andreariccardi/blog/3VPo/a-new-approach-for-parallax-mapping-presenting-the-contact-refinement-parallax-mapping-technique

        float layerNumbers = lerp(minimumIterations, maximumIterations, levelOfDetail);
        float layerDepth = 1.0 / layerNumbers;
        float currentStep = 0.0;

        float2 S = tangentSpaceViewDirection.xy / tangentSpaceViewDirection.z * amplitude;
        float2 deltaCoordinates = S / layerNumbers;

        float2 currentCoordinates = coordinates + offset * S;

        float currentSample = 1.0 - Tex2DS(texture, sampler, currentCoordinates)[channel];
        float previousSample = currentSample;

        while (currentSample > currentStep)
        {
            currentStep += layerDepth;
            currentCoordinates -= deltaCoordinates;

            previousSample = currentSample;
            currentSample = 1.0 - Tex2DS(texture, sampler, currentCoordinates)[channel];
        }

        if (currentStep > 0.0)
        {
            currentCoordinates += deltaCoordinates;
            currentStep -= layerDepth;
            currentSample = previousSample;

            float2 adjustedDelta = deltaCoordinates * layerDepth;
            float adjustedStep = layerDepth * layerDepth;

            while (currentSample > currentStep)
            {
                currentStep += adjustedStep;
                currentCoordinates -= adjustedDelta;
                previousSample = currentSample;

                currentSample = 1.0 - Tex2DS(texture, sampler, currentCoordinates)[channel];
            }
        }

        return currentCoordinates;
    }

float2 SGEParallaxOcclusionMappingStandard(float2 coordinates, Texture2D texture, SamplerState sampler, float3 tangentSpaceViewDirection, float amplitude, float minimumIterations,float maximumIterations, float levelOfDetail, float offset, int channel)
{
    float layerNumbers = lerp(minimumIterations, maximumIterations, levelOfDetail);
    float layerDepth = 1 / layerNumbers;
    float currentLayerDepth = 0;

    float2 S = tangentSpaceViewDirection.xy / tangentSpaceViewDirection.z * amplitude;
    float2 deltaCoordinates = S / layerNumbers;

    float2 currentCoordinates = coordinates + offset * S;

    float currentDepthValue = 1 - Tex2DS(texture, sampler, currentCoordinates)[channel];

    while (currentLayerDepth < currentDepthValue)
    {
        currentCoordinates -= deltaCoordinates;
        currentDepthValue = 1 - Tex2DS(texture, sampler, currentCoordinates)[channel];
        currentLayerDepth += layerDepth;
    }

    float2 previousCoordinates = currentCoordinates + deltaCoordinates;
    float afterDepth = currentDepthValue - currentLayerDepth;
    float beforeDepth = 1 - Tex2DS(texture, sampler, previousCoordinates)[channel] - currentLayerDepth + layerDepth;
    float weight = afterDepth / (afterDepth - beforeDepth);
    currentCoordinates = previousCoordinates * weight + currentCoordinates * (1 - weight);

    return currentCoordinates;
}

float2 SGEParallaxOcclusionMappingSteep(float2 coordinates, Texture2D texture, SamplerState sampler, float3 tangentSpaceViewDirection, float amplitude, float minimumIterations,float maximumIterations, float levelOfDetail, float offset, int channel)
    {
        float layerNumbers = lerp(minimumIterations, maximumIterations, levelOfDetail);
        float layerDepth = 1 / layerNumbers;
        float currentLayerDepth = 0;

        float2 S = tangentSpaceViewDirection.xy / tangentSpaceViewDirection.z * amplitude;
        float2 deltaCoordinates = S / layerNumbers;

        float2 currentCoordinates = coordinates + offset * S;

        float currentDepthValue = 1 - Tex2DS(texture, sampler, currentCoordinates)[channel];

        while (currentLayerDepth < currentDepthValue)
        {
            currentCoordinates -= deltaCoordinates;
            currentDepthValue = 1 - Tex2DS(texture, sampler, currentCoordinates)[channel];
            currentLayerDepth += layerDepth;
        }
        return currentCoordinates;
    }
		