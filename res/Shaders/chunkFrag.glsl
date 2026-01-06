#version 330 core
out vec4 result;

uniform vec3 color;

in highp vec2 TexCoord;
in float outLight;

uniform sampler2D ourTexture;


void main()
{
    vec4 texColor = texture(ourTexture, TexCoord);
    if (texColor.a < 0.95) discard;
    result = texColor * vec4(outLight, outLight, outLight, 1.0);
}
