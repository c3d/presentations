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
uniform float     iKind;
uniform float     iSmooth;
uniform vec3      iShift;
uniform vec3      iColor1;
uniform vec3      iColor2;

#define iTime iGlobalTime

// The MIT License
// Copyright © 2014 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// Smooth Voronoi - avoiding aliasing, by replacing the usual min() function, which is
// discontinuous, with a smooth version. That can help preventing some aliasing, and also
// provides with more artistic control of the final procedural textures/models.

// More Voronoi shaders:
//
// Exact edges:  https://www.shadertoy.com/view/ldl3W8
// Hierarchical: https://www.shadertoy.com/view/Xll3zX
// Smooth:       https://www.shadertoy.com/view/ldB3zc
// Voronoise:    https://www.shadertoy.com/view/Xd23Dh



float hash1( float n ) { return fract(sin(n)*43758.5453); }
vec2  hash2( vec2  p ) { p = vec2( dot(p,vec2(127.1,311.7)), dot(p,vec2(269.5,183.3)) ); return fract(sin(p)*43758.5453); }

// The parameter w controls the smoothness
vec4 voronoi( in vec2 x, float w )
{
    vec2 n = floor( x );
    vec2 f = fract( x );

    vec4 m = vec4( 8.0, 0.0, 0.0, 0.0 );
    for( int j=-2; j<=2; j++ )
    {
        for( int i=-2; i<=2; i++ )
        {
            vec2 g = vec2( float(i),float(j) );
            vec2 o = hash2( n + g );

            // animate
            o = 0.5 + 0.5*sin( iTime + 6.2831*o );

            // distance to cell
            float d = length(g - f + o);

            // do the smoth min for colors and distances
            vec3 col = iColor1 + (iColor2-iColor1) * (0.5 + 0.5 * sin( hash1(dot(n+g,vec2(7.0,113.0)))*2.5 + 3.5 + iShift));
            float h = smoothstep( 0.0, 1.0, 0.5 + 0.5*(m.x-d)/w );

	    m.x   = mix( m.x,     d, h ) - h*(1.0-h)*w/(1.0+3.0*w); // distance
            m.yzw = mix( m.yzw, col, h ) - h*(1.0-h)*w/(1.0+3.0*w); // color
        }
    }
    return m;
}

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    vec2 p = fragCoord.xy/iResolution.yy;

    float k = 2.0 + 70.0 * pow( 0.5 + 0.5*sin(0.25*6.2831*iTime), 4.0 );
    k = 0.5 * iSmooth;
    vec4 c = voronoi( 6.0*p, k );

    vec3 col = c.yzw;

    col *= 1.0 - 0.8*c.x*(1.0-smoothstep(0.0, 1.0, iKind));
    col *= mix(c.x,1.0,(1.0-smoothstep(1.0, 2.0, iKind)));

    fragColor = vec4( col, 1.0 );
}


void main(void)
{
    vec4 color = vec4(0.0,0.0,0.0,1.0);

    mainImage( color, gl_FragCoord.xy + 0.03 * iViewpoint);
    color.a = 1.0;
    gl_FragColor = color;
}
