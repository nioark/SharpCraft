
class ChunkMesh{

    public void addFaceNo(int faceIndex, int x, int y, int z, ushort block_id, ref List<uint> meshData){

        for (int face= 0; face < 2; face++){
            for (int vertice = 0; vertice < 3; vertice++){

                //get triangle for current face
                int triangle = VoxelData.faces[faceIndex, face];

                //get vertice index for current triangle
                int verticeIndex = VoxelData.triangles[triangle,vertice];

                byte[] data = new byte[3];



                data[0] = (byte)(VoxelData.vertices[verticeIndex,0] + (byte)x);
                data[1] = (byte)(VoxelData.vertices[verticeIndex,1] + (byte)y);
                data[2] = (byte)(VoxelData.vertices[verticeIndex,2] + (byte)z);


                meshData.Add(System.BitConverter.ToUInt32(data));

                /*for (int uvsValues = 0; uvsValues < 2; uvsValues++){
                    //UV < Triangle <- Face
                    //Console.WriteLine(block_id + " " + faceIndex + " " + face + " " + vertice + " " + uvsValues);
                    //Console.WriteLine(Blocks.blocks[block_id].TexturesUVs[0]);
                    //Console.WriteLine(block_id);
                    meshData.Add((byte)(Blocks.blocks[block_id].TexturesUVs[faceIndex][face, vertice, uvsValues]));
                    //meshData.Add(VoxelData.uvs[faceIndex,face, vertice, uvsValues]);
                }*/

                //meshData.Add(VoxelData.facesLight[faceIndex]);
            }
        }

    }

    public void addFace(int faceIndex, int x, int y, int z, ushort block_id, ref List<float> meshData){

        // Get the texture layer for this face
        ushort textureLayer = Blocks.blocks[block_id].GetTextureLayer((Faces)faceIndex);

        for (int face= 0; face < 2; face++){
            for (int vertice = 0; vertice < 3; vertice++){

                //get triangle for current face
                int triangle = VoxelData.faces[faceIndex, face];

                //get vertice index for current triangle
                int verticeIndex = VoxelData.triangles[triangle,vertice];

                // Position (3 floats)
                meshData.Add(VoxelData.vertices[verticeIndex,0] + (float)x);
                meshData.Add(VoxelData.vertices[verticeIndex,1] + (float)y);
                meshData.Add(VoxelData.vertices[verticeIndex,2] + (float)z);

                // UV - per-face normalized [0,1] (2 floats)
                meshData.Add(VoxelData.faceUVs[faceIndex, face, vertice, 0]);
                meshData.Add(VoxelData.faceUVs[faceIndex, face, vertice, 1]);

                // Texture layer index (1 float)
                meshData.Add((float)textureLayer);

                // Light (1 float)
                meshData.Add(VoxelData.facesLight[faceIndex]);
            }
        }

    }
}
