#ifndef RAIN__MATH_H_
#define RAIN__MATH_H_
#include <math.h>
#define RAIN__ALIGNED_(X) __attribute__((aligned(X)))

struct rain_float2 { float x, y; } RAIN__ALIGNED_(8);
typedef struct rain_float2 rain_float2;
struct rain_float3 { float x, y, z; }  RAIN__ALIGNED_(16);
typedef struct rain_float3 rain_float3;
struct rain_float4 { float x, y, z, w; } RAIN__ALIGNED_(16);
typedef struct rain_float4 rain_float4;
struct rain_float4x4 { struct rain_float4 rows[4]; } RAIN__ALIGNED_(16);
typedef struct rain_float4x4 rain_float4x4;

#endif // RAIN__MATH_H_
