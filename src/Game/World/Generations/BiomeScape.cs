
using System.Diagnostics;
using OpenTK.Mathematics;

class BiomeScape
{
    FastNoise _mainNoise;
    FastNoise tempNoise;
    FastNoise humidyNoise;

    private World world;

    Random random;

    public enum Biomes
    {
        RainForest,
        Swamp,
        SeasonalForest,
        Forest,
        Savanna,
        Shrubland,
        Taiga,
        Tundra,
        Plains,
        Desert,
        Stone,
    }

    public BiomeScape(FastNoise noise, ref World world)
    {
        _mainNoise = noise;
        tempNoise = new FastNoise(noise.GetSeed() + 255212121);
        humidyNoise = new FastNoise(noise.GetSeed() + 52525251);
        random = new Random();
        this.world = world;
    }

    public float getTemperature(int x, int z)
    {
        float frequency = 0.005f;

        return GenUtil.Lerp(-1f, 1f, -20, 100, tempNoise.GetSimplex(x * frequency, z * frequency));
    }

    public float getHumidy(int x, int z)
    {
        float frequency = 0.5f;

        return GenUtil.Lerp(-1f, 1f, 0, 100, humidyNoise.GetSimplex(x * frequency, z * frequency));
    }

    public Biomes getBiome(int x, int z)
    {
        float temp = getTemperature(x, z);
        float rainfall = getHumidy(x, z);
        if (temp < 25 && rainfall < 50)
            return Biomes.Tundra;
        else if (temp < 50 && rainfall < 75)
            return Biomes.Taiga;
        else if (temp < 65 && rainfall < 25)
            return Biomes.Plains;
        else if (temp < 75 && rainfall < 50)
            return Biomes.Shrubland;
        else if (temp < 75 && rainfall < 75)
            return Biomes.Forest;
        else if (temp < 85 && rainfall < 80)
            return Biomes.Swamp;
        else if (temp < 100 && rainfall < 100)
            return Biomes.RainForest;
        else if (temp < 75 && rainfall < 75)
            return Biomes.SeasonalForest;
        else if (temp < 75 && rainfall < 50)
            return Biomes.Savanna;
        else if (temp < 100 && rainfall < 25)
            return Biomes.Desert;


        return Biomes.Stone;
    }

    Vector2 lastPos = new Vector2(0, 0);
    public void genSurfaceTree(ref byte[] voxelMap, ref byte[,] heightMap, int xChunk, int yChunk)
    {
        for (int x = 0; x < World.CHUNK_SIZE; x++)
            for (int z = 0; z < World.CHUNK_SIZE; z++)
            {
                int xWorld = x + (Math.Abs(xChunk) * World.CHUNK_SIZE);
                int zWorld = z + (Math.Abs(yChunk) * World.CHUNK_SIZE);

                if (xChunk < 0)
                    xWorld *= -1;

                if (yChunk < 0)
                    zWorld *= -1;

                int height = heightMap[x, z];

                float freq = 1.5f;
                float chance = random.Next(0, 100);
                if (chance == 5)
                {

                    if (Vector2.Distance(new Vector2(xWorld, zWorld), lastPos) > 20)
                    {
                        lastPos = new Vector2(xWorld, zWorld);


                        int tree_maxheight = random.Next(5, 7);
                        int tree_height = 0;
                        for (int iX = -2; iX < 3; iX++)
                            for (int iY = -2; iY < 1; iY++)
                                for (int iZ = -2; iZ < 3; iZ++)
                                {
                                    world.SetBlock(xWorld + iX, height + tree_maxheight + iY, zWorld + iZ, 8);
                                }

                        for (int i = 1; i < tree_maxheight; i++)
                        {
                            voxelMap[GenUtil.to1D(x, height + i, z)] = 7;
                            //Console.WriteLine("Original pos: " + x + " " + z + " || " + xWorld + " " + zWorld + " || " + xChunk + " " + yChunk);
                            tree_height = i;
                        }

                        //world.SetBlock(xWorld, tree_height + 1, zWorld, 5);
                    }


                }

            }
    }
    public void genSurfaceMaterial(ref byte[] voxelMap, ref byte[,] heightMap, int xChunk, int yChunk)
    {

        for (int x = 0; x < World.CHUNK_SIZE; x++)
            for (int z = 0; z < World.CHUNK_SIZE; z++)
            {
                int xWorld = x + (xChunk * World.CHUNK_SIZE);
                int zWorld = z + (yChunk * World.CHUNK_SIZE);

                int height = heightMap[x, z];

                //Biomes biome = getBiome(xWorld, zWorld);
                Biomes biome = Biomes.Taiga;
                switch (biome)
                {
                    case Biomes.Desert:
                        {
                            voxelMap[GenUtil.to1D(x, height, z)] = 4;
                            voxelMap[GenUtil.to1D(x, height - 2, z)] = 3;
                            voxelMap[GenUtil.to1D(x, height - 1, z)] = 3;
                            break;
                        }
                    case Biomes.RainForest:
                        {
                            voxelMap[GenUtil.to1D(x, height, z)] = 1;
                            voxelMap[GenUtil.to1D(x, height - 2, z)] = 2;
                            voxelMap[GenUtil.to1D(x, height - 1, z)] = 2;
                            break;
                        }
                    case Biomes.Forest:
                        {
                            voxelMap[GenUtil.to1D(x, height, z)] = 1;
                            voxelMap[GenUtil.to1D(x, height - 2, z)] = 2;
                            voxelMap[GenUtil.to1D(x, height - 1, z)] = 2;
                            break;
                        }
                    case Biomes.Swamp:
                        {
                            voxelMap[GenUtil.to1D(x, height, z)] = 2;
                            voxelMap[GenUtil.to1D(x, height - 2, z)] = 2;
                            voxelMap[GenUtil.to1D(x, height - 1, z)] = 2;
                            break;
                        }
                    case Biomes.Savanna:
                        {
                            voxelMap[GenUtil.to1D(x, height, z)] = 5;
                            voxelMap[GenUtil.to1D(x, height - 2, z)] = 2;
                            voxelMap[GenUtil.to1D(x, height - 1, z)] = 2;
                            break;
                        }
                    default:
                        {
                            voxelMap[GenUtil.to1D(x,height,z)] = 1;
                            voxelMap[GenUtil.to1D(x,height - 1,z)] = 2;
                            voxelMap[GenUtil.to1D(x,height - 2,z)] = 2;
                            voxelMap[GenUtil.to1D(x,height - 3,z)] = 6;
                            break;
                        }


                }

            }

    }
}