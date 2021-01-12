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


// Created by inigo quilez - iq/2014
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.


// A simple and cheap 2D shader to accompany the Pirates of the Caribean music.


float fbm( vec2 p )
{
    return 0.5000*texture2D( iChannel1, p*1.00 ).x + 
        0.2500*texture2D( iChannel1, p*2.02 ).x + 
        0.1250*texture2D( iChannel1, p*4.03 ).x + 
        0.0625*texture2D( iChannel1, p*8.04 ).x;
}

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    float time = mod( iGlobalTime, 60.0 );
    vec2 p = (-iResolution.xy+2.0*fragCoord.xy) / iResolution.y;
    vec2 i = p;

    p.x -= iViewpoint * 0.1;

    // camera
    p += vec2(1.0,3.0)*0.001*2.0*cos( iGlobalTime*5.0 + vec2(0.0,1.5) );    
    p += vec2(1.0,3.0)*0.001*1.0*cos( iGlobalTime*9.0 + vec2(1.0,4.5) );    
    p *= 0.85 + 0.05*length(p);
    float an = 0.3*sin( 0.1*time );
    float co = cos(an);
    float si = sin(an);
    p = mat2( co, -si, si, co )*p;
    
    // water
    vec2 q = vec2(p.x,1.0)/p.y;
    q.y -= 0.9*time;    
    vec2 off = texture2D( iChannel0, 0.1*q*vec2(1.0,2.0) - vec2(0.0,0.007*iGlobalTime) ).xy;
    q += 0.4*(-1.0 + 2.0*off);
    vec3 col = texture2D( iChannel0, 0.1*q *vec2(.5,8.0) + vec2(0.0,0.01*iGlobalTime) ).zyx;
    col *= 0.4;
    float re = 1.0-smoothstep( 0.0, 0.7, abs(p.x-0.6) - abs(p.y)*0.5+0.2 );
    col += 1.0*vec3(1.0,0.9,0.73)*re*0.2*off.y*5.0*(1.0-col.x);
    float re2 = 1.0-smoothstep( 0.0, 2.0, abs(p.x-0.6) - abs(p.y)*0.85 );
    col += 0.7*re2*smoothstep(0.35,1.0,texture2D( iChannel1, 0.1*q *vec2(0.5,8.0) ).x);
    
    // sky
    vec3 sky = vec3(0.0,0.05,0.1)*1.4;
    // stars    
    sky += 0.5*smoothstep( 0.95,1.00,texture2D( iChannel1, 0.25*p ).x);
    sky += 0.5*smoothstep( 0.85,1.0,texture2D( iChannel1, 0.25*p ).x);
    sky += 0.2*pow(1.0-max(0.0,p.y),2.0);
    // clouds    
    float f = fbm( 0.002*vec2(p.x,1.0)/p.y );
    vec3 cloud = vec3(0.3,0.4,0.5)*0.7*(1.0-0.85*sqrt(smoothstep(0.4,1.0,f)));
    sky = mix( sky, cloud, 0.95*smoothstep( 0.4, 0.6, f ) );
    sky = mix( sky, vec3(0.33,0.34,0.35), pow(1.0-max(0.0,p.y),2.0) );
    col = mix( col, sky, smoothstep(0.0,0.1,p.y) );
    
    // horizon
    col += 0.1*pow(clamp(1.0-abs(p.y),0.0,1.0),9.0);

    // moon
    float d = length(p-vec2(0.6,0.5));
    float g = 1.0 - smoothstep( 0.2, 0.22, d );
    float moontex = 0.8+0.2*smoothstep(0.25,0.7,fbm(0.06*p));
    vec3 moon = vec3(1.0,0.97,0.9)*(1.0-0.1*smoothstep(0.2,0.5,f));
    col += 0.8*moon*exp(-4.0*d)*vec3(1.1,1.0,0.8);
    col += 0.2*moon*exp(-2.0*d);
    col = mix( col, moon*moontex, g );
    
    // postprocess
    col *= 1.4;
    col = pow( col, vec3(1.5,1.2,1.0) );    
    col *= clamp(1.0-0.3*length(i), 0.0, 1.0 );

    // fade
    col *=       smoothstep(  0.0,  4.0, time );
    col *= 1.0 - smoothstep( 56.0, 60.0, time );

    fragColor = vec4( col, 1.0 );
}

void main(void)
{
    vec4 color = vec4(0.0,0.0,0.0,1.0);
    
    mainImage( color, gl_FragCoord.xy );
    color.w = 1.0;
    gl_FragColor = color;
}
