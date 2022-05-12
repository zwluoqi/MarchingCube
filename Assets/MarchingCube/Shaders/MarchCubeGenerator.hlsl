#ifndef QINGZHU_MARCHINGCUBE_GENERATOR
#define QINGZHU_MARCHINGCUBE_GENERATOR


#include "MarchingCubeLookupTable.hlsl"
#include "NoiseShader/SimplexNoise3D.hlsl"
struct ShapeSetting
{
    float roughness;
    float cubeSize;
    float3 offset;
    int type;
    float3 strength;
};

struct TriangleXXOO
{
    float3 a;
    float3 b;
    float3 c;
};

//等于0位曲面边界

float KeyCircle(float3 pos)
{
    return length(pos - float3(1,1,1)) - 0.5f;
}

float KeyNoise(float strength,float3 pos,float height)
{
    float s =  snoise(float3(pos.x,0,pos.z));
    s = s*0.5+0.5;
    s *= strength;
    return height-s;
}

float KeyMathSin(float3 pos)
{
    float x = pos.x - 1.0f;
    float y = pos.y - 1.0f;
    float z = pos.z - 1.0f;
             
    return sin(x*y + x*z + y*z) + sin(x*y) + sin(y*z) + sin(x*z) - 0.5f;
}






#endif