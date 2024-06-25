﻿#version 410

in vec3 texCoords;
in vec3 aPos;

uniform samplerCube skybox;
uniform mat4 projection_matrix;
uniform mat4 view_matrix;

out vec4 pos;
out vec3 newpos;
out vec4 FragColor;

void main()
{ 
    pos = projection_matrix * view_matrix* vec4(aPos, 1.0f);
    newpos = aPos;
    FragColor = texture(skybox, texCoords);
}
