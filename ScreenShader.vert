#version 450

// Out texture coordinates passed to the fragment shader
out vec2 texCoords;

void main() {
    // Define the full-screen triangle vertices based on the vertex ID
    vec2 positions[3] = vec2[3](
        vec2(-1.0, -1.0),  // Bottom-left corner
        vec2( 3.0, -1.0),  // Bottom-right corner (off-screen)
        vec2(-1.0,  3.0)   // Top-left corner (off-screen)
    );

    // Set the vertex position in NDC
    gl_Position = vec4(positions[gl_VertexID], 0.0, 1.0);

    // Calculate texture coordinates from vertex position in NDC
    texCoords = positions[gl_VertexID] * 0.5 + 0.5;
}
