#ifndef QINGZHU_MARCHINGCUBE_GENERATOR
#define QINGZHU_MARCHINGCUBE_GENERATOR


// #include "MarchingCubeLookupTable.hlsl"
#include "../NoiseShader/SimplexNoise3D.hlsl"
struct ShapeSetting
{
    float roughness;
    float cubeSize;
    float3 floorOffset;
    int type;
    float strength;

    int octaves;
    float weightMultiplier;
    float persistence;
    float lacunarity;
    float2 sharpenParams;
    float3 weightNoiseXYZ;
};

struct TriangleXXOO
{
    float3 a;
    float3 b;
    float3 c;
};

//等于0位曲面边界

float KeyCircle(float3 pos,ShapeSetting shape_setting)
{
    return length(pos - float3(1,1,1)) - 0.5f;
}


float KeyNoise2(float3 pos,ShapeSetting shape_setting,StructuredBuffer<float3> offsets)
{
    float frequency = shape_setting.roughness*0.001;
    float amplitude = 1;
    float weight = 1;
    float noise = 0;
    for (int j =0; j < shape_setting.octaves; j++) {
        float3 x = (pos) * frequency + offsets[j];
        x*= shape_setting.weightNoiseXYZ;
        float n = snoise(x);
        float v = 1-abs(n);
        v = v*v;
        v *= weight;
        weight = max(min(v*shape_setting.weightMultiplier,1),0);
        noise += v * amplitude;
        amplitude *= shape_setting.persistence;
        frequency *= shape_setting.lacunarity;
    }

    //地面高度调整
    float finalVal = -(pos.y + shape_setting.floorOffset.y) + noise * shape_setting.strength;

    //梯田
    float x = max(shape_setting.sharpenParams.x,0.001);
    finalVal += (fmod(pos.y,x)*x) * shape_setting.sharpenParams.y;
    
    return finalVal;
}

float KeyNoise(ShapeSetting shape_setting,float3 pos)
{
    float density = 0;
    //
    float v = snoise(shape_setting.roughness*float3(pos.x,pos.y,pos.z));
    v = v*v;
    v *= shape_setting.strength;
    density += v;
    return density-pos.y;
}

float KeyMathSin(float3 pos,ShapeSetting shape_setting)
{
    float x = pos.x - 1.0f;
    float y = pos.y - 1.0f;
    float z = pos.z - 1.0f;
             
    return sin(x*y + x*z + y*z) + sin(x*y) + sin(y*z) + sin(x*z) - 0.5f;
}






#endif