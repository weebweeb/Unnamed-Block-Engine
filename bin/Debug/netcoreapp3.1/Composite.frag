#version 460



uniform sampler2D accumColorTex;
uniform sampler2D accumWeightTex;

out vec4 finalColor;

void main() {
    vec4 color = texelFetch(accumColorTex, ivec2(gl_FragCoord.xy), 0);
    float weight = texelFetch(accumWeightTex, ivec2(gl_FragCoord.xy), 0).r;

    finalColor = vec4(color.rgb / max(color.a, 1e-5), weight);
}
