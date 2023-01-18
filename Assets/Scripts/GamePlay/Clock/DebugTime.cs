using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;
using TMPro;
public class DebugTime : MonoBehaviour
{

    private Timeline time;
    private TextMeshProUGUI TMPtext;

    public bool isPaused = false;
    private Clock uiClock;
    private int round;
    private int timeTemp;
    [HideInInspector] public bool recored,setStyle;
    private string logText;
    void Start()
    {
        round = 0;
        timeTemp = 0;
        logText = "";
        time = transform.GetComponent<Timeline>();
        TMPtext = transform.GetComponent<TextMeshProUGUI>();
        uiClock = Timekeeper.instance.Clock("UI_Display");
        recored = false;
        setStyle = false;
    }
    void LateUpdate()
    {
        
        if(uiClock.paused == true)
        {
            if(recored==false)
            {
                TMPtext.fontSize = 10f; 
                logText +=  $" (round:{round}time:{Mathf.Round(time.time)-timeTemp}) \n";
                TMPtext.text = logText;
                timeTemp += Mathf.RoundToInt(time.time);
                round ++ ;
                recored = true;
                setStyle = false;
            }
            
        }else
        {
            if(setStyle==false)
            {
                TMPtext.fontSize = 36.5f;  
                setStyle = true;
            }
            TMPtext.text =  (Mathf.Round(time.time )-timeTemp).ToString();
        }
        
    }
  
   
  
   
}

