float2 SGEParallaxOcclusionMappingContactRefinementTextureCoordinate(float2 coordinates, Texture2D texture, SamplerState sampler, float3 tangentSpaceViewDirection, float amplitude, float minimumIterations,float maximumIterations, float levelOfDetail, float offset, int channel)
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

float2 SGEParallaxOcclusionMappingStandardTextureCoordinate(float2 coordinates, Texture2D texture, SamplerState sampler, float3 tangentSpaceViewDirection, float amplitude, float minimumIterations,float maximumIterations, float levelOfDetail, float offset, int channel)
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

float2 SGEParallaxOcclusionMappingSteepTextureCoordinate(float2 coordinates, Texture2D texture, SamplerState sampler, float3 tangentSpaceViewDirection, float amplitude, float minimumIterations,float maximumIterations, float levelOfDetail, float offset, int channel)
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

float2 SGEParallaxOcclusionMappingContactRefinementWorldSpacePosition(float2 coordinates, Texture2D texture, SamplerState sampler, float3 tangentSpaceViewDirection, float amplitude, float minimumIterations,float maximumIterations, float levelOfDetail, float offset, int channel, float2 worldSpacePosition, float3 tangentU, float3 tangentV)
{
    // Calculate UV-per-world ratio using screen-space derivatives
    // This gives us the texel density
    float2 dUVdX = ddx(coordinates);
    float2 dUVdY = ddy(coordinates);
    float2 dWorlddX = ddx(worldSpacePosition);
    float2 dWorlddY = ddy(worldSpacePosition);

    // UV change / world change = UV per world unit
    float2 uvPerWorldX = dUVdX / (max(length(dWorlddX), 0.0001));
    float uvPerWorldMagX = length(uvPerWorldX);

    float2 uvPerWorldY = dUVdY / (max(length(dWorlddY), 0.0001));
    float uvPerWorldMagY = length(uvPerWorldY);

    // Take the maximum to get the UV-per-world ratio
    float uvPerWorld = max(uvPerWorldMagX, uvPerWorldMagY);

    // Get tangent vector magnitudes (world units per UV unit)
    float tangentMagnitude = (length(tangentU) + length(tangentV)) * 0.5;

    // Inverse of uvPerWorld gives us world-per-UV from derivatives
    float worldPerUV = 1.0 / (uvPerWorld + 0.0001);

    // UV scale factor: ratio of actual world-per-UV to tangent world-per-UV
    // If UVs are 2x bigger, worldPerUV will be smaller, so factor will be < 1
    float uvScaleFactor = worldPerUV / (tangentMagnitude + 0.0001);

    // Run POM on the UV coordinates to get the offset UV
    float2 pomUV = SGEParallaxOcclusionMappingContactRefinementTextureCoordinate(coordinates, texture, sampler, tangentSpaceViewDirection, amplitude, minimumIterations, maximumIterations, levelOfDetail, offset, channel);

    // Calculate the UV offset in tangent space
    float2 uvOffset = pomUV - coordinates;

    // Convert tangent space offset to world space with UV scale correction
    float2 worldOffset = (uvOffset.x * tangentU.xy + uvOffset.y * tangentV.xy) * uvScaleFactor;

    // Detect the worldPos scale by comparing to unscaled version
    float worldPosScale = length(worldSpacePosition * 0.01f) / (length(worldSpacePosition) + 0.0001);

    // Apply world space offset scaled to match worldPos input scale
    return (worldSpacePosition * 0.01f + (worldOffset * worldPosScale)) * 100;
}

float2 SGEParallaxOcclusionMappingStandardWorldSpacePosition(float2 coordinates, Texture2D texture, SamplerState sampler, float3 tangentSpaceViewDirection, float amplitude, float minimumIterations,float maximumIterations, float levelOfDetail, float offset, int channel, float2 worldSpacePosition, float3 tangentU, float3 tangentV)
{
    // Calculate UV-per-world ratio using screen-space derivatives
    // This gives us the texel density
    float2 dUVdX = ddx(coordinates);
    float2 dUVdY = ddy(coordinates);
    float2 dWorlddX = ddx(worldSpacePosition);
    float2 dWorlddY = ddy(worldSpacePosition);

    // UV change / world change = UV per world unit
    float2 uvPerWorldX = dUVdX / (max(length(dWorlddX), 0.0001));
    float uvPerWorldMagX = length(uvPerWorldX);

    float2 uvPerWorldY = dUVdY / (max(length(dWorlddY), 0.0001));
    float uvPerWorldMagY = length(uvPerWorldY);

    // Take the maximum to get the UV-per-world ratio
    float uvPerWorld = max(uvPerWorldMagX, uvPerWorldMagY);

    // Get tangent vector magnitudes (world units per UV unit)
    float tangentMagnitude = (length(tangentU) + length(tangentV)) * 0.5;

    // Inverse of uvPerWorld gives us world-per-UV from derivatives
    float worldPerUV = 1.0 / (uvPerWorld + 0.0001);

    // UV scale factor: ratio of actual world-per-UV to tangent world-per-UV
    // If UVs are 2x bigger, worldPerUV will be smaller, so factor will be < 1
    float uvScaleFactor = worldPerUV / (tangentMagnitude + 0.0001);

    // Run POM on the UV coordinates to get the offset UV
    float2 pomUV = SGEParallaxOcclusionMappingStandardTextureCoordinate(coordinates, texture, sampler, tangentSpaceViewDirection, amplitude, minimumIterations, maximumIterations, levelOfDetail, offset, channel);

    // Calculate the UV offset in tangent space
    float2 uvOffset = pomUV - coordinates;

    // Convert tangent space offset to world space with UV scale correction
    float2 worldOffset = (uvOffset.x * tangentU.xy + uvOffset.y * tangentV.xy) * uvScaleFactor;

    // Detect the worldPos scale by comparing to unscaled version
    float worldPosScale = length(worldSpacePosition * 0.01f) / (length(worldSpacePosition) + 0.0001);

    // Apply world space offset scaled to match worldPos input scale
    return (worldSpacePosition * 0.01f + (worldOffset * worldPosScale)) * 100;
}

float2 SGEParallaxOcclusionMappingSteepWorldSpacePosition(float2 coordinates, Texture2D texture, SamplerState sampler, float3 tangentSpaceViewDirection, float amplitude, float minimumIterations,float maximumIterations, float levelOfDetail, float offset, int channel, float2 worldSpacePosition, float3 tangentU, float3 tangentV)
{
    // Calculate UV-per-world ratio using screen-space derivatives
    // This gives us the texel density
    float2 dUVdX = ddx(coordinates);
    float2 dUVdY = ddy(coordinates);
    float2 dWorlddX = ddx(worldSpacePosition);
    float2 dWorlddY = ddy(worldSpacePosition);

    // UV change / world change = UV per world unit
    float2 uvPerWorldX = dUVdX / (max(length(dWorlddX), 0.0001));
    float uvPerWorldMagX = length(uvPerWorldX);

    float2 uvPerWorldY = dUVdY / (max(length(dWorlddY), 0.0001));
    float uvPerWorldMagY = length(uvPerWorldY);

    // Take the maximum to get the UV-per-world ratio
    float uvPerWorld = max(uvPerWorldMagX, uvPerWorldMagY);

    // Get tangent vector magnitudes (world units per UV unit)
    float tangentMagnitude = (length(tangentU) + length(tangentV)) * 0.5;

    // Inverse of uvPerWorld gives us world-per-UV from derivatives
    float worldPerUV = 1.0 / (uvPerWorld + 0.0001);

    // UV scale factor: ratio of actual world-per-UV to tangent world-per-UV
    // If UVs are 2x bigger, worldPerUV will be smaller, so factor will be < 1
    float uvScaleFactor = worldPerUV / (tangentMagnitude + 0.0001);

    // Run POM on the UV coordinates to get the offset UV
    float2 pomUV = SGEParallaxOcclusionMappingSteepTextureCoordinate(coordinates, texture, sampler, tangentSpaceViewDirection, amplitude, minimumIterations, maximumIterations, levelOfDetail, offset, channel);

    // Calculate the UV offset in tangent space
    float2 uvOffset = pomUV - coordinates;

    // Convert tangent space offset to world space with UV scale correction
    float2 worldOffset = (uvOffset.x * tangentU.xy + uvOffset.y * tangentV.xy) * uvScaleFactor;

    // Detect the worldPos scale by comparing to unscaled version
    float worldPosScale = length(worldSpacePosition * 0.01f) / (length(worldSpacePosition) + 0.0001);

    // Apply world space offset scaled to match worldPos input scale
    return (worldSpacePosition * 0.01f + (worldOffset * worldPosScale)) * 100;
}