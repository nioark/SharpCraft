using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

static class ChunkTexture{

    static public GlTexture glTexture { get; private set; }
    static public int id { get; private set; }

    // Initialize with pre-generated atlas mipmaps
    static public void Initialize(List<Image<Rgba32>> atlasMipmaps){
        glTexture = new GlTexture(atlasMipmaps);
        id = glTexture.texture_id;
    }
}