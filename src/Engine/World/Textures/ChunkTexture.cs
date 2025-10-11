static class ChunkTexture{
    
    static public GlTexture glTexture { get; private set; } = new GlTexture("ouput.png");
    static public int id { get; private set; } = glTexture.texture_id;
}