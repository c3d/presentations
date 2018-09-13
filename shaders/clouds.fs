// ****************************************************************************
//  Shader.fsh                                                 Firefly project
// ****************************************************************************
//
//   File Description:
//
//      The primary testing shader
//
//
//
//
//
//
//
//
// ****************************************************************************

//  (C) 2015 Christophe de Dinechin <christophe@taodyne.com>
//  (C) 2015 Taodyne SAS
// ****************************************************************************


#ifdef OPENGL_ES
precision mediump float;
#endif

uniform vec3      iResolution;           // viewport resolution (in pixels)
uniform float     iGlobalTime;           // shader playback time (in seconds)
uniform float     iChannelTime[4];       // channel playback time (in seconds)
uniform vec3      iChannelResolution[4]; // channel resolution (in pixels)
uniform vec4      iMouse;                // mouse pixel coords. xy: current (if MLB down), zw: click
uniform sampler2D iChannel0;          // input channel. XX = 2D/Cube
uniform sampler2D iChannel1;          // input channel. XX = 2D/Cube
uniform sampler2D iChannel2;          // input channel. XX = 2D/Cube
uniform sampler2D iChannel3;          // input channel. XX = 2D/Cube
uniform vec4      iDate;                 // (year, month, day, time in seconds)
uniform float     iViewpoint;

#define noiseMap  iChannel0

#define MARCH_TMIN      0.4
#define MARCH_TMIN_HIT  (0.9 * PLAYER_SIZE)
#define MARCH_TMAX      40.0
#define MARCH_PRECISION 0.001
#define MARCH_STEPS     64

const float pi = 3.141592653589793;


// ============================================================================
//
//    Noise generation
//
// ============================================================================

float hash(float n)
// ----------------------------------------------------------------------------
//   Simple hash function
// ----------------------------------------------------------------------------
{
    return fract(sin(n)*43758.5453);
}


float noise(in vec3 x)
// ----------------------------------------------------------------------------
//   3D LUT-based noise
// ----------------------------------------------------------------------------
{
    vec3 p = floor(x);
    vec3 f = fract(x);
    f = f*f*(3.0-2.0*f);

    vec2 uv = (p.xy+vec2(37.0,17.0)*p.z) + f.xy;
    vec2 rg = texture2D(noiseMap, (uv+ 0.5)/256.0, -100.0).yx;
    return mix(rg.x, rg.y, f.z);
}


float noise(in vec2 x)
// ----------------------------------------------------------------------------
//   2D LUT-based noise
// ----------------------------------------------------------------------------
{
    vec2 p = floor(x);
    vec2 f = fract(x);
    vec2 uv = p.xy + f.xy*f.xy*(3.0-2.0*f.xy);
    return texture2D(noiseMap, (uv+118.4)/256.0, -100.0).x;
}


vec3 noised(vec2 x)
// ----------------------------------------------------------------------------
//  Compute noise with derivatives
// ----------------------------------------------------------------------------
{
    vec2 p = floor(x);
    vec2 f = fract(x);
    vec2 u = f*f*(3.0-2.0*f);
    float a = texture2D(noiseMap,(p+vec2(0.5,0.5))/256.0,-100.0).x;
    float b = texture2D(noiseMap,(p+vec2(1.5,0.5))/256.0,-100.0).x;
    float c = texture2D(noiseMap,(p+vec2(0.5,1.5))/256.0,-100.0).x;
    float d = texture2D(noiseMap,(p+vec2(1.5,1.5))/256.0,-100.0).x;
    return vec3(a+(b-a)*u.x+(c-a)*u.y+(a-b-c+d)*u.x*u.y,
                6.0*f*(1.0-f)*(vec2(b-a,c-a)+(a-b-c+d)*u.yx));
}


