
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

    private Vector2 lastTreePos = new Vector2(float.MinValue, float.MinValue);
    private const float MIN_TREE_SPACING = 8.0f;
    private TreeGenerator? treeGenerator;

    public void genSurfaceTree(ref byte[] voxelMap, ref byte[,] heightMap, int xChunk, int zChunk)
    {
        // Initialize tree generator if needed
        if (treeGenerator == null)
        {
            treeGenerator = new TreeGenerator(world, _mainNoise.GetSeed());
        }

        for (int x = 0; x < World.CHUNK_SIZE; x++)
        {
            for (int z = 0; z < World.CHUNK_SIZE; z++)
            {
                int xWorld = x + (xChunk * World.CHUNK_SIZE);
                int zWorld = z + (zChunk * World.CHUNK_SIZE);

                int height = heightMap[x, z];

                // Get biome for this position
                Biomes biome = getBiome(xWorld, zWorld);

                // Check if this biome supports trees
                TreeGenerator.TreeType? treeType = GetTreeTypeForBiome(biome);
                if (treeType == null)
                    continue;

                // Get spawn chance for this biome
                float spawnChance = GetTreeChance(biome);
                if (random.NextDouble() > spawnChance)
                    continue;

                // Check minimum spacing from other trees
                if (TooCloseToExistingTree(xWorld, zWorld))
                    continue;

                // Generate the tree
                treeGenerator.GenerateTree(xWorld, height, zWorld, treeType.Value);
            }
        }
    }

    /// <summary>
    /// Determines which tree type spawns in a given biome
    /// </summary>
    private TreeGenerator.TreeType? GetTreeTypeForBiome(Biomes biome)
    {
        switch (biome)
        {
            case Biomes.Forest:
            case Biomes.RainForest:
                return TreeGenerator.TreeType.Large;

            case Biomes.SeasonalForest:
            case Biomes.Taiga:
            case Biomes.Plains:
            case Biomes.Swamp:
            case Biomes.Savanna:
                return TreeGenerator.TreeType.Small;

            default:
                return null; // No trees in desert, tundra, shrubland, stone
        }
    }

    /// <summary>
    /// Returns the spawn chance for trees in a given biome (0.0 to 1.0)
    /// </summary>
    private float GetTreeChance(Biomes biome)
    {
        switch (biome)
        {
            case Biomes.RainForest: return 0.12f;
            case Biomes.Forest: return 0.08f;
            case Biomes.SeasonalForest: return 0.06f;
            case Biomes.Taiga: return 0.05f;
            case Biomes.Swamp: return 0.04f;
            case Biomes.Savanna: return 0.03f;
            case Biomes.Plains: return 0.01f;
            default: return 0f;
        }
    }

    /// <summary>
    /// Checks if a position is too close to an existing tree
    /// </summary>
    private bool TooCloseToExistingTree(int xWorld, int zWorld)
    {
        float distance = Vector2.Distance(
            new Vector2(xWorld, zWorld),
            lastTreePos
        );

        if (distance < MIN_TREE_SPACING && lastTreePos.X != float.MinValue)
            return true;

        lastTreePos = new Vector2(xWorld, zWorld);
        return false;
    }
    public void genSurfaceMaterial(ref byte[] voxelMap, ref byte[,] heightMap, int xChunk, int yChunk)
    {

        for (int x = 0; x < World.CHUNK_SIZE; x++)
            for (int z = 0; z < World.CHUNK_SIZE; z++)
            {
                int xWorld = x + (xChunk * World.CHUNK_SIZE);
                int zWorld = z + (yChunk * World.CHUNK_SIZE);

                int height = heightMap[x, z];

                Biomes biome = getBiome(xWorld, zWorld);
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