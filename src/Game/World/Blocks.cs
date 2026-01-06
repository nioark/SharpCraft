
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

class YamlBlocks {
    public Dictionary<String, YamlBlock>? blocks;
}

static class Blocks {

    public static int TEXTURE_SIZE = 16;
    public static Dictionary<uint,Block> blocks {get; private set;} = new Dictionary<uint,Block>();
    static Blocks(){

    }

    public static void Initialize(){
        Console.WriteLine("Loading Blocks");
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
        .Build();

        dynamic data = deserializer.Deserialize<YamlBlocks>(File.ReadAllText("res/blocks.yaml"));
        //Console.WriteLine(blocks.blocks["stone"].block_name);

        //Get data from

        Dictionary<int, bool> id_exists = new Dictionary<int, bool>();

        Atlas atlas = new Atlas("res/textures/blocks/");

        foreach (KeyValuePair<string, YamlBlock> entry in data.blocks){
            string key = entry.Key;
            YamlBlock dataBlock = entry.Value;

            #region TEXTURE_NAME_READ

            string[] texturesName = Enumerable.Repeat<string>("null", 6).ToArray(); //All set to "null"

            //Sets all faces to be the same texture
            if (dataBlock.all_sides_texture != null){
                for (int i = 0; i < 6; i++)
                    texturesName[i] = dataBlock.all_sides_texture;
            }

            //Sets just horizontal faces to be the same texture
            if (dataBlock.horizontal_texture != null){
                for (int i = 2; i < 6; i++) //Iterate horizontal faces
                    texturesName[i] = dataBlock.horizontal_texture;
            }

            for (int i = 0; i < 6; i++){
                string? var_name = Enum.GetName(typeof(YamlFaces), i);
                var parameter = dataBlock.GetType().GetField(var_name).GetValue(dataBlock) as string;

                if (parameter != null)
                    texturesName[i] = parameter;

            }

            //Debug all faces
            //foreach(Faces face in Enum.GetValues(typeof(Faces)))
                //Console.WriteLine(face.ToString() + ": "  + texturesName[(int)face]);

            #endregion

            #region ERROR_CHECKING

            if (dataBlock.block_name == null || dataBlock.id == null){
                throw new Exception(String.Format("BLOCK entry {0} INITIALIZING_ERROR: No id found", entry.Key));
            }

            if (!id_exists.TryAdd(dataBlock.id, true)){
                throw new Exception(String.Format("BLOCK entry {0} INITIALIZING_ERROR: ID {1} already exist", entry.Key, dataBlock.id));
            }

            #endregion

            #region FINAL

            ushort[] textureIds = new ushort[6];

            for (int i = 0; i < 6; i++){
                Console.WriteLine(texturesName[i]);
                atlas.TryAddTexture(texturesName[i]);
                textureIds[i] = atlas.getTextureId(texturesName[i]);
                Console.WriteLine(textureIds[i] + " texture_id down");

            }

            for (int i = 0; i < 6; i++){
                Console.Write(", " + textureIds[i]);
            }


            bool isTransparent = dataBlock.transparent;
            Block block = new Block(dataBlock.block_name, (ushort)dataBlock.id, textureIds, texturesName, isTransparent);
            blocks.Add(block.ID,block);
            #endregion
        }
        #region TEXTURE_GENERATION

        atlas.generateAtlas();

        // Initialize ChunkTexture with pre-generated atlas mipmaps
        ChunkTexture.Initialize(atlas.GetAtlasMipmaps());

        Console.WriteLine(blocks.Count());

       foreach (var entry in blocks)
       {
            Block block = entry.Value;
            int face = 0;
            foreach (ushort textureId in block.TexturesIds){
                //Console.WriteLine(textureId + " Texture id " + blocks[i].BlockName);
                float[,,] uvs = atlas.generateUVs(textureId,(Faces)face);
                //Console.WriteLine(uvs.Length);
                block.TexturesUVs.Add(uvs);
                face++;
            }
        }

        blocks.Add(0, null); //just to fill space of air block ID = 0

        Console.WriteLine("Blocks initialized " + blocks.Count() + " total blocks");

        #endregion

    }



}