using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JellyData;
public class JellyChecker : MonoBehaviour
{
    private RaycastHit hit;
    private Ray ray;
    public Transform checker;
    public float rayDistance = 1.5f;
    public float hitForce = 1f;
    public enum RaycastType {up, down, forward, back};
    public LayerMask layerMask = 1;
    public RaycastType raycastType;
    public Shader shader;
    Vector3 dir;
    LineRenderer line = null;
    JellyDataSet data;
      int probe = 0;
      Vector4[] pAfs = new Vector4[JellyDataSet.CountMax];
      Vector4[] dAts = new Vector4[JellyDataSet.CountMax];
    void Start()
    {
        if(checker==null)
        {
            checker = transform;
            data = new JellyDataSet();
            pAfs = new Vector4[JellyDataSet.CountMax];
            dAts = new Vector4[JellyDataSet.CountMax];
            probe = 0;
        }

        // Physics
            switch (raycastType)
            {
                case RaycastType.up:
                    
                    break;
                case RaycastType.down:
                    dir = - checker.transform.up;
                    // InvokeRepeating("HitEffect", 2.0f, 0.5f);
                    break;
                case RaycastType.forward:
                    dir = checker.transform.forward;
                    // visual hit
                    if(checker.GetComponent<LineRenderer>()!=null)
                    {
                        line = checker.GetComponent<LineRenderer>();
                        line.material = new Material(shader);
                    }
                    break;
                case RaycastType.back:
                    
                    break;
                default:
                    break;
            }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Physics
            switch (raycastType)
            {
                case RaycastType.up:
                    
                    break;
                case RaycastType.down:
                    dir = - checker.transform.up;
                    HitEffect();
                    break;
                case RaycastType.forward:
                    dir = checker.transform.forward;
                    HitEffect();
                    break;
                case RaycastType.back:
                    
                    break;
                default:
                    break;
            }
            
    }
    private void AddForce(Vector3 pos,Vector3 dir,float force)
    {
        Debug.Log($"Bingo:{transform.name}");
        Vector4 posAndForce = new Vector4(pos.x, pos.y, pos.z, force);
        Vector4 dirAndTime = new Vector4(dir.x, dir.y, dir.z, Time.time);
        EnQueue(posAndForce, dirAndTime);
        Transmit();
    }
    private void EnQueue(Vector4 a,Vector4 b)
    {
        pAfs[probe] = a;
        dAts[probe] = b;
        probe++;
        probe %= JellyDataSet.CountMax;
    }
    private void Transmit()
    {
        line.sharedMaterial.SetInt("_Count", JellyDataSet.CountMax);
        line.sharedMaterial.SetFloat("_Spring", JellyDataSet.Spring);
        line.sharedMaterial.SetFloat("_Damping", JellyDataSet.Damping);
        line.sharedMaterial.SetFloat("_Namida",JellyDataSet.Namida);
        line.sharedMaterial.SetVectorArray("_pAfs", pAfs);
        line.sharedMaterial.SetVectorArray("_dAts", dAts);
        

        switch(JellyDataSet.type)
        {
            case JellyDataSet.Type.Wave:
                line.sharedMaterial.EnableKeyword("_IsWave");
                line.sharedMaterial.DisableKeyword("_Explore");
                break;
            case JellyDataSet.Type.Explore:
                line.sharedMaterial.EnableKeyword("_Explore");
                line.sharedMaterial.DisableKeyword("_IsWave");
                break;
        }

    }
    public bool HitEffect()
    {
        ray = new Ray(checker.transform.position, dir);

        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);

        if(Physics.Raycast(ray, out hit, rayDistance) )
        {
            // Debug.Log(hit.collider.transform.gameObject.layer+"----------");
 
                if(hit.collider.GetComponent<JellyShaderModify>() != null )
                {
                    hit.collider.GetComponent<JellyShaderModify>().AddForce(hit.point,ray.direction.normalized, - hitForce);
                }
                if(line!=null) 
                {
                        this.AddForce(line.GetPosition(0),dir,hitForce);
                }
            
        }
        if(line!=null  )
        {
            line.SetPosition(0, checker.transform.position);
            line.SetPosition(1, checker.transform.position + dir * rayDistance);
        }
        return hit.collider;
    }
}