float fbm(vec3 p)
// ----------------------------------------------------------------------------
//   Fractal based map in 3D
// ----------------------------------------------------------------------------
{
    const mat3 m = mat3(0.00,  0.90,  0.60,
                        -0.90,  0.36, -0.48,
                        -0.60, -0.48,  0.34);

    float f;
    f  =      0.5000*noise(p); p = m*p*2.02;
    f +=      0.2500*noise(p); p = m*p*2.33;
    f +=      0.1250*noise(p); p = m*p*2.01;
    f +=      0.0625*noise(p);
    return f/(0.9175);
}


float fbm(vec2 p)
// ----------------------------------------------------------------------------
//   Fractal based map in 2D
// ----------------------------------------------------------------------------
{
    const mat2 mr = mat2 (0.84147,  0.54030,
                          0.54030, -0.84147);
    float f;
    f  =      0.5000*noise(p); p = mr*p*2.02;
    f +=      0.2500*noise(p); p = mr*p*2.33;
    f +=      0.1250*noise(p); p = mr*p*2.01;
    f +=      0.0625*noise(p);
    return f/(0.9175);
}


const mat2 terrainXform = mat2(1.6,-1.2,1.2,1.6);

float terrain(vec2 x, int loops)
// ----------------------------------------------------------------------------
//   The terrain function from elevated
// ----------------------------------------------------------------------------
{
    vec2  p = x*0.003;
    float a = 0.0;
    float b = 1.0;
    vec2  d = vec2(0.0);
    for(int i=0; i < loops; i++)
    {
        vec3 n = noised(p);
        d += n.yz;
        a += b*n.x/(1.0+dot(d,d));
        b *= 0.5;
        p = terrainXform*p;
    }

    return 1.0 * a;
}


float heightMap(vec3 p)
// ----------------------------------------------------------------------------
//    Height map
// ----------------------------------------------------------------------------
{
    float n = noise(p.xz*1.14);
    return p.y - terrain(p.xz, 6);
    return p.y - 2.28*n + 1.0;
}


float waterHeightMap(vec2 pos)
// ----------------------------------------------------------------------------
//   Water height map
// ----------------------------------------------------------------------------
{
    const mat2 mr = mat2 (0.84147,  0.54030,
                          0.54030, -0.84147);
    vec2 posm = pos * mr;
    posm.x += 0.25*iGlobalTime;
    float f = fbm(vec3(posm*1.9, iGlobalTime*0.27));
    float height = 0.5+0.1*f;
    height += 0.13*sin(posm.x*6.0 + 10.0*f);

    return  height;
}



// ============================================================================
//
//    Union, difference and rotation operators
//
// ============================================================================

vec2 U(vec2 mt1, float m, float t)
// ----------------------------------------------------------------------------
//   Union of two shapes
// ----------------------------------------------------------------------------
{
    vec2 mt2 = vec2(m, t);
    return mix(mt1, mt2, step(t,mt1.y));
}


vec2 SU(vec2 mt1, float m, float t)
// ----------------------------------------------------------------------------
//   Union of two shapes
// ----------------------------------------------------------------------------
{
    vec2 mt2 = vec2(m, t);
    return mix(mt1, mt2, 0.0);
}


vec2 D(vec2 mt1, float m, float t)
// ----------------------------------------------------------------------------
//   Difference of two shapes
// ----------------------------------------------------------------------------
{
    vec2 mt2 = vec2(-m, t);
    return mix(-mt2,mt1,step(-t,mt1.y));
}


vec3 repeat(vec3 p, vec3 c)
// ----------------------------------------------------------------------------
//   Repeat a given coordinate along size c
// ----------------------------------------------------------------------------
{
    return mod(p,c)-0.5*c;
}


mat2 rotate(float a)
// ----------------------------------------------------------------------------
//    Return a 2D rotation matrix
// ----------------------------------------------------------------------------
{
    float s = sin(a);
    float c = cos(a);
    return mat2(c,-s,s,c);
}


vec3 rotX(vec3 p, float a)
// ----------------------------------------------------------------------------
//    Rotate p along X axis
// ----------------------------------------------------------------------------
{
    p.yz = rotate(a)*p.yz;
    return p;
}


vec3 rotY(vec3 p, float a)
// ----------------------------------------------------------------------------
//    Rotate p along Y axis
// ----------------------------------------------------------------------------
{
    p.xz = rotate(a)*p.xz;
    return p;
}


vec3 rotZ(vec3 p, float a)
// ----------------------------------------------------------------------------
//    Rotate p along Z axis
// ----------------------------------------------------------------------------
{
    p.xy = rotate(a)*p.xy;
    return p;
}



// ============================================================================
//
//   Distance primitives
//
// ============================================================================

float plane(vec3 p)
// ----------------------------------------------------------------------------
//    Distance to a plane
// ----------------------------------------------------------------------------
{
    return p.y;
}


float box(vec3 p, vec3 b)
// ----------------------------------------------------------------------------
//   Distance for a box
// ----------------------------------------------------------------------------
{
    vec3 d = abs(p) - b;
    return min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0));
}


float twisted(vec3 p, vec3 b)
// ----------------------------------------------------------------------------
//   Distance for a box
// ----------------------------------------------------------------------------
{
    p.xy = rotate(p.z * 3.1) * p.xy;
    vec3 d = abs(p) - b;
    return min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0));
}


float sphere(vec3 p, float s)
// ----------------------------------------------------------------------------
//   Distance for a sphere
// ----------------------------------------------------------------------------
{
    return length(p)-s;
}


float cylinder(vec3 p, vec2 h)
// ----------------------------------------------------------------------------
//   Distance for a cylinder
// ----------------------------------------------------------------------------
{
    vec2 d = abs(vec2(length(p.xz),p.y)) - h;
    return min(max(d.x,d.y),0.0) + length(max(d,0.0));
}


float tunnel(vec3 p, float r, float h)
// ----------------------------------------------------------------------------
//   Return the tunnel distance
// ----------------------------------------------------------------------------
{
    vec2 d = abs(vec2(length(p.xy),p.z)) - vec2(r, h);
    return min(max(d.x,d.y),0.0) + length(max(d,0.0));
}


float pipe(vec3 p, float r)
// ----------------------------------------------------------------------------
//    Distance for a pipe
// ----------------------------------------------------------------------------
{
    return length(p.xy) - r;
}


float cone(vec3 p, vec2 sincos, float h)
// ----------------------------------------------------------------------------
//    Distance for a cone
// ----------------------------------------------------------------------------
{
    float q = length(p.xz);
    return max(max(dot(sincos,vec2(q,p.y)), p.y), -h - p.y);
}


float cone(vec3 p, float a, float h)
// ----------------------------------------------------------------------------
//    Distance for a cone (utility)
// ----------------------------------------------------------------------------
{
    return cone(p, vec2(sin(a), cos(a)), h);
}


float p_pipe(vec3 p, float r, float d)
// ----------------------------------------------------------------------------
//   Distance for a p-pipe
// ----------------------------------------------------------------------------
{
    return length(p.xy) - r - 0.02*(max(sin(p.z*d)-0.8,0.));
}


float roundedBox(vec3 p, vec3 b, float r)
// ----------------------------------------------------------------------------
//    Box with rounded corners
// ----------------------------------------------------------------------------
{
    return length(max(abs(p)-b,0.0))-r;
}


float torus(vec3 p, vec2 t)
// ----------------------------------------------------------------------------
//   Torus
// ----------------------------------------------------------------------------
{
    return length(vec2(length(p.xz)-t.x,p.y))-t.y;
}


float heart(vec3 p)
// ----------------------------------------------------------------------------
//   Return the distance to a heart
// ----------------------------------------------------------------------------
{
    // Animate
    float tt = mod(iGlobalTime,1.5)/1.5;
    float ss = pow(tt,.2)*0.5 + 0.5;
    ss = 1.0 + ss*0.5*sin(tt*6.2831*3.0 + p.y*0.5)*exp(-tt*4.0);
    p.xy *= vec2(0.5,1.5) + ss*vec2(0.5,-0.5);

    // Shape
    float a = atan(p.x,p.y)/pi;
    float r = length(p.xy);
    float h = abs(a);
    float d = 0.3*(13.0*h - 22.0*h*h + 10.0*h*h*h)/(6.0-5.0*h);
    d += max(0.05 - 4.5*p.z*p.z, 0.0);

    return max(3.0 * (r - d) , abs(p.z) - 0.1);
}



// ============================================================================
//
//    Sky and clouds
//
// ============================================================================

vec3 sky(vec3 rd)
// ----------------------------------------------------------------------------
//    Compute a sky color
// ----------------------------------------------------------------------------
{
    vec3 color = mix(vec3(0.9, 0.3, 01), vec3(0.1,.15,0.8), smoothstep(-0.2, 0.4, rd.y));
    return color;
}


vec3 sunDir = normalize(vec3(-0.1,0.7, 0.9));
vec3 sun(vec3 rd)
// ----------------------------------------------------------------------------
//    Compute a sun color
// ----------------------------------------------------------------------------
{
    float sundot = clamp(dot(rd,sunDir),0.0,1.0);
    return (0.55*vec3(1.0,0.7,0.4)*pow(sundot,  3.0) +
            0.45*vec3(1.0,0.8,0.6)*pow(sundot,  8.0) +
            0.25*vec3(1.0,0.8,0.6)*pow(sundot, 15.0));
}


vec3 clouds(vec3 ro, vec3 rd, vec3 sky)
// ----------------------------------------------------------------------------
//   Compute clouds
// ----------------------------------------------------------------------------
{
    vec2 sc = ro.xz + rd.xz*(1000.0-ro.y)/rd.y + vec2(3231.0) * sin(0.0831 * iGlobalTime) * sign(rd.y);
    return mix(sky, vec3(1.0,0.95,1.0), 0.7*smoothstep(0.25,0.8,fbm(0.005*sc))*exp(-0.000000005*sc.y*sc.y));
}


vec3 horizon(vec3 rd, vec3 sky)
// ----------------------------------------------------------------------------
//    Compute horizon
// ----------------------------------------------------------------------------
{
    vec3 hcol = mix(vec3(0.1, 0.02, 0.0), vec3(0.1, 0.05, 0.01), noise(1310.3*rd.xy));
    return mix(sky, hcol, pow(1.0-max(rd.y+0.07,0.0), 8.0));
}



// ============================================================================
//
//   Scene map
//
// ============================================================================

vec2 mapm(vec3 p)
// ----------------------------------------------------------------------------
//    Mapping function - Return a vector with material and t-factor
// ----------------------------------------------------------------------------
{
    vec2 mt = vec2(     3.0, tunnel(p, 2.30, 4.0));
    mt = D(mt,          3.0, tunnel(p, 2.27, 4.2));
    mt = U(mt,          1.0, box(p - vec3(0.0,-1.5-0.001*p.z*p.z, 0.0), vec3(0.8, 0.1,25.0)));
    mt = U(mt,          2.0, box(p - vec3(0.0, 0.0, 2.0), vec3(0.7, 0.1, 0.1)));
    mt = U(mt,          2.0, box(p - vec3(0.0,-0.4, 2.0), vec3(0.1, 1.0, 0.1)));
    mt = U(mt,          4.0, heart(p - vec3(0.0, 0.0, 1.7)));
    return mt;
}


float map(vec3 p)
// ----------------------------------------------------------------------------
//   Simplified distance mapping for shadow and occlusion
// ----------------------------------------------------------------------------
{
    return mapm(p).y;
}


vec2 raymarch(vec3 ro, vec3 rd)
// ----------------------------------------------------------------------------
//    Ray marching function
// ----------------------------------------------------------------------------
{
    float m = -1.0;
    float t = MARCH_TMIN;

    for(int s = 0; s < MARCH_STEPS; s++)
    {
        vec2 mt = mapm(ro+rd*t);
        t += mt.y;
        m  = mt.x;
        if (any(lessThan(vec2(MARCH_TMAX, mt.y), vec2(t, t*MARCH_PRECISION))))
            break;
    }

    // No material if beyond max distance
    if (t > MARCH_TMAX)
        m = -1.0;

    // Return factor and material
    return vec2(m, t);
}


float shadow(vec3 ro, vec3 rd, float tmin, float tmax)
// ----------------------------------------------------------------------------
//   Approximation of soft shadow
// ----------------------------------------------------------------------------
{
    float res = 1.0;
    float t = tmin;
    for(int i=0; i<16; i++)
    {
        float h = map(ro + rd*t);
        res = min(res, 8.0*h/t);
        t += clamp(h, 0.02, 0.10);
        if(any(lessThan(vec2(h, tmax), vec2(0.001, t))))
            break;
    }
    return clamp(res, 0.0, 1.0);

}


float occlusion(vec3 pos, vec3 nor)
// ----------------------------------------------------------------------------
//   Compute ambient occlusion by comparing ray with 5 nearby points
// ----------------------------------------------------------------------------
{
    float occ = 0.0;
    float weight = 1.0;
    for(int i=0; i<5; i++)
    {
        float hr = 0.01 + 0.12*float(i)/4.0;
        vec3 aopos =  nor * hr + pos;
        float dd = map(aopos);
        occ += -(dd-hr)*weight;
        weight *= 0.95;
    }
    return clamp(1.0-3.0*occ, 0.0, 1.0);
}


vec3 normal(vec3 pos)
// ----------------------------------------------------------------------------
//   Compute the normal around the given point
// ----------------------------------------------------------------------------
{
    vec3 eps = vec3(0.001, 0.0, 0.0);
    vec3 nor = vec3(
        map(pos+eps.xyy) - map(pos-eps.xyy),
        map(pos+eps.yxy) - map(pos-eps.yxy),
        map(pos+eps.yyx) - map(pos-eps.yyx) );
    return normalize(nor);
}


vec3 render(vec3 ro, vec3 rd)
// ----------------------------------------------------------------------------
//   Render the scene
// ----------------------------------------------------------------------------
{
    vec3  skyColor = sky(rd) + sun(rd);
    vec3  bgColor = clouds(ro, rd, skyColor);
    float blackAndWhite = dot(vec3(0.25, 0.34, 0.23), bgColor);
    return vec3(blackAndWhite);
}


vec3 camera(float t)
// ----------------------------------------------------------------------------
//    Position of the camera
// ----------------------------------------------------------------------------
{
    return vec3(0.0,-0.3,-0.2) + vec3(0.04,0.3,0.06)*sin(vec3(0.1,0.13,0.17) * t);
}


void main()
// ----------------------------------------------------------------------------
//    Setup the ray and render
// ----------------------------------------------------------------------------
{
    vec2 q = gl_FragCoord.xy / iResolution.xy;
    vec2 p = -1.0 + 2.0 * q;
    p.x *= iResolution.x/iResolution.y;

    float roll = 0.0;
    float cosr = cos(roll), sinr = sin(roll);

    // Ray computations
    vec3 ro = camera(iGlobalTime);
    vec3 ta = vec3(0.0, 0.0, 2.0);
    vec3 cw = normalize(ta-ro);
    vec3 cu = normalize(cross(cw,vec3(0,1,0)));
    vec3 cv = normalize(cross(cu,cw));
    vec3 rd = normalize((p.x - 0.07 * iViewpoint)* (cosr * cu + sinr * cv)
                        + p.y * (cosr * cv - sinr * cu) + 1.0 * cw);

    ro -= 1.3 * smoothstep(-0.3, 0.3, -cw.y) * cw;
    vec3 color = render(ro, rd);
    color = pow(color, vec3(0.455));
    gl_FragColor = vec4(color, 1);
}
