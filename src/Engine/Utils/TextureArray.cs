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

    private const byte ALPHA_THRESHOLD = 200; // Must match fragment shader threshold

    public unsafe TextureArray(List<Image<Rgba32>> textures)
    {
        LayerCount = textures.Count;
        int textureSize = textures[0].Width;

        // Calculate max mipmap levels (log2 of texture size)
        int maxLevels = 1;
        int size = textureSize;
        while (size > 1)
        {
            size /= 2;
            maxLevels++;
        }

        int textureArrayID = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2DArray, textureArrayID);

        // Allocate and upload base level (level 0)
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

        // Upload base level textures
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
                    0,                          // mipmap level 0
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

        // Generate and upload mipmaps with alpha preservation
        List<Image<Rgba32>> currentMips = new List<Image<Rgba32>>(textures);
        for (int level = 1; level < maxLevels; level++)
        {
            int mipSize = Math.Max(1, textureSize >> level);

            // Allocate storage for this mipmap level
            GL.TexImage3D(
                TextureTarget.Texture2DArray,
                level,
                PixelInternalFormat.Rgba,
                mipSize,
                mipSize,
                textures.Count,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                IntPtr.Zero
            );

            // Generate and upload each layer's mipmap
            for (int layer = 0; layer < textures.Count; layer++)
            {
                Image<Rgba32> mipLevel = GenerateMipLevel(currentMips[layer]);
                currentMips[layer] = mipLevel; // Store for next iteration

                mipLevel.Mutate(x => x.Flip(FlipMode.Vertical));

                var memoryGroup = mipLevel.GetPixelMemoryGroup();
                var memory = memoryGroup.ToArray()[0];
                byte[] pixelData = MemoryMarshal.AsBytes(memory.Span).ToArray();

                fixed (byte* data = pixelData)
                {
                    GL.TexSubImage3D(
                        TextureTarget.Texture2DArray,
                        level,
                        0, 0, layer,
                        mipLevel.Width,
                        mipLevel.Height,
                        1,
                        PixelFormat.Rgba,
                        PixelType.UnsignedByte,
                        (IntPtr)data
                    );
                }
            }
        }

        // Set texture parameters
        GL.TexParameter(TextureTarget.Texture2DArray,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.NearestMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2DArray,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);

        // Set max mipmap level
        GL.TexParameter(TextureTarget.Texture2DArray,
            TextureParameterName.TextureMaxLevel,
            maxLevels - 1);

        // GL.TexParameter(TextureTarget.Texture2DArray,
        //       TextureParameterName.TextureLodBias,
        //       -2.0f);  // Try values between -5.0 (sharper) to 0.0 (default)

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

        Console.WriteLine($"Texture array created: {textures.Count} layers, {textureSize}x{textureSize}, {maxLevels} mipmap levels");
        Console.WriteLine($"Anisotropic filtering: {maxAniso}x");
    }

    // Generate mipmap level with alpha preservation
    private Image<Rgba32> GenerateMipLevel(Image<Rgba32> prevLevel)
    {
        int newWidth = Math.Max(1, prevLevel.Width / 2);
        int newHeight = Math.Max(1, prevLevel.Height / 2);
        var mipLevel = new Image<Rgba32>(newWidth, newHeight);

        for (int y = 0; y < newHeight; y++)
        {
            int srcY = y * 2;
            for (int x = 0; x < newWidth; x++)
            {
                int srcX = x * 2;

                int r = 0, g = 0, b = 0;
                int count = 0;
                bool hasOpaque = false;

                // Sample 2x2 area
                for (int dy = 0; dy < 2 && srcY + dy < prevLevel.Height; dy++)
                {
                    for (int dx = 0; dx < 2 && srcX + dx < prevLevel.Width; dx++)
                    {
                        var pixel = prevLevel[srcX + dx, srcY + dy];
                        r += pixel.R;
                        g += pixel.G;
                        b += pixel.B;

                        if (pixel.A > ALPHA_THRESHOLD)
                            hasOpaque = true;

                        count++;
                    }
                }

                byte outR = (byte)(r / count);
                byte outG = (byte)(g / count);
                byte outB = (byte)(b / count);
                // Preserve alpha: if any pixel was opaque, output is fully opaque
                byte outA = hasOpaque ? (byte)255 : (byte)0;

                mipLevel[x, y] = new Rgba32(outR, outG, outB, outA);
            }
        }

        return mipLevel;
    }
}
