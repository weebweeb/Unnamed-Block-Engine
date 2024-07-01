#version 330 core
out vec4 FragColor;

in vec3 texCoords;

uniform samplerCube skybox;


vec4 sampleCubemap(vec3 dir)
{
    return texture(skybox, dir);
}

void main()
{    


    vec4 color = sampleCubemap(texCoords);

    // Sample neighboring texels for better blending at edges
    vec4 blendColor = color;
    const float blendFactor = 0.001; // Adjust for stronger/weaker blending

    // X axis blending
    blendColor += sampleCubemap(texCoords + vec3(blendFactor, 0.0, 0.0));
    blendColor += sampleCubemap(texCoords - vec3(blendFactor, 0.0, 0.0));

    // Y axis blending
    blendColor += sampleCubemap(texCoords + vec3(0.0, blendFactor, 0.0));
    blendColor += sampleCubemap(texCoords - vec3(0.0, blendFactor, 0.0));

    // Z axis blending
    blendColor += sampleCubemap(texCoords + vec3(0.0, 0.0, blendFactor));
    blendColor += sampleCubemap(texCoords - vec3(0.0, 0.0, blendFactor));

    // Average the color
    blendColor /= 7.0; // (1 original + 6 samples)

    // Output the blended color
    FragColor = blendColor;

  //FragColor = texture(skybox, texCoords);



}