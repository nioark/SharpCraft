#version 330 core
out vec4 result;

in vec2 TexCoord;

uniform sampler2D ourTexture;

void main()
{
    result = texture(ourTexture, TexCoord);
} 