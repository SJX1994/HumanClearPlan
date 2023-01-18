using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReSetGhost : MonoBehaviour
{
    public Transform targetBody;
    public void ResetGhost()
    {
       // Debug.Log("reset-----------------"+transform.name);
        transform.position = targetBody.position;
        transform.rotation = targetBody.rotation;
    }
}
