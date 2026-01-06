#version 330 core
out vec4 result;

uniform vec3 color;

in highp vec3 TexCoord;
in float outLight;

uniform sampler2DArray textureArray;


void main()
{
    vec4 texColor = texture(textureArray, TexCoord);
    if (texColor.a < 0.95) discard;
    result = texColor * vec4(outLight, outLight, outLight, 1.0);
}
