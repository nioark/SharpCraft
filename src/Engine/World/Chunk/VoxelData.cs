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

    // Per-face normalized UVs [0,1] - different orientation per face
    public static readonly float[,,,] faceUVs = new float[,,,]
    {
        // TOP (faceIndex 0) - triangles 8,9
        {
            { {0,0}, {0,1}, {1,1} },  // Triangle 1
            { {0,0}, {1,1}, {1,0} }   // Triangle 2
        },

        // BOTTOM (faceIndex 1) - triangles 10,11
        {
            { {0,0}, {1,1}, {0,1} },  // Triangle 1 - flipped X
            { {0,0}, {1,0}, {1,1} }   // Triangle 2 - flipped X
        },

        // FRONT (faceIndex 2) - triangles 0,1
        {
            { {0,0}, {0,1}, {1,1} },
            { {0,0}, {1,1}, {1,0} }
        },

        // BACK (faceIndex 3) - triangles 2,3
        {
            { {0,0}, {1,1}, {0,1} },  // flipped X
            { {0,0}, {1,0}, {1,1} }   // flipped X
        },

        // LEFT (faceIndex 4) - triangles 4,5
        {
            { {0,0}, {1,1}, {0,1} },  // flipped X
            { {0,0}, {1,0}, {1,1} }   // flipped X
        },

        // RIGHT (faceIndex 5) - triangles 6,7
        {
            { {0,0}, {0,1}, {1,1} },
            { {0,0}, {1,1}, {1,0} }
        }
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

    public static readonly int[,] faces = {
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
