#version 410

out vec3 texCoords;
in vec4 pos;
in vec3 newpos;

void main()
{
    gl_Position = vec4(pos.x, pos.y, pos.w, pos.w);
    // We want to flip the z axis due to the different coordinate systems (left hand vs right hand)
    texCoords = vec3(newpos.x, newpos.y, -newpos.z);
}  