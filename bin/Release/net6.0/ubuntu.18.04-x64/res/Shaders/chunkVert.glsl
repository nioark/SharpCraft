#version 330 core
layout (location = 0) in vec3 pos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float light;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec2 TexCoord;
out float outLight;

void main()
{
    gl_Position = vec4(pos, 1.0) * model * view * projection;
    TexCoord = aTexCoord;
    outLight = light;
}