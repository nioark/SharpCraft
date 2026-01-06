using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;
using System.Runtime.InteropServices;
using System.Collections.Generic;

class GlTexture{

    public int texture_id { get; private set; }

    public unsafe GlTexture(string imagePath){
        Image<Rgba32> image = Image.Load<Rgba32>(imagePath);
        image.Mutate(x => x.Flip(FlipMode.Vertical));

        int texture = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, texture);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);

        UploadMipLevel(image, 0);
        this.texture_id = texture;
    }

    // Constructor for atlas with pre-generated mipmaps
    public unsafe GlTexture(List<Image<Rgba32>> mipmaps){
        int texture = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, texture);

        // Set texture parameters BEFORE uploading data
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        // Use mipmaps only if we have more than one level
        if (mipmaps.Count > 1){
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mipmaps.Count - 1);
            // Negative LOD bias makes the GPU favor higher-res mipmaps for longer distance
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, -5f);
        } else {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
        }

        // Upload all mipmap levels
        for (int level = 0; level < mipmaps.Count; level++){
            var image = mipmaps[level];
            Console.WriteLine($"Uploading mipmap level {level}: {image.Width}x{image.Height}");
            image.Mutate(x => x.Flip(FlipMode.Vertical));
            UploadMipLevel(image, level);
        }
        Console.WriteLine($"TextureMaxLevel set to: {mipmaps.Count - 1}");

        this.texture_id = texture;
    }

    private unsafe void UploadMipLevel(Image<Rgba32> image, int level){
        var memoryGroup = image.GetPixelMemoryGroup();
        var memory = memoryGroup.ToArray()[0];
        byte[] pixelData = MemoryMarshal.AsBytes(memory.Span).ToArray();

        fixed(byte* data = pixelData){
            GL.TexImage2D(
                TextureTarget.Texture2D,
                level,
                PixelInternalFormat.Rgba,
                image.Width,
                image.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                (IntPtr)data
            );
        }
    }
}
