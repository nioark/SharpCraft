using OpenTK.Mathematics;

class PlayerRender {

    public Vector3 playerPos {get; private set;} 
    private Vector3 velocity = new Vector3();
    private MeshRenderer playerRender;

    PlayerRender(Vector3 playerPos, GlShader shader){
        this.playerPos = playerPos;
        this.playerRender = new MeshRenderer(playerPos, shader);
    }

    public void Update(){
        
    }
    
}