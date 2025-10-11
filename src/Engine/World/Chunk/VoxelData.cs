using System.Collections;
using System.Collections.Generic;

public static class VoxelData
{

    //    5--------6
    //   /        /|
    //  /        / |
    // 1--------2 |
    // |        |  |
    // |  4     |  7
    // |        | /
    // |        |/
    // 0--------3

    
    public static readonly float[,] vertices = new float[8,3]{
        {0, 0, 1},
        {0, 1, 1},
        {1, 1, 1},
        {1, 0, 1},

        {0, 0, 0},
        {0, 1, 0},
        {1, 1, 0},
        {1, 0, 0}
    };

    
    
    public static readonly float[,,,] uvs = new float[,,,]{
		//Face index, face triangle, vertice index, uv

		{{ {0,0}, {0,1}, {1,1}}, //Top
		 { {0,0}, {1,1}, {1,0}}},

		{{ {0,0}, {1,1}, {0,1}}, //Bottom
		 { {0,0}, {1,0}, {1,1}}},

		{{ {0,0}, {0,1}, {1,1}}, //Front
		 {{0,0}, {1,1}, {1,0}}},

		{{ {0,0}, {1,1}, {0,1}}, //Back
		 { {0,0}, {1,0}, {1,1}}},

		{{ {0,0}, {1,1}, {0,1}}, //Left
		 { {0,0}, {1,0}, {1,1}}},

		{{ {0,0}, {0,1}, {1,1}}, //Right
		 { {0,0}, {1,1}, {1,0}}},

	};

	public static readonly float[,,] textures = new float[,,]{
		//Block id, face

		//Air just for textures work
		{{0,0}, {0,0}, {0,0}, {0,0}, {0,0}, {0,0}},

		//Stone

		//Front, Back, Left, Right, Top, Bottom
		{{0,3}, {0,3}, {0,3}, {0,3}, {0,3}, {0,3}},

		//Dirt
		{{1,3}, {1,3}, {1,3}, {1,3}, {1,3}, {1,3}},

		//Grass
		{{2,3}, {2,3}, {2,3}, {2,3}, {3,2}, {1,3}}
	};

	public static readonly int[,] offsets = new int[,]{
		{ 0, 1, 0}, //Top
		{ 0,-1, 0}, //Bottom
		{ 0, 0, 1}, //Front
		{ 0, 0,-1}, //Back
		{-1, 0, 0}, //Left
		{ 1, 0, 0}, //Right
	};

	public static readonly int[,] triangles = new int[12,3]{


		{0,1,2}, //Front
		{0,2,3},

		{4,6,5}, //Back
		{4,7,6},

		{0,5,1}, //Left
		{0,4,5},

		{3,2,6}, //Right
		{3,6,7},

		{1,5,6}, //Top   Changed this and worked wtf?
		{1,6,2},

		{0,7,4}, //Bottom
		{0,3,7},
	};

	public static readonly int[,] faces = new int[,]{
		{8,9}, //Top
		{10,11},  //Bottom
		{0,1}, //Front
		{2,3}, //Back
		{4,5}, //Left
		{6,7}, //Right
	};

	public static readonly float[] facesLight = new float[]{
		1.0f, //Top
		0.4f,  //Bottom
		0.8f, //Front
		0.8f, //Back
		0.6f, //Left
		0.6f, //Right
	};
}