#version 330

// This work is based on Keenan Crane's paper about 4D julia set
// https://www.cs.cmu.edu/~kmcrane/Projects/QuaternionJulia/paper.pdf

in vec2 fragTexCoord;
out vec4 fragColor;

// Basic variables
uniform float escapeThreshold = 4.0;
uniform float raymarchingPrecision = 0.00001;
uniform float boundingSphereRadius = 100.0;
uniform int maxIteration = 10;
uniform int maxSteps = 254;

// Julia's const
uniform vec4 c = vec4(-0.2, 0.7, 0.0, 0.0);

// Camera Position
uniform vec3 cameraPosition = vec3(5.0, 0.0, 0.0);

// Camera Direction
uniform vec3 cameraForward = vec3(-1.0, 0.0, 0.0);
uniform vec3 cameraRight = vec3(0.0, 1.0, 0.0);
uniform vec3 cameraUp = vec3(0.0, 0.0, 1.0);

// Color Gradient
uniform vec3 colorGradient[10];

//-- Quaternion operations --//

vec4 quatMult(vec4 q1, vec4 q2) {
    vec4 r;
    r.x = q1.x * q2.x - dot(q1.yzw, q2.yzw);
    r.yzw = q1.x * q2.yzw + q2.x * q1.yzw + cross(q1.yzw, q2.yzw);
    return r;
}

vec4 quatSquare(vec4 q) {
    vec4 r;
    r.x = q.x * q.x - dot(q.yzw, q.yzw);
    r.yzw = 2.0 * q.x * q.yzw;
    return r;
}

void iterateQuat(inout vec4 z, inout vec4 dz) {
    for (int i = 0; i < maxIteration; i++) {
        dz = 2.0 * quatMult(z, dz);
        z = quatSquare(z) + c;
        if (dot(z, z) > escapeThreshold) break;
    }
}

//-- Compute Normal --//

vec3 computeNormal( in vec3 p){
    vec4 z = vec4(p,0.0);

    // identity derivative
    vec4 J0 = vec4(1,0,0,0);
    vec4 J1 = vec4(0,1,0,0);
    vec4 J2 = vec4(0,0,1,0);
    
  	for(int i=0; i<maxIteration; i++)
    {
        vec4 cz = vec4(z.x, -z.yzw);
        
        // chain rule of jacobians (removed the 2 factor)
        J0 = vec4( dot(J0,cz), dot(J0.xy,z.yx), dot(J0.xz,z.zx), dot(J0.xw,z.wx) );
        J1 = vec4( dot(J1,cz), dot(J1.xy,z.yx), dot(J1.xz,z.zx), dot(J1.xw,z.wx) );
        J2 = vec4( dot(J2,cz), dot(J2.xy,z.yx), dot(J2.xz,z.zx), dot(J2.xw,z.wx) );

        // z -> z2 + c
        z = quatSquare(z) + c; 
        
        if(dot(z, z)>4.0) break;
    }
    
	vec3 v = vec3( dot(J0,z), 
                   dot(J1,z), 
                   dot(J2,z) );

    return normalize( v );
}
//-- Distance estimation --//

float distanceToJulia(vec3 p) {
    vec4 z = vec4(p, 0.0);
    vec4 dz = vec4(1.0, 0.0, 0.0, 0.0);
    iterateQuat(z, dz);
    float zNorm = length(z);
    return 0.5 * zNorm * log(zNorm) / length(dz);
}

//-- Raymarch --//

vec4 raymarch(vec3 rayOrigin, vec3 rayDirection) {
    float dist;
    //float minDist = distanceToJulia(rayOrigin);
    while (true) {
        dist = distanceToJulia(rayOrigin);
        //if (dist<minDist) minDist = dist;
        if (dist < raymarchingPrecision) {
           vec4 normal = vec4(computeNormal(rayOrigin.xyz), 1.0); // return a color based on the 3D coordinates
           return normal;
        }
        
        if (dot(rayOrigin, rayOrigin) > boundingSphereRadius * boundingSphereRadius) {
            return vec4(0.0, 0.0, 0.0, 1.0);
        }
        
        rayOrigin += rayDirection * dist;
    }
}

//-- Colorize --//

vec4 colorize(vec4 normal) {
    if (normal.xyz == vec3(0.0, 0.0, 0.0)) return vec4(0.0, 0.0, 0.0, 1.0);
    
    float localBrightness = dot(normal.xyz, vec3(1.0, 0.0, 0.0));
    localBrightness = (localBrightness+1)/2;

    float scaled = localBrightness * 9;
    int indice = int(floor(scaled));
    float t = fract(scaled);

    vec3 colorA = colorGradient[indice];
    vec3 colorB = colorGradient[indice+1];

    vec3 finalColor = mix(colorA, colorB, t);

    return vec4(finalColor, 1.0);
}

//-- Main --//

void main() {
    // Convert [0, 1] coordinates to  [-1, 1]
    vec2 uv = fragTexCoord * 2.0 - 1.0;

    // Compute final direction of the ray
    vec3 rayDirection = normalize(-cameraForward + uv.x * cameraRight + uv.y * cameraUp);

    // Throw the ray
    fragColor = colorize(raymarch(cameraPosition, rayDirection));
    // fragColor = raymarch(cameraPosition, rayDirection);
}