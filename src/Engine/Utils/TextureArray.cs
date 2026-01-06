using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;
using System.Runtime.InteropServices;
using System.Collections.Generic;

class TextureArray
{
    public int TextureID { get; private set; }
    public int LayerCount { get; private set; }

    public unsafe TextureArray(List<Image<Rgba32>> textures)
    {
        LayerCount = textures.Count;
        int textureSize = textures[0].Width;

        int textureArrayID = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2DArray, textureArrayID);

        // Allocate storage for all layers
        GL.TexImage3D(
            TextureTarget.Texture2DArray,
            0,                          // level 0
            PixelInternalFormat.Rgba,
            textureSize,                // width
            textureSize,                // height
            textures.Count,             // depth = number of layers
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            IntPtr.Zero                 // null = allocate only
        );

        // Upload each texture to its layer
        for (int layer = 0; layer < textures.Count; layer++)
        {
            var texture = textures[layer];
            texture.Mutate(x => x.Flip(FlipMode.Vertical));

            var memoryGroup = texture.GetPixelMemoryGroup();
            var memory = memoryGroup.ToArray()[0];
            byte[] pixelData = MemoryMarshal.AsBytes(memory.Span).ToArray();

            fixed (byte* data = pixelData)
            {
                GL.TexSubImage3D(
                    TextureTarget.Texture2DArray,
                    0,                          // mipmap level
                    0, 0, layer,                // x, y, z offsets
                    texture.Width,
                    texture.Height,
                    1,                          // depth (1 layer at a time)
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    (IntPtr)data
                );
            }
        }

        // Generate mipmaps for the entire array
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

        // Set texture parameters
        GL.TexParameter(TextureTarget.Texture2DArray,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.NearestMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2DArray,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);

        // Enable anisotropic filtering
        float maxAniso = GL.GetFloat((GetPName)0x84FF); // GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT
        GL.TexParameter(TextureTarget.Texture2DArray,
            (TextureParameterName)0x84FE, // GL_TEXTURE_MAX_ANISOTROPY_EXT
            maxAniso);

        GL.TexParameter(TextureTarget.Texture2DArray,
            TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2DArray,
            TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);

        TextureID = textureArrayID;

        Console.WriteLine($"Texture array created: {textures.Count} layers, {textureSize}x{textureSize}");
        Console.WriteLine($"Anisotropic filtering: {maxAniso}x");
    }
}
