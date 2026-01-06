using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

static class ChunkTexture{

    static public TextureArray textureArray { get; private set; }
    static public int id => textureArray?.TextureID ?? 0;

    // Initialize with texture array
    static public void Initialize(TextureArray texArray){
        textureArray = texArray;
    }
}
