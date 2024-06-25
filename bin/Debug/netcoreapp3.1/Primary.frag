#version 430
in vec2 uv;
uniform float opacity;
uniform sampler2D texture;
in vec3 normals;


layout(std140, binding = 11) uniform LightData
{
    int num_lights;
    vec4 lighting_directions[100];
    vec4 lighting_colors[100];
    vec4 lighting_brightness[100];
    vec3 ambient;
};

out vec4 fragment;


vec4 tex = texture2D(texture, uv);

void main(void)
{
    

    vec3 diffusecolor = ambient;
    
     for (int i = 0; i < num_lights; i++) {
        float diffuse = max(dot(normalize(lighting_directions[i].xyz), normals), 0.0);
        
        diffusecolor += lighting_colors[i].xyz * diffuse  * lighting_brightness[i].x;
    }
    

    vec3 finalColor = tex.rgb * diffusecolor;

    fragment = vec4(finalColor, opacity*tex.a);
    };
