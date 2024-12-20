#version 430
in vec3 uv;
uniform float opacity;
uniform sampler2DArray textureArray;
in vec3 normals;


layout(std140, binding = 11) uniform LightData
{
    int num_lights;
    vec4 lighting_directions[100];
    vec4 lighting_colors[100];
    vec4 lighting_brightness[100];
    vec3 ambient;
};

out vec4 accumcolor;
out float accumweight;


vec4 tex = texture(textureArray, uv);

void main(void)
{
    

    vec3 diffusecolor = ambient;
    
     for (int i = 0; i < num_lights; i++) {
        float diffuse = max(dot(normalize(lighting_directions[i].xyz), normals), 0.0);
        
        diffusecolor += lighting_colors[i].xyz * diffuse  * lighting_brightness[i].x;
    }
    


    vec3 finalColor = tex.rgb * diffusecolor;

    

    float weight =
    max(min(1.0, max(max(accumcolor.r, accumcolor.g), accumcolor.b) * accumcolor.a), accumcolor.a) *
    clamp(0.03 / (1e-5 + pow(gl_FragCoord.z / 200, 4.0)), 1e-2, 3e3);

    accumcolor = vec4(finalColor * tex.a, opacity*tex.a) * weight;

    accumweight = tex.a;

    };
