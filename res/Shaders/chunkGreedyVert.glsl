#version 330 core
layout (location = 0) in uint inVertexData;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec2 TexCoord;

void main()
{   
    float x = float(inVertexData & 0x3Fu);
    float y = float((inVertexData & 0xFC0u) >> 6u);
    float z = float((inVertexData & 0x3F000u) >> 12u);

    gl_Position = vec4(x,y,z, 1.0) * model * view * projection;
    TexCoord = vec2(1,1);
}