#ifndef QINGZHU_MARCHINGCUBE_GENERATOR
#define QINGZHU_MARCHINGCUBE_GENERATOR


#include "MarchingCubeLookupTable.hlsl"
#include "NoiseShader/SimplexNoise3D.hlsl"
struct ShapeSetting
{
    float roughness;
    float cubeSize;
    float offset;
    int type;
};

struct TriangleXXOO
{
    float3 a;
    float3 b;
    float3 c;
};

float KeyCircle(float3 pos)
{
    return length(pos - float3(1,1,1)) - 0.5f;
}

float KeyNoise(float3 pos)
{
    return pos.y - snoise(pos);
}

float KeyMathSin(float3 pos)
{
    float x = pos.x - 1.0f;
    float y = pos.y - 1.0f;
    float z = pos.z - 1.0f;
             
    return sin(x*y + x*z + y*z) + sin(x*y) + sin(y*z) + sin(x*z) - 0.5f;
}



float3 VertexInterp(float3 p1, float3 p2, int v1, int v2, int isoVal)
{
    if (isoVal == v1)
    {
        return p1;
    }
    if (isoVal == v2)
    {
        return p2;
    }

    if (v1 == v2)
    {
        return p2;
    }

    float t = (isoVal - v1)*1.0f / (v2 - v1);
    return lerp(p1, p2, t);
}


#endif