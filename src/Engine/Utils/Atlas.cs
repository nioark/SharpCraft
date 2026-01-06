using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

class Atlas
{
    private string file_path;
    public Dictionary<string, Image<Rgba32>> textures_image {get; private set;} = new Dictionary<string, Image<Rgba32>>();
    private Dictionary<string, ushort> textures_layer = new Dictionary<string, ushort>();

    public Atlas(string file_path){
        this.file_path = file_path;
    }

    public ushort TryAddTexture(string texture_name){
        if (texture_name == "null") return 0;

        // Return existing layer if already loaded
        if (textures_layer.TryGetValue(texture_name, out ushort existingLayer))
            return existingLayer;

        // Load new texture and assign layer
        ushort layer = (ushort)textures_image.Count;
        Image<Rgba32> image = Image.Load<Rgba32>(file_path + texture_name + ".png");

        textures_image[texture_name] = image;
        textures_layer[texture_name] = layer;

        Console.WriteLine($"Added texture: {texture_name} -> layer {layer}");
        return layer;
    }

    // Returns the individual texture images (not a packed atlas)
    public List<Image<Rgba32>> GetTextureLayers(){
        return new List<Image<Rgba32>>(textures_image.Values);
    }

    public ushort GetTextureLayer(string texture_name){
        return textures_layer[texture_name];
    }
}
