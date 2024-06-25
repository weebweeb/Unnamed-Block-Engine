#version 450
in vec3 vertexPosition;
in vec2 vertexUV;
in vec3 vertexNormals;


out vec2 uv;
out vec3 normals;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;

void main(void)
{
    uv = vertexUV;
    normals = normalize((model_matrix * vec4(vertexNormals,0)).xyz);
    
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
};
