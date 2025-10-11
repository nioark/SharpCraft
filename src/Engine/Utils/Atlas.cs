using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using OpenTK.Mathematics;

class Atlas{

    private string file_path;
    public Dictionary<string, Image> textures_image {get; private set;} = new Dictionary<string, Image>(); 
    private Dictionary<string, ushort> textures_id = new Dictionary<string, ushort>();

    private GlTexture atlas_texture;
    private ushort texture_count = 0;
    private int atlas_pixel_size = 0;
    private int atlas_size_blocks = 0;
    private float normalized_block_texture_size = 0;
    private int lengthBlocks = 0;
    public Atlas(string file_path){
        this.file_path = file_path;
    }

    public bool TryAddTexture(string texture_name){
        Image img;
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

        using (Image<Rgba32> outputImage = new Image<Rgba32>(pixel_size, pixel_size)) // create output image of the correct dimensions
        {

            // take the 2 source images and draw them onto the image
            outputImage.Mutate(o => {
                
                int x = 0;
                int y = atlas_pixel_size - Blocks.TEXTURE_SIZE;
                foreach (KeyValuePair<string, Image> entry in textures_image){
                    //Console.WriteLine(x + " " + y + " max: " +  atlas_pixel_size);
                    if (x > atlas_pixel_size - 1){
                        x = 0;
                        y -= Blocks.TEXTURE_SIZE;
                    }
                
                    Image img = entry.Value;
                    //Console.WriteLine(x + " " + y + " tried");
                    o.DrawImage(img, new Point(x, y), 1f);
                    x += Blocks.TEXTURE_SIZE;
                }

            }
               
            );

            outputImage.Save("ouputNormal.png");
            outputImage.Mutate(o => o.Resize(pixel_size * 16, pixel_size * 16, KnownResamplers.NearestNeighbor)); //SCALE TO REMOVEM MOIRE PATTERNS FROM TEXTURE
            outputImage.Save("ouput.png");
        }

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