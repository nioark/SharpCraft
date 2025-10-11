using System.Collections.Concurrent;
using OpenTK.Mathematics;

public class World
{
    #region WORLD_STATIC_SETTINGS
    public static int CHUNK_SIZE = 16;
    public static int CHUNK_HEIGHT = 256;

    #endregion

    public int world_seed { get; private set; }

    private ConcurrentDictionary<Vector2, Chunk> chunks = new ConcurrentDictionary<Vector2, Chunk>();

    private ConcurrentBag<Chunk> chunksAddMesh = new ConcurrentBag<Chunk>();
    private ConcurrentQueue<Chunk> chunksRemove = new ConcurrentQueue<Chunk>();

    public static Random random { get; private set; }
    public static FastNoise fastNoise { get; private set; }

    private Generation generation;
    private Camera player;

    private int chunkRenderDistance = 10;

    private GlShader shader;

    public World(int seed, Camera camera, GlShader shader)
    {
        player = camera;
        world_seed = seed;
        random = new Random(seed);
        fastNoise = new FastNoise(seed);
        this.shader = shader;

        generation = new Generation(fastNoise, this);
    }

    public static List<Vector2> Spiral(int X, int Y)
    {
        int x = 0, y = 0, dx = 0, dy = -1;
        int t = Math.Max(X, Y);
        int maxI = t * t;

        var order = new List<Vector2>();

        for (int i = 0; i < maxI; i++)
        {
            if ((-X / 2 <= x) && (x <= X / 2) && (-Y / 2 <= y) && (y <= Y / 2))
            {
                //DO STUFF
                order.Add(new Vector2(x, y));
            }

            if ((x == y) || ((x < 0) && (x == -y)) || ((x > 0) && (x == 1 - y)))
            {
                t = dx; dx = -dy; dy = t;
            }
            x += dx; y += dy;
        }
        return order;
    }

