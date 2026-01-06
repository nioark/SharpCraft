using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
class MeshRenderer{

    int meshDataCount = 0;
    int meshDataTris = 0;

    private int vbo, vao;

    Matrix4 modelMatrix;

    GlShader shader;
    

    public MeshRenderer(Vector3 position, GlShader shader){
                //Get shader make for this class in specific
        this.shader = shader;
        
        modelMatrix = Matrix4.CreateTranslation(position);
    }
    public unsafe void UploadMeshGPU(ref List<float> meshData){
        
        GL.CreateVertexArrays(1, out this.vao);

        GL.BindVertexArray(vao);
        

        this.meshDataCount = meshData.Count();
        // Each vertex: v,v,v, uv,uv, l = 6 floats
        this.meshDataTris = meshDataCount / 6;


        GL.CreateBuffers(1, out vbo);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData<float>(BufferTarget.ArrayBuffer, sizeof(float) * meshDataCount, meshData.ToArray(), BufferUsageHint.StaticDraw);

        
        // v, v, v, uv, uv, l

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 6 * sizeof(float), 5 * sizeof(float));
        GL.EnableVertexAttribArray(2);
    }

    //todo change to drawDynamic and add drawStatic removing attribut set at static 
    public void draw(){
        shader.Use();
        GL.BindVertexArray(vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

        shader.SetMatrix4("model", modelMatrix);

        GL.BindTexture(TextureTarget.Texture2D, ChunkTexture.id);
    
         //TODO meshDataCount, wrong count peformance loss
        //GL.Color3(155,100,5);
        GL.DrawArrays(PrimitiveType.Triangles, 0, meshDataTris);
    }

    public void clearBuffers(){

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        unsafe{
            GL.BufferData(BufferTarget.ArrayBuffer, meshDataTris, (IntPtr)null, BufferUsageHint.StaticDraw);
        }
        

        GL.DeleteBuffer(vbo);
        GL.DeleteVertexArray(vao);

        meshDataTris = 0;
    }
}