using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Chronos;
using System.Threading;
using SyrupPlayer;
using CleverCrow.Fluid.StatsSystem;
public class GhostSystem : MonoBehaviour
{
    private Thread thread;
    object sharedValueLock = new object();
    int sharedValue;
    private GhostRecorder[] recorders;
    private GhostActor[] ghostActors;

    private FollowPlayer[] cameraFollows;
    private CharacterExample[] characterExample;
    // public Transform playerControlled;
    // public Transform playerGhost;
    private Volume volume; 
    private VolumeProfile profile;
    private UnityEngine.Rendering.Universal.Vignette vignette;
    private UnityEngine.Rendering.Universal.Bloom bloom;
    private Clock musicClock;

   
    public float recordDuration = 5;
    private IEnumerator coroutine;

    private void Start()
    {
     
        musicClock = Timekeeper.instance.Clock("Music");
        volume = FindObjectOfType<Volume>();
        profile = volume.sharedProfile;
        profile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out var bloom);
        this.bloom = bloom;
        profile.TryGet<UnityEngine.Rendering.Universal.Vignette>(out var vignette);
        this.vignette = vignette;
        recorders = FindObjectsOfType<GhostRecorder>();
        ghostActors = FindObjectsOfType<GhostActor>();
        cameraFollows = FindObjectsOfType<FollowPlayer>();
        characterExample = FindObjectsOfType<CharacterExample>();
        CameraFollowGhost(true);
        PostProcessing(false);
      
    }
    void CameraFollowGhost(bool m_isRecording)
    {
        foreach (FollowPlayer cameraFollow in cameraFollows)
        {
            cameraFollow.isRecording = m_isRecording;
        }
    }
    void GhostBack(bool m_isRecording)
    {
        GameObject[] imgObjs = GameObject.FindGameObjectsWithTag("GhostObject");
        if(imgObjs.Length > 0)
        {
            foreach (GameObject imgObj in imgObjs)
            {
                imgObj.GetComponent<Collider>().enabled = m_isRecording;
                imgObj.GetComponent<Renderer>().enabled = m_isRecording;
                imgObj.GetComponent<Rigidbody>().isKinematic = !m_isRecording;
            }
            if(!m_isRecording)
            {
                foreach (GameObject imgObj in imgObjs)
                {
                    imgObj.GetComponent<ReSetGhost>().ResetGhost();
                    
                }
            }
            
        }else
        {
            Debug.LogError("no tag");
        }
        
    }
    void PostProcessing(bool m_isRecording)
    {
        if(m_isRecording)
        {
            bloom.intensity.value = 5f;
            vignette.intensity.value = 0.5f;
        }else
        {
            bloom.intensity.value = 1f;
            vignette.intensity.value = 0.1f;
        }
    }
    void ReflashHitEvent()
    {
        foreach(CharacterExample obj in characterExample)
        {
            obj.hitOnce = false;
        }
    }
    void ResetPlayer(bool m_isRecording)
    {
        GameObject[] playerObjs = GameObject.FindGameObjectsWithTag("Player");
        if(playerObjs.Length > 0)
        {
            foreach (GameObject playerObj in playerObjs)
            {
                if(playerObj.GetComponent<ReSetGhost>()!=null)
                {
                    playerObj.GetComponent<Player>().enabled = false;
                    playerObj.GetComponent<ReSetGhost>().ResetGhost();
                    if(m_isRecording)
                    {
                        coroutine = WaitAndPrint(0.1f,playerObj);
                        StartCoroutine(coroutine);
                    }
                    else
                    {
                        playerObj.GetComponent<Player>().enabled = false;
                    }
                    
                    
                }
                
            }
        }else
        {
            Debug.LogError("no tag");
        }
    }
    void SetMusic(bool m_isRecording)
    {
        musicClock.localTimeScale = m_isRecording ? 1.5f : 0.5f;
    }
    private IEnumerator WaitAndPrint(float waitTime,GameObject playerObj)
    {
        //while (true)
        //{
            yield return new WaitForSeconds(waitTime);
            playerObj.GetComponent<Player>().enabled = true;
        //}
    }
    private bool isRecording;
    private bool isReplaying;

    public void StartRecording()
    {
        for (int i = 0; i < recorders.Length; i++)
        {
            recorders[i].StartRecording(recordDuration);
        }

        OnRecordingStart();
        CameraFollowGhost(true);
        GhostBack(true);
        PostProcessing(true);
        ResetPlayer(true);
        ReflashHitEvent();
        SetMusic(false);
    }

    public void StopRecording()
    {
        for (int i = 0; i < recorders.Length; i++)
        {
            recorders[i].StopRecording();
        }

        OnRecordingEnd();
        CameraFollowGhost(true);
        GhostBack(false);
        PostProcessing(false);
        ResetPlayer(false);
        SetMusic(false);
    }

    public void StartReplay()
    {
        for (int i = 0; i < ghostActors.Length; i++)
        {
            ghostActors[i].StartReplay();
        }

        for (int i = 0; i < recorders.Length; i++)
        {
            recorders[i].GetComponent<Renderer>().enabled = false;
        }

        
        OnReplayStart();
        CameraFollowGhost(false);
        PostProcessing(false);
        GhostBack(false);
        ResetPlayer(false);
        SetMusic(true);
    }

    public void StopReplay()
    {
        for (int i = 0; i < ghostActors.Length; i++)
        {
            ghostActors[i].StopReplay();
        }

        for (int i = 0; i < recorders.Length; i++)
        {
            recorders[i].GetComponent<Renderer>().enabled = true;
        }

        // cameraFollow.followTarget = playerControlled;

        OnReplayEnd();
        CameraFollowGhost(false);
        PostProcessing(false);
        GhostBack(false);
        ResetPlayer(false);
        SetMusic(false);
        
    }

    #region Event Handlers
    public event EventHandler RecordingStarted;
    public event EventHandler RecordingEnded;
    public event EventHandler ReplayStarted;
    public event EventHandler ReplayEnded;
    #endregion

    #region Event Invokers
    protected virtual void OnRecordingStart()
    {
        if (RecordingStarted != null)
        {
            RecordingStarted.Invoke(this, EventArgs.Empty);
        }
    }

    protected virtual void OnRecordingEnd()
    {
        if (RecordingEnded != null)
        {
            RecordingEnded.Invoke(this, EventArgs.Empty);
        }
    }


    protected virtual void OnReplayStart()
    {
        if (ReplayStarted != null)
        {
            ReplayStarted.Invoke(this, EventArgs.Empty);
        }
    }

    protected virtual void OnReplayEnd()
    {
        if (ReplayEnded != null)
        {
            ReplayEnded.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion
}