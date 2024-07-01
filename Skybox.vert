#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 texCoords;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;

void main()
{
    vec4 pos = projection_matrix * view_matrix * vec4(aPos, 1.0f);
    // Having z equal w will always result in a depth of 1.0f
    gl_Position = vec4(pos.x, pos.y, pos.w, pos.w);
    // We want to flip the z axis due to the different coordinate systems (left hand vs right hand)
    texCoords = vec3(aPos.x, aPos.y, -aPos.z);
}
