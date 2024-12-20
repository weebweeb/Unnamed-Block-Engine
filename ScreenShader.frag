#version 450

in vec2 texCoords;  

out vec4 finalColor;

uniform sampler2D accumColorTex;  
uniform sampler2D accumWeightTex; 

void main() {
    
    vec4 accumColor = texture(accumColorTex, texCoords);
    float accumWeight = texture(accumWeightTex, texCoords).r;

    // Normalize the accumulated color by the accumulated weight
    if (accumWeight > 0.0) {
        finalColor = vec4(accumColor.rgb / accumWeight, 1.0);
    } else {
        finalColor = vec4(0.0, 0.0, 0.0, 1.0);
    }
}