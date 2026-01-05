using OpenTK.Mathematics;

class Chunk
{

    #region COMPONENTS
    private MeshRenderer MeshRenderer;
    private ChunkMesh chunkMesh;


    #endregion

    //private Dictionary<int, int> yCollum = new Dictionary<int,int>();
    #region INFORMATIVE_VARIABLES
    public bool readyToRender { get; set; } = false;
    public bool isGenerated { get; private set; } = false;
    public bool isPostGenerated { get; private set; } = false;
    public bool hasMesh { get; private set; } = false;
    public bool hasNeighbors = false;
    private float genTime;

    private int chunkEnd = World.CHUNK_SIZE - 1;

    #endregion

    #region DATA_VARIABLES
    private List<float> meshData = new List<float>();
    public Vector2 chunkPos { get; private set; }
    public Chunk? leftNeighbor, rightNeighbor, frontNeighbor, backNeighbor;
    private int CHUNK_1D_SIZE = World.CHUNK_SIZE * World.CHUNK_HEIGHT;
    private int CHUNK_SIZE = World.CHUNK_SIZE;
    private byte[] voxelMap = new byte[World.CHUNK_SIZE * World.CHUNK_HEIGHT * World.CHUNK_SIZE];
    private byte[,] heightMap = new byte[World.CHUNK_SIZE, World.CHUNK_SIZE];

    #endregion

    private Generation generation;

    public Chunk(Vector2 chunkPos, GlShader shader, ref Generation generation)
    {
        this.generation = generation;
        this.chunkMesh = new ChunkMesh();
        this.MeshRenderer = new MeshRenderer(new Vector3(chunkPos.X * World.CHUNK_SIZE, 0f, chunkPos.Y * World.CHUNK_SIZE), shader);
        this.chunkPos = chunkPos;
    }

    private int to1D(int x, int y, int z)
    {
        return (z * CHUNK_1D_SIZE) + (y * CHUNK_SIZE) + x;
    }

    public byte getVoxelMapBlock(int x, int y, int z)
    {
        return voxelMap[to1D(x, y, z)];
    }

    public void setVoxelMapBlock(int x, int y, int z, byte id)
    {
        voxelMap[to1D(x, y, z)] = id;
    }

    public void GenerateLandScape()
    {
        var watch = new System.Diagnostics.Stopwatch();

        watch.Start();

        Tuple<byte[], byte[,]> output = generation.getChunkLandScape((int)chunkPos.X, (int)chunkPos.Y);
        this.voxelMap = output.Item1;
        this.heightMap = output.Item2;

        isGenerated = true;

        watch.Stop();

        genTime = (float)watch.ElapsedTicks / 10000;
    }

    public void GeneratePostScape()
    {
        generation.addChunkPostScape((int)chunkPos.X, (int)chunkPos.Y, ref this.voxelMap, ref this.heightMap);
        isPostGenerated = true;
    }

    private void addBlock(int x, int y, int z)
    {
        ushort id = voxelMap[to1D(x, y, z)];

        if (id == 0)
            return;

        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
        {
            int neighborX = VoxelData.offsets[faceIndex, 0] + x;
            int neighborY = VoxelData.offsets[faceIndex, 1] + y;
            int neighborZ = VoxelData.offsets[faceIndex, 2] + z;

            if (shouldRenderFace(faceIndex, neighborX, neighborY, neighborZ, x, y, z, id))
            {
                chunkMesh.addFace(faceIndex, x, y, z, id, ref meshData);
            }
        }
    }

    private bool shouldRenderFace(int faceIndex, int neighborX, int neighborY, int neighborZ, int x, int y, int z, ushort blockId)
    {
        // Check horizontal neighbors (cross-chunk boundaries)
        if (neighborX >= World.CHUNK_SIZE)
            return isBlockTransparent(rightNeighbor, 0, y, z);

        if (neighborX < 0)
            return isBlockTransparent(leftNeighbor, chunkEnd, y, z);

        if (neighborZ >= World.CHUNK_SIZE)
            return isBlockTransparent(frontNeighbor, x, y, 0);

        if (neighborZ < 0)
            return isBlockTransparent(backNeighbor, x, y, chunkEnd);

        // Check vertical boundaries
        if (neighborY >= World.CHUNK_HEIGHT)
            return false;

        if (neighborY < 0)
            return true;

        // Check same-chunk neighbor
        return isBlockTransparent(this, neighborX, neighborY, neighborZ);
    }

    private bool isBlockTransparent(Chunk chunk, int x, int y, int z)
    {
        if (chunk == null)
            return true;

        ushort blockId = chunk.voxelMap[to1D(x, y, z)];
        
        if (blockId == 0)
            return true;

        return Blocks.blocks[blockId].Transparent;
    }

    private bool isNeighborBlockTransparent(Chunk neighbor, int x, int y, int z)
    {
        return neighbor == null || neighbor.voxelMap[to1D(x, y, z)] == 0;
    }

    public void GenerateMesh()
    {
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        for (int y = 0; y < World.CHUNK_HEIGHT; y++)
        {
            for (int x = 0; x < World.CHUNK_SIZE; x++)
                for (int z = 0; z < World.CHUNK_SIZE; z++)
                    addBlock(x, y, z);
        }

        hasMesh = true;


        watch.Stop();
        //Console.WriteLine($" Gen time: {genTime} ms, ChunkMesh Time: {(float)watch.ElapsedTicks / 10000} ms");
    }

    public void MeshUpload()
    {
        MeshRenderer.UploadMeshGPU(ref meshData);
        readyToRender = true;

        meshData = null;
    }


    public void ClearGPUMesh()
    {
        MeshRenderer.clearBuffers();
    }
    public void Render()
    {
        MeshRenderer.draw();
    }

}