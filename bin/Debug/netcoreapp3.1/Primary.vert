#version 450
layout (location = 0) in vec3 vertexPosition;
layout (location = 1) in vec2 vertexUV;
layout (location = 2) in vec3 vertexNormals;


out vec3 uv;
out vec3 normals;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
layout(std140, binding = 4) buffer MatriceStructData {
    mat4 model_matrix[100000];
    vec4 TextureIDs[100000];

};

void main(void)
{
    uv = vec3(vertexUV.x,vertexUV.y,TextureIDs[gl_InstanceID].x);
    normals = normalize((model_matrix[gl_InstanceID] * vec4(vertexNormals,0)).xyz);
    
    gl_Position = projection_matrix * view_matrix * model_matrix[gl_InstanceID] * vec4(vertexPosition, 1);
};