    public async void generateChunkSquare(){
        int size = 30;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++){
                Vector2 chunkPos = new Vector2(x,y);
                Chunk chunk = new Chunk(chunkPos, shader, ref generation);
                chunks.TryAdd(chunkPos, chunk);
                chunk.GenerateLandScape();
                chunk.GeneratePostScape();  
            }

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++){
                Chunk chunk = chunks[new Vector2(x,y)];

                chunk.frontNeighbor = y != size - 1 ? chunks[new Vector2(x,y + 1)] : null;
                chunk.backNeighbor  = y != 0  ? chunks[new Vector2(x,y - 1)] : null;
                chunk.leftNeighbor  = x != 0  ? chunks[new Vector2(x - 1,y)] : null;
                chunk.rightNeighbor = x != size - 1  ? chunks[new Vector2(x + 1,y)] : null;
                chunk.GenerateMesh(); 
                chunksAddMesh.Add(chunk);
            }

    }

    public async void Update()
    {
        bool keepRunning = true;

        while (keepRunning)
        {
            var orderGenerate = Spiral(chunkRenderDistance, chunkRenderDistance);

            var playerPos = new Vector2((int)((player.Position.X + 8) / CHUNK_SIZE), (int)((player.Position.Z + 8) / CHUNK_SIZE));

            #region CHUNK_TERRAIN_GENERATION
            foreach (var pos in orderGenerate)
            {
                var chunkPos = pos + playerPos;

                if (!chunks.ContainsKey(chunkPos))
                    chunks.TryAdd(chunkPos, new Chunk(chunkPos, shader, ref generation));

                Chunk chunk = chunks[chunkPos];

                if (chunk.isGenerated == false)
                    chunk.GenerateLandScape();
            }
            #endregion

            var orderAddNeighbors = Spiral(chunkRenderDistance - 1, chunkRenderDistance - 1);

            #region ADD_CHUNK_NEIGHBORS
            foreach (var pos in orderAddNeighbors)
            {

                var chunkPos = pos + playerPos;
                Chunk chunk = chunks[chunkPos];


                if (chunk.hasNeighbors == false)
                {
                    chunk.frontNeighbor = chunks[chunkPos + new Vector2(0, 1)];
                    chunk.backNeighbor = chunks[chunkPos + new Vector2(0, -1)];
                    chunk.leftNeighbor = chunks[chunkPos + new Vector2(-1, 0)];
                    chunk.rightNeighbor = chunks[chunkPos + new Vector2(1, 0)];

                    chunk.hasNeighbors = true;
                }

            }
            #endregion

            var orderPost = Spiral(chunkRenderDistance - 1, chunkRenderDistance - 1);

            #region CHUNK_POST_GENERATION
            foreach (var pos in orderPost)
            {
                var chunkPos = pos + playerPos;

                // if (!chunks.ContainsKey(chunkPos))
                //     chunks.TryAdd(chunkPos, new Chunk(chunkPos, shader, ref generation));


                Chunk chunk = chunks[chunkPos];

                if (chunk.isPostGenerated == false)
                    chunk.GeneratePostScape();
            }
            #endregion

            var orderGenerateMesh = Spiral(chunkRenderDistance - 2, chunkRenderDistance - 2);



            #region GENERATE_CHUNK_MESH
            foreach (var pos in orderGenerateMesh)
            {
                //I chunk doesnt exist
                var chunkPos = pos + playerPos;
                Chunk chunk = chunks[chunkPos];

                if (chunk.hasMesh == false)
                {
                    chunk.GenerateMesh();
                    chunksAddMesh.Add(chunk);
                }
            }
            #endregion

            #region CHUNK_DELETE_OUTRANGE
            var orderDelete = Spiral(chunkRenderDistance + 4, chunkRenderDistance + 4);
            int range = (chunkRenderDistance / 2);

            float distanceThr = ((float)chunkRenderDistance / 1.4f) * CHUNK_SIZE;

            foreach (KeyValuePair<Vector2, Chunk> entry in chunks)
            {
                Vector2 pos = entry.Value.chunkPos * CHUNK_SIZE;
                var distance = Math.Sqrt((Math.Pow(pos.Y - playerPos.Y * CHUNK_SIZE, 2) + Math.Pow(pos.X - playerPos.X * CHUNK_SIZE, 2)));


                if (distance > distanceThr)
                {
                    if (!chunksRemove.Contains(entry.Value))
                    {
                        chunksRemove.Enqueue(entry.Value);
                    }

                }
            }

            #endregion


        }

    }

    public void UpdateChunkBuffer()
    {
        Chunk chunk;

        bool needAdd = chunksAddMesh.TryTake(out chunk);
        if (needAdd)
            chunk.MeshUpload();


        Chunk chunk2;

        bool needRemove = chunksRemove.TryDequeue(out chunk2);

        if (needRemove)
        {
            Chunk? val;
            chunks.Remove(chunk2.chunkPos, out val);
            if (val != null)
            {
                val.readyToRender = false;
                val.ClearGPUMesh();
            }
        }



    }

    public void StartUpdateThread()
    {
        Task t = new Task(Update);
        t.Start();
    }

    public void SetBlock(int xWorld, int yWorld, int zWorld, byte id)
    {
        int xChunk = (int)(xWorld / World.CHUNK_SIZE);
        int zChunk = (int)(zWorld / World.CHUNK_SIZE);

        Chunk ch;
        if (chunks.TryGetValue(new Vector2(xChunk, zChunk), out ch))
        {
            int x = Math.Abs(xWorld % World.CHUNK_SIZE);
            int z = Math.Abs(zWorld % World.CHUNK_SIZE);

            ch.setVoxelMapBlock(x, yWorld, z, id);
        }
        else
        {
            Console.WriteLine("Couldnt set block in " + xChunk + " " + zChunk + " || " + xWorld + " " + yWorld + " " + zWorld);
        }


    }

    public Block GetBlock(int xWorld, int yWorld, int zWorld)
    {
        int xChunk = (int)Math.Ceiling((float)xWorld / (float)World.CHUNK_SIZE);
        int zChunk = (int)Math.Ceiling((float)zWorld / (float)World.CHUNK_SIZE);
        //Console.WriteLine("Couldnt set block in " + ((xChunk * World.CHUNK_SIZE) - xWorld) + " " + yWorld + " " + ((zChunk * World.CHUNK_SIZE) - zWorld));
        Chunk ch;
        Block block;

        if (chunks.TryGetValue(new Vector2(xChunk, zChunk), out ch))
        {

            byte id = ch.getVoxelMapBlock((xChunk * World.CHUNK_SIZE) - xWorld, yWorld, (zChunk * World.CHUNK_SIZE) - zWorld);

            Blocks.blocks.TryGetValue(id, out block);
            return block;
        }

        Console.WriteLine("Couldnt get block in " + xChunk + " " + zChunk);

        Blocks.blocks.TryGetValue(0, out block);
        return block;



    }

    public void Render()
    {
        foreach (Chunk chunk in chunks.Values)
        {
            if (chunk.readyToRender)
                chunk.Render();
        }
    }
}