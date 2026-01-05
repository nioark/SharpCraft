using System;

class TreeGenerator
{
    private World world;
    private Random random;

    // Block IDs
    private const byte WOOD_ID = 7;
    private const byte LEAVES_ID = 8;

    public enum TreeType
    {
        Small,
        Large
    }

    public TreeGenerator(World world, int seed)
    {
        this.world = world;
        this.random = new Random(seed);
    }

    /// <summary>
    /// Main entry point for tree generation
    /// </summary>
    public void GenerateTree(int xWorld, int yBase, int zWorld, TreeType type)
    {
        // Check height limit
        int maxHeight = (type == TreeType.Small) ? 10 : 20;
        if (yBase + maxHeight >= World.CHUNK_HEIGHT)
            return;

        switch (type)
        {
            case TreeType.Small:
                GenerateSmallTree(xWorld, yBase, zWorld);
                break;
            case TreeType.Large:
                GenerateLargeTree(xWorld, yBase, zWorld);
                break;
        }
    }

    /// <summary>
    /// Generates a small tree with a single trunk and rounded dome leaves
    /// </summary>
    private void GenerateSmallTree(int xWorld, int yBase, int zWorld)
    {
        // Randomize trunk height (4-6 blocks)
        int trunkHeight = random.Next(4, 7);

        // Generate trunk
        for (int y = 1; y <= trunkHeight; y++)
        {
            world.SetBlock(xWorld, yBase + y, zWorld, WOOD_ID);
        }

        // Generate rounded leaf dome on top
        int leafCenterY = yBase + trunkHeight;
        int leafRadius = 2;
        int leafHeight = 3;

        GenerateLeafDome(xWorld, leafCenterY, zWorld, leafRadius, leafHeight);
    }

    /// <summary>
    /// Generates a large tree with branches and multiple leaf clusters
    /// </summary>
    private void GenerateLargeTree(int xWorld, int yBase, int zWorld)
    {
        // Trunk height (8-12 blocks)
        int trunkHeight = random.Next(8, 13);

        // Height where branches start
        int branchStartY = random.Next(4, 7);

        // Generate main trunk
        for (int y = 1; y <= trunkHeight; y++)
        {
            world.SetBlock(xWorld, yBase + y, zWorld, WOOD_ID);
        }

        // Generate branches
        int numBranches = random.Next(2, 5);
        int branchY = branchStartY;

        for (int i = 0; i < numBranches; i++)
        {
            // Make sure we don't go too high
            if (branchY >= trunkHeight - 2)
                break;

            // Pick random direction
            int dir = random.Next(4);
            int dx = 0, dz = 0;
            switch (dir)
            {
                case 0: dx = 1; break;
                case 1: dx = -1; break;
                case 2: dz = 1; break;
                case 3: dz = -1; break;
            }

            // Branch length (2-4 blocks)
            int branchLength = random.Next(2, 5);
            GenerateBranch(xWorld, yBase + branchY, zWorld, dx, dz, branchLength);

            // Alternate branch heights
            branchY += 2;
        }

        // Generate main canopy at top (larger sphere)
        int canopyY = yBase + trunkHeight;
        GenerateLeafSphere(xWorld, canopyY, zWorld, 3);
    }

    /// <summary>
    /// Generates a horizontal branch with leaves at the end
    /// </summary>
    private void GenerateBranch(int xStart, int yStart, int zStart, int dx, int dz, int length)
    {
        int endX = xStart;
        int endZ = zStart;

        // Generate branch wood
        for (int i = 1; i <= length; i++)
        {
            endX = xStart + dx * i;
            endZ = zStart + dz * i;
            world.SetBlock(endX, yStart, endZ, WOOD_ID);
        }

        // Add leaf cluster at branch end
        GenerateLeafDome(endX, yStart, endZ, 2, 2);
    }

    /// <summary>
    /// Generates a rounded dome of leaves (flattened sphere)
    /// </summary>
    private void GenerateLeafDome(int xCenter, int yCenter, int zCenter, int radius, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    // Calculate distance from center (flatten Y for dome effect)
                    float dx = x;
                    float dy = y * 0.7f; // Flatten the dome
                    float dz = z;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);

                    // Adjust effective radius based on height layer
                    float effectiveRadius = radius;
                    if (y >= height - 1)
                        effectiveRadius = radius * 0.6f; // Top layer is smaller

                    if (distance <= effectiveRadius)
                    {
                        // Add some randomness for natural look
                        if (random.Next(100) > 15) // 85% fill rate
                        {
                            world.SetBlock(xCenter + x, yCenter + y, zCenter + z, LEAVES_ID);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generates a full sphere of leaves
    /// </summary>
    private void GenerateLeafSphere(int xCenter, int yCenter, int zCenter, int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    float distance = (float)Math.Sqrt(x * x + y * y + z * z);

                    if (distance <= radius)
                    {
                        // Add some randomness for natural look
                        if (random.Next(100) > 15)
                        {
                            world.SetBlock(xCenter + x, yCenter + y, zCenter + z, LEAVES_ID);
                        }
                    }
                }
            }
        }
    }
}
