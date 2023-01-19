using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos.Example;
using UnityEngine.UI;
using TMPro;
public class TimeRound : ExampleBaseBehaviour
{
    public float roundTime = 5f; // The amount of time between each spawn.
    public float roundDelay = 3f; // The amount of time before spawning starts.
    private Button buttonStartRecord;
    private Button buttonEndRecord;
    private Button buttonStartReplay;
    private Button buttonEndReplay;
    private bool isRecording;
    private TextMeshProUGUI roundTimeText;
    private float roundTimer;

    // Start is called before the first frame update
    void Start()
    {
        GhostSystem_UIManager gui = transform.GetComponent<GhostSystem_UIManager>();
        buttonStartRecord = gui.ButtonStartRecord.GetComponent<Button>();
        buttonEndRecord = gui.ButtonEndRecord.GetComponent<Button>();
        buttonStartReplay = gui.ButtonStartReplay.GetComponent<Button>();
        buttonEndReplay = gui.ButtonEndReplay.GetComponent<Button>();
        roundTimeText = gui.RoundTimeText;
        isRecording = false;
        roundTimer = 0;
        StartCoroutine(Round());
        StartCoroutine(RoundTime());
    }

    IEnumerator Round ()
    {
        yield return time.WaitForSeconds(roundDelay); // Wait for the delay
        while (true) // Repeat infinitely
        {
            
            // 新回合
            if(isRecording == false)
            {
                
               // Debug.Log("结束回放"+time.time);
                buttonEndReplay.onClick.Invoke();
               // Debug.Log("开始录制"+time.time);
                buttonStartRecord.onClick.Invoke();
                isRecording = true;
                // Wait for the interval
                yield return time.WaitForSeconds(roundTime);
            }
            if(isRecording == true)
            {
                roundTimer = 0;
               // Debug.Log("停止录制"+time.time);
                buttonEndRecord.onClick.Invoke();
               // Debug.Log("开始回放"+time.time);
                buttonStartReplay.onClick.Invoke();
                isRecording = false;
                // Wait for the interval
                yield return time.WaitForSeconds(roundTime);
            }
            
        }
    }
    IEnumerator RoundTime()
    {
        yield return time.WaitForSeconds(1);
        while (true)
        {
            if(isRecording == true)
            {
                roundTimer += 1;
                roundTimeText.text = roundTimer.ToString();
            }
            yield return time.WaitForSeconds(1);
        }
    }
}
