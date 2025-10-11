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
        //Console.WriteLine( x + " " + y + " " + z );
        ushort id = voxelMap[to1D(x, y, z)];

        if (id == 0)
            return;

        for (int i = 0; i < 6; i++)
        {
            int xBlock = VoxelData.offsets[i, 0] + x;
            int yBlock = VoxelData.offsets[i, 1] + y;
            int zBlock = VoxelData.offsets[i, 2] + z;

            //RightNeighbor
            if (xBlock > World.CHUNK_SIZE - 1)
            {
                if (rightNeighbor == null || rightNeighbor.voxelMap[to1D(0, y, z)] == 0)
                    chunkMesh.addFace(i, x, y, z, id, ref meshData);
                continue;
            }

            //LeftNeighbor
            if (xBlock < 0)
            {
                if (leftNeighbor == null || leftNeighbor.voxelMap[to1D(chunkEnd, y, z)] == 0)
                    chunkMesh.addFace(i, x, y, z, id, ref meshData);
                continue;
            }

            //FrontNeighbor
            if (zBlock > World.CHUNK_SIZE - 1)
            {
                if (frontNeighbor == null || frontNeighbor.voxelMap[to1D(x, y, 0)] == 0)
                    chunkMesh.addFace(i, x, y, z, id, ref meshData);
                continue;
            }

            //BackNeighbor
            if (zBlock < 0)
            {
                if (backNeighbor == null || backNeighbor.voxelMap[to1D(x, y, chunkEnd)] == 0)
                    chunkMesh.addFace(i, x, y, z, id, ref meshData);
                continue;
            }

            if (yBlock > World.CHUNK_HEIGHT - 1)
            {
                continue;
            }

            if (xBlock < 0 || yBlock < 0 || zBlock < 0)
            {
                continue;
            }

            if (yBlock < 0)
            {
                chunkMesh.addFace(i, x, y, z, id, ref meshData);
                continue;
            }

            if (voxelMap[to1D(xBlock, yBlock, zBlock)] == 0)
            {
                chunkMesh.addFace(i, x, y, z, id, ref meshData);
                continue;
            }


        }
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