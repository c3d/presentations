uniform vec3      iResolution;           // viewport resolution (in pixels)
uniform float     iGlobalTime;           // shader playback time (in seconds)
uniform float     iChannelTime0;       // channel playback time (in seconds)
uniform float     iChannelTime1;       // channel playback time (in seconds)
uniform float     iChannelTime2;       // channel playback time (in seconds)
uniform float     iChannelTime3;       // channel playback time (in seconds)
uniform vec3      iChannelResolution0; // channel resolution (in pixels)
uniform vec3      iChannelResolution1; // channel resolution (in pixels)
uniform vec3      iChannelResolution2; // channel resolution (in pixels)
uniform vec3      iChannelResolution3; // channel resolution (in pixels)
uniform vec4      iMouse;                // mouse pixel coords. xy: current (if MLB down), zw: click
uniform sampler2D iChannel0;          // input channel. XX = 2D/Cube
uniform sampler2D iChannel1;          // input channel. XX = 2D/Cube
uniform sampler2D iChannel2;          // input channel. XX = 2D/Cube
uniform sampler2D iChannel3;          // input channel. XX = 2D/Cube
uniform vec4      iDate;                 // (year, month, day, time in seconds)
uniform float     iViewpoint;


//CBS
//Parallax scrolling fractal galaxy.
//Inspired by JoshP's Simplicity shader: https://www.shadertoy.com/view/lslGWr

// http://www.fractalforums.com/new-theories-and-research/very-simple-formula-for-fractal-patterns/
float field(in vec3 p,float s) {
    float strength = 7. + .03 * log(1.e-6 + fract(sin(iGlobalTime) * 4373.11));
    float accum = s/4.;
    float prev = 0.;
    float tw = 0.;
    for (int i = 0; i < 26; ++i) {
        float mag = dot(p, p);
        p = abs(p) / mag + vec3(-.5, -.4, -1.5);
        float w = exp(-float(i) / 7.);
        accum += w * exp(-strength * pow(abs(mag - prev), 2.2));
        tw += w;
        prev = mag;
    }
    return max(0., 5. * accum / tw - .7);
}

// Less iterations for second layer
float field2(in vec3 p, float s) {
    float strength = 7. + .03 * log(1.e-6 + fract(sin(iGlobalTime) * 4373.11));
    float accum = s/4.;
    float prev = 0.;
    float tw = 0.;
    for (int i = 0; i < 18; ++i) {
        float mag = dot(p, p);
        p = abs(p) / mag + vec3(-.5, -.4, -1.5);
        float w = exp(-float(i) / 7.);
        accum += w * exp(-strength * pow(abs(mag - prev), 2.2));
        tw += w;
        prev = mag;
    }
    return max(0., 5. * accum / tw - .7);
}

vec3 nrand3( vec2 co )
{
    vec3 a = fract( cos( co.x*8.3e-3 + co.y )*vec3(1.3e5, 4.7e5, 2.9e5) );
    vec3 b = fract( sin( co.x*0.3e-3 + co.y )*vec3(8.1e5, 1.0e5, 0.1e5) );
    vec3 c = mix(a, b, 0.5);
    return c;
}


vec3 stereo(vec3 p, float depth, float amp)
{
    float s = -0.01 * depth * iViewpoint;
    s *= 1.0 + amp * sin(18.0 * p.x + 27.1 * p.y + 0.8 * p.x * p.y);
    return vec3(s, 0, 0);
}


void mainImage( out vec4 fragColor, in vec2 fragCoord ) {
    vec2 uv = 2. * fragCoord.xy / iResolution.xy - 1.;
    vec2 uvs = uv * iResolution.xy / max(iResolution.x, iResolution.y);
    vec3 p = vec3(uvs / 4., 0) + vec3(1., -1.3, 0.);
    p += .2 * vec3(sin(iGlobalTime / 16.), sin(iGlobalTime / 12.),  sin(iGlobalTime / 128.));

    float freqs[4];
    //Sound
    freqs[0] = texture2D( iChannel0, vec2( 0.01, 0.25 ) ).x;
    freqs[1] = texture2D( iChannel0, vec2( 0.07, 0.25 ) ).x;
    freqs[2] = texture2D( iChannel0, vec2( 0.15, 0.25 ) ).x;
    freqs[3] = texture2D( iChannel0, vec2( 0.30, 0.25 ) ).x;

    float t = field(p + stereo(p, 1.0, 0.6),freqs[2]);
    float v = (1. - exp((abs(uv.x) - 1.) * 6.)) * (1. - exp((abs(uv.y) - 1.) * 6.));

    //Second Layer
    vec3 p2 = vec3(uvs / (4.+sin(iGlobalTime*0.11)*0.2+0.2+sin(iGlobalTime*0.15)*0.3+0.4), 1.5) + vec3(2., -1.3, -1.);
    p2 += 0.25 * vec3(sin(iGlobalTime / 16.), sin(iGlobalTime / 12.),  sin(iGlobalTime / 128.));
    float t2 = field2(p2 + stereo(p2, 1.5, 0.5), freqs[3]);
    vec4 c2 = mix(.4, 1., v) * vec4(1.3 * t2 * t2 * t2 ,1.8  * t2 * t2 , t2* freqs[0], t2);


    //Let's add some stars
    //Thanks to http://glsl.heroku.com/e#6904.0
    vec2 seed = p.xy * 2.0 + stereo(p, 2.0, 0.6).xy;
    seed = floor(seed * iResolution.x);
    vec3 rnd = nrand3( seed );
    vec4 starcolor = vec4(pow(rnd.y,40.0));

    //Second Layer
    vec2 seed2 = p2.xy * 2.0 + stereo(p2, 3.3, 0.8).xy;
    seed2 = floor(seed2 * iResolution.x);
    vec3 rnd2 = nrand3( seed2 );
    starcolor += vec4(pow(rnd2.y,30.0));

    fragColor = mix(freqs[3]-.3, 1., v) * vec4(1.5*freqs[2] * t * t* t , 1.2*freqs[1] * t * t, freqs[3]*t, 1.0)+c2+starcolor;
}


void main(void)
{
    vec4 color = vec4(0.0,0.0,0.0,1.0);
    mainImage( color, gl_FragCoord.xy );
    color.w = 1.0;
    gl_FragColor = color;
}
