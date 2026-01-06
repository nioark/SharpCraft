using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using OpenTK.Mathematics;
using System.Collections.Generic;

class Atlas{

    private string file_path;
    public Dictionary<string, Image<Rgba32>> textures_image {get; private set;} = new Dictionary<string, Image<Rgba32>>();
    private Dictionary<string, ushort> textures_id = new Dictionary<string, ushort>();

    private GlTexture atlas_texture;
    private ushort texture_count = 0;
    private int atlas_pixel_size = 0;
    private int atlas_size_blocks = 0;
    private float normalized_block_texture_size = 0;
    private int lengthBlocks = 0;

    private const byte ALPHA_THRESHOLD = 200; // Must match GlTexture threshold

    public Atlas(string file_path){
        this.file_path = file_path;
    }

    // Generate mipmap level with alpha preservation (for single textures only)
    private Image<Rgba32> GenerateMipLevel(Image<Rgba32> prevLevel){
        int newWidth = Math.Max(1, prevLevel.Width / 2);
        int newHeight = Math.Max(1, prevLevel.Height / 2);
        var mipLevel = new Image<Rgba32>(newWidth, newHeight);

        for (int y = 0; y < newHeight; y++){
            for (int x = 0; x < newWidth; x++){
                // Sample 2x2 area and average colors
                int srcX = x * 2;
                int srcY = y * 2;

                int r = 0, g = 0, b = 0;
                int count = 0;
                bool hasOpaque = false;

                for (int dy = 0; dy < 2 && srcY + dy < prevLevel.Height; dy++){
                    for (int dx = 0; dx < 2 && srcX + dx < prevLevel.Width; dx++){
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
                byte outA = hasOpaque ? (byte)255 : (byte)0;

                mipLevel[x, y] = new Rgba32(outR, outG, outB, outA);
            }
        }

        return mipLevel;
    }

    public bool TryAddTexture(string texture_name){
        Image<Rgba32> img;
       bool l = textures_image.TryGetValue(texture_name, out img);
        if (texture_name == "null") return false;
        Console.WriteLine("Tried to add " + texture_name + " Size: " + textures_image.Count() + " " + l);
        Image<Rgba32> image = Image.Load<Rgba32>(file_path + texture_name + ".png");

        bool g = textures_image.TryAdd(texture_name, image);

        ushort val;
        if (!textures_id.TryGetValue(texture_name, out val)){
            textures_id.TryAdd(texture_name, texture_count);
            texture_count += 1;

        }

        Console.WriteLine(texture_count);
        return  l ;
    }

    public void generateAtlas(){
        lengthBlocks = texture_count + 1;

        double size = Math.Sqrt(lengthBlocks);
        if (((size % 1) == 0) == false) //Check if has decimal place
            size = (int)size + 1;

        Console.WriteLine("Generating atlas with " + size + "x" + size + " size");
        atlas_size_blocks = (int)size;

        normalized_block_texture_size = 1f / (float)atlas_size_blocks;

        int pixel_size = (int)size * Blocks.TEXTURE_SIZE;
        atlas_pixel_size = pixel_size;

        // Generate mipmap levels at base resolution
        List<Image<Rgba32>> atlasMipmaps = new List<Image<Rgba32>>();

        // Base level at original resolution
        Image<Rgba32> currentAtlas = CreateAtlasLevel(pixel_size);
        atlasMipmaps.Add(currentAtlas);
        currentAtlas.Save("ouputNormal.png");

        // Generate mipmap levels (max 5 levels for 16x16 textures)
        int maxLevels = 4;
        for (int i = 1; i < maxLevels && currentAtlas.Width > 1; i++){
            currentAtlas = GenerateMipLevelSimple(currentAtlas);
            atlasMipmaps.Add(currentAtlas);
        }

        // Upscale ALL levels by 16x for final texture
        for (int i = 0; i < atlasMipmaps.Count; i++){
            int scaledWidth = atlasMipmaps[i].Width * 16;
            int scaledHeight = atlasMipmaps[i].Height * 16;
            atlasMipmaps[i].Mutate(o => o.Resize(scaledWidth, scaledHeight, KnownResamplers.NearestNeighbor));
        }

        // Save final atlas (base level)
        atlasMipmaps[0].Save("ouput.png");

        // Save all mipmap levels for debugging
        for (int i = 0; i < atlasMipmaps.Count; i++){
            atlasMipmaps[i].Save("ouput_mip" + i + ".png");
            Console.WriteLine($"Atlas mipmap {i}: {atlasMipmaps[i].Width}x{atlasMipmaps[i].Height}");
        }

        // Store mipmap levels for GlTexture to use
        this.atlasMipmaps = atlasMipmaps;
    }

    // Simple mipmap generation - just downsample without alpha tricks
    private Image<Rgba32> GenerateMipLevelSimple(Image<Rgba32> prevLevel){
        int newWidth = Math.Max(1, prevLevel.Width / 2);
        int newHeight = Math.Max(1, prevLevel.Height / 2);
        var mipLevel = new Image<Rgba32>(newWidth, newHeight);

        for (int y = 0; y < newHeight; y++){
            for (int x = 0; x < newWidth; x++){
                // Sample 2x2 area and take top-left pixel (point sampling)
                int srcX = x * 2;
                int srcY = y * 2;

                // Clamp to bounds
                srcX = Math.Min(srcX, prevLevel.Width - 1);
                srcY = Math.Min(srcY, prevLevel.Height - 1);

                var pixel = prevLevel[srcX, srcY];
                mipLevel[x, y] = pixel;
            }
        }

        return mipLevel;
    }

    private List<Image<Rgba32>> atlasMipmaps = new List<Image<Rgba32>>();

    // Get mipmap levels for texture loading
    public List<Image<Rgba32>> GetAtlasMipmaps(){
        return atlasMipmaps;
    }

    private Image<Rgba32> CreateAtlasLevel(int pixelSize){
        var outputImage = new Image<Rgba32>(pixelSize, pixelSize);

        int x = 0;
        int y = pixelSize - Blocks.TEXTURE_SIZE;
        foreach (KeyValuePair<string, Image<Rgba32>> entry in textures_image){
            if (x > pixelSize - 1){
                x = 0;
                y -= Blocks.TEXTURE_SIZE;
            }

            Image<Rgba32> img = entry.Value;
            outputImage.Mutate(o => o.DrawImage(img, new Point(x, y), 1f));
            x += Blocks.TEXTURE_SIZE;
        }

        return outputImage;
    }
    public float pixelToFloat(int pixel_coordinate){
        return (float)(pixel_coordinate / atlas_pixel_size);
    }

    public float idToFloat(int id){
        return pixelToFloat(id * Blocks.TEXTURE_SIZE);
    }

    public float[,,] generateUVs(ushort texture_id, Faces face){
        int yHeight = (int)((float)texture_id / (float)atlas_size_blocks);
        float y = (float)yHeight / (float)atlas_size_blocks;
        float x = ((float)texture_id / (float)atlas_size_blocks) - yHeight;

        Console.WriteLine(x  + " " +  y + " " + texture_id);

        Vector2 topLeft = new Vector2(x, y + normalized_block_texture_size);
        Vector2 topRight = new Vector2(x + normalized_block_texture_size, y + normalized_block_texture_size);
        Vector2 bottomLeft = new Vector2(x, y);
        Vector2 bottomRight = new Vector2(x + normalized_block_texture_size, y);

        //Console.WriteLine(texture_id);

        float[,,] uvs = {
            { {0,0}, {0,0}, {0,0}}, //Top
            { {0,0}, {0,0}, {0,0}}
        };

        switch (face){
            case Faces.Top:
            {
                uvs = new float[,,]{
                    { {bottomLeft.X,bottomLeft.Y}, {topLeft.X, topLeft.Y}, {topRight.X, topRight.Y}}, //Top
                    { {bottomLeft.X,bottomLeft.Y}, {topRight.X, topRight.Y}, {bottomRight.X, bottomRight.Y}}
                };
                break;
            }
            case Faces.Bottom:
            {
                uvs = new float[,,]{
                    { {bottomLeft.X,bottomLeft.Y}, {topRight.X, topRight.Y}, {topLeft.X, topLeft.Y}}, //Top
                    { {bottomLeft.X,bottomLeft.Y}, {bottomRight.X, bottomRight.Y}, {topRight.X, topRight.Y}}
                };
                break;
            }
            case Faces.Front:
            {
                uvs = new float[,,]{
                    { {bottomLeft.X,bottomLeft.Y}, {topLeft.X, topLeft.Y}, {topRight.X, topRight.Y}}, //Top
                    { {bottomLeft.X,bottomLeft.Y}, {topRight.X, topRight.Y}, {bottomRight.X, bottomRight.Y}}
                };
                break;
            }
            case Faces.Back:
            {
                uvs = new float[,,]{
                    { {bottomLeft.X,bottomLeft.Y}, {topRight.X, topRight.Y}, {topLeft.X, topLeft.Y}}, //Top
                    { {bottomLeft.X,bottomLeft.Y}, {bottomRight.X, bottomRight.Y}, {topRight.X, topRight.Y}}
                };
                break;
            }
            case Faces.Left:
            {
                uvs = new float[,,]{
                    { {bottomLeft.X,bottomLeft.Y}, {topRight.X, topRight.Y}, {topLeft.X, topLeft.Y}}, //Top
                    { {bottomLeft.X,bottomLeft.Y}, {bottomRight.X, bottomRight.Y}, {topRight.X, topRight.Y}}
                };
                break;
            }
            case Faces.Right:
            {
                uvs = new float[,,]{
                    { {bottomLeft.X,bottomLeft.Y}, {topLeft.X, topLeft.Y}, {topRight.X, topRight.Y}}, //Top
                    { {bottomLeft.X,bottomLeft.Y}, {topRight.X, topRight.Y}, {bottomRight.X, bottomRight.Y}}
                };
                break;
            }
            default:
            {
                break;
            }
        }

        return uvs;


    }
    public ushort getTextureId(string texture_name){
        return textures_id[texture_name];
    }

}
