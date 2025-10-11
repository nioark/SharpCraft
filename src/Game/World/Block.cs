
public enum Faces : ushort{
    Top = 0,
    Bottom = 1,
    Front = 2,
    Back = 3,
    Left = 4,
    Right = 5
}

public enum YamlFaces : ushort {
    top_texture = 0,
    bottom_texture = 1,
    front_texture = 2,
    back_texture = 3,
    left_texture = 4,
    right_texture = 5
}

class YamlBlock {
    public int id;
    public string? block_name;
    public string? all_sides_texture;
    public string? horizontal_texture;
    public string? top_texture;
    public string? bottom_texture;
    public string? front_texture;
    public string? back_texture;
    public string? left_texture;
    public string? right_texture;
}

public class Block {

    public string BlockName {get; private set;}
    public string[] TexturesName  {get; private set;} = new string[6];
    public ushort[] TexturesIds  {get; private set;} = new ushort[6];

    public List<float[,,]> TexturesUVs {get; private set;} = new List<float[,,]>();
    public uint ID {get; private set;}

    public Block(string name, uint id, ushort[] TexturesIds, string[] TexturesName){
        this.BlockName = name;
        this.ID = id;
        this.TexturesName = TexturesName;
        this.TexturesIds = TexturesIds;
        this.TexturesUVs = TexturesUVs;
    }

    public void setUvs(List<float[,,]>  TexturesUVs){
        this.TexturesUVs = TexturesUVs;
    }
    
    public string GetTextureName(Faces face){
        return TexturesName[((ushort)face)];
    }

    public ushort GetTextureIds(Faces face){
        return TexturesIds[((ushort)face)];
    }


}


