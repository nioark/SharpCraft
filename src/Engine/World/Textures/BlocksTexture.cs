public record Texture(string TextureName, uint Id);
public class BlocksTexture{
    
    public List<Texture> textures { get; private set;} = new List<Texture>();
    public BlocksTexture(){

    }

}