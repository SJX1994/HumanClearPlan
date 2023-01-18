
using UnityEngine;
// https://github.com/aniruddhahar/URP-AnimatedGrass
[ExecuteAlways]
public class playerpos : MonoBehaviour
{
    public string playerRef;
    void FixedUpdate()
    {

        Shader.SetGlobalVector(playerRef, transform.position);
        
    }
}
