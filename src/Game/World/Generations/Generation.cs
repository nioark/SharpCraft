public class Generation{

    private FastNoise noise;
    private LandScape landScape;

    private BiomeScape biomeScape;

    public Generation(FastNoise _noise, World world){
        noise = _noise;
        landScape = new LandScape(_noise);
        biomeScape = new BiomeScape(_noise, ref world);
    }

    public Tuple<byte [], byte [,]> getChunkLandScape(int chunkX, int chunkY){
        byte [] voxelMap = new byte [World.CHUNK_SIZE * World.CHUNK_HEIGHT * World.CHUNK_SIZE];
        byte [,] heightMap = new byte[World.CHUNK_SIZE,World.CHUNK_SIZE];
        #region LANDSCAPE_GENERATION

        landScape.genLandScape(ref voxelMap, ref heightMap, chunkX, chunkY);
        landScape.gen3dNoise(ref voxelMap, ref heightMap, chunkX, chunkY);

        #endregion

        return Tuple.Create(voxelMap,heightMap);
    }

    public void addChunkPostScape(int chunkX, int chunkY, ref byte [] voxelMap, ref byte [,] heightMap){

        #region BIOME_SURFACE_MATERIAL

        biomeScape.genSurfaceMaterial(ref voxelMap, ref heightMap, chunkX, chunkY);

        #endregion

        #region BIOME_TREES

        biomeScape.genSurfaceTree(ref voxelMap, ref heightMap, chunkX, chunkY);

        #endregion
    }
}

public static class GenUtil{
    private static int CHUNK_1D_SIZE = World.CHUNK_SIZE * World.CHUNK_HEIGHT;
    private static int CHUNK_SIZE = World.CHUNK_SIZE;

    public static int to1D( int x, int y, int z ) {
        return (z * CHUNK_1D_SIZE) + (y * CHUNK_SIZE ) + x;
    }

    public static float Lerp (float x0, float x1, float y0, float y1, float x)
    {
        float d = x1 - x0;
        if (d == 0)
            return (y0 + y1) / 2;
        return y0 + (x - x0) * (y1 - y0) / d;
    }

    public static float Lerp (float[] Xs, float[] Ys, float x)
    {
        // Finds the right interval
        int index = Array.BinarySearch(Xs, x);
        // If the index is non-negative
        // an exact match has been found!
        if (index >= 0)
            return Ys[index];
        // If the index is negative, it represents the bitwise
        // complement of the next larger element in the array.
        index = ~index;
        // index == 0           => result smaller than Ys[0]
        if (index == 0)
            return Ys[0];
        // index == Ys.Length   => result greater than Ys[Ys.Length-1]
        if (index == Ys.Length)
            return Ys[Ys.Length - 1];
        // else                 => result between Ys[index-1] and Ys[index]
        // Lerp
        return Lerp
        (
            Xs[index - 1], Xs[index],
            Ys[index - 1], Ys[index],
            x
        );
    }
}
