using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;
using System.Runtime.InteropServices;

class GlTexture{

    public int texture_id { get; private set; }

    public unsafe GlTexture(string imagePath){
        Image<Rgba32> image = Image.Load<Rgba32>(imagePath);
        image.Mutate(x => x.Flip(FlipMode.Vertical));

        var _IMemoryGroup = image.GetPixelMemoryGroup();
        var _MemoryGroup = _IMemoryGroup.ToArray()[0];
        byte[] PixelData = MemoryMarshal.AsBytes(_MemoryGroup.Span).ToArray();
        
        int texture = GL.GenTexture();

        fixed(byte *data = PixelData){
            
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
            //float maxTextureMaxAnisotropy = GL.GetFloat((GetPName)All.MaxTextureMaxAnisotropy);
            //GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)All.TextureMaxAnisotropy, maxTextureMaxAnisotropy);
            

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                image.Size().Width,
                image.Size().Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                (IntPtr)data)
            ;

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        this.texture_id = texture;
       
    }
}