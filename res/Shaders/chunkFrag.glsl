#version 330 core
out vec4 result;

uniform vec3 color;

in vec2 TexCoord;
in float outLight;

uniform sampler2D ourTexture;


void main()
{
    result = texture(ourTexture, TexCoord) * vec4(outLight, outLight, outLight, 1.0);
}
