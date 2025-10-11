using OpenTK.Mathematics;

class LandScape{

    #region GENERATION_SETTINGS
    static double lacunarity = 5.0; //Control increase in frequency of octaves
    static double persistance = 4.2; //Control decrease in amplitude of octaves
    //NOT SETTED YET
    int octaves = 4;

    int landspace_height = 120;

    int landscape_scale = 1;
    #endregion

    #region Continentalness

    float[] Xs = new float[5]{-1.0f, -0.2f, 0.3f, 0.4f, 1.0f};
    float[] Ys = new float[5]{60f, 65, 74f, 80f, 105f};

    //float[] Xs = new float[4]{-1.0f, 0.3f, 0.4f, 1.0f};
    //float[] Ys = new float[4]{50f, 10f, 150f, 150f};

    //float[] Xs = new float[2]{-1.0f, 1.0f};
    //float[] Ys = new float[2]{50.0f, 150.0f};

    
    float[] XsD = new float[6]{-1f, -0.7f, -0.5f, 0.0f, 0.3f, 1f};
    float[] YsD = new float[6]{0.0080f, 0.0080f, 0.0080f, 0.0065f, 0.300f, 0.450f};
    #endregion

    FastNoise Noise;
    public LandScape(FastNoise noise){
        Noise = noise;
    }
   

    float noise_avg = 0;
    float max_noise = -1000f;
    float min_noise = 1000f;
    int getTerrainHeight(int x, int z){
        float final_noise = 0;
        for (int octave = 1; octave <= octaves; octave++){
            float frequency = (float)Math.Pow(1.2, (double)octave);
            float amplitude = (float)Math.Pow(1.2, (double)octave);
            final_noise += World.fastNoise.GetPerlin(x * frequency,z * frequency) * amplitude;
        }
        final_noise = final_noise / octaves;

        float erosion = GenUtil.Lerp(-1f, 1f, 0, -1f, World.fastNoise.GetPerlin(x * 0.25f,z * 0.25f));

        noise_avg += final_noise;

        if (final_noise < min_noise)
            min_noise = final_noise;

         if (final_noise > max_noise)
            max_noise = final_noise;

        //Console.WriteLine(final_noise);
        int height = (int)GenUtil.Lerp(Xs, Ys, (final_noise + erosion));
        //int height = (int)((final_noise * landscape_scale) + getContinentalness(x,z));

        return height;
    }

    public void genLandScape(ref byte[] voxelMap, ref byte[,] heightMap, int xChunk, int yChunk){
        noise_avg = 0.0f;
        max_noise = -1000f;
        min_noise = 1000f;

        for (int x = 0; x < World.CHUNK_SIZE; x++)
            for (int z = 0; z < World.CHUNK_SIZE; z++){
                
                int height = getTerrainHeight(x + (xChunk * World.CHUNK_SIZE),z + (yChunk * World.CHUNK_SIZE));
                heightMap[x,z] = (byte)height;

                //voxelMap[GenUtil.to1D(x,height,z)] = 1;
                //voxelMap[GenUtil.to1D(x,height - 1,z)] = 2;
                //voxelMap[GenUtil.to1D(x,height - 2,z)] = 2;
                //voxelMap[GenUtil.to1D(x,height - 3,z)] = 6;

                for (int y = 0; y <= height; y++){
                    voxelMap[GenUtil.to1D(x,y,z)] = 6;
                } //Fills out of stone

                
            }
        //Console.WriteLine("Max_H: " + max_noise + " Min_H: " +  min_noise + " Avg_H: " + noise_avg / (float)(World.CHUNK_SIZE * World.CHUNK_HEIGHT));
    }

    
    public void gen3dNoise(ref byte[] voxelMap, ref byte[,] heightMap, int xChunk, int yChunk){
         for (int x = 0; x < World.CHUNK_SIZE; x++)
            for (int z = 0; z < World.CHUNK_SIZE; z++){

                int level = 60;
                for (int y = level; y < World.CHUNK_HEIGHT; y++){
                
                    //float n = GenUtil.Lerp(XsD, YsD, y);
                    float f = 3f;
                    float amp = 1.3f;

                    //float squashing = 0.500f;
                    int xWorld = x + (xChunk * World.CHUNK_SIZE);
                    int zWorld = z + (yChunk * World.CHUNK_SIZE);

                    float squashing = GenUtil.Lerp(-1f, 1f, 0.0003f, 0.240f, World.fastNoise.GetPerlin(xWorld * 0.15f,zWorld * 0.15f) * 1.5f);

                    float density = World.fastNoise.GetPerlin(xWorld * f,y * f, zWorld * f) + (y - level) * squashing;

                    if (density < 0.80f){
                        if (heightMap[x,z] < (byte)y)
                            heightMap[x,z] = (byte)y;
                            
                        voxelMap[GenUtil.to1D(x,y,z)] = 6;
                        //voxelMap[GenUtil.to1D(x,y,z)] = 1;
                        //voxelMap[GenUtil.to1D(x,y - 1,z)] = 2;
                        //voxelMap[GenUtil.to1D(x,y - 2,z)] = 2;
                        //voxelMap[GenUtil.to1D(x,y - 3,z)] = 6;
                    }
                        
                    
                }
            }
            
    }



}