using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    public static float Value { get; private set; }
    public static string StringValue { get {
            int total = (int)Value;
            int sec = total % 60;
            int min = total / 60;
            return (min < 10 ? "0" : "") + min + ":" + (sec < 10 ? "0" : "") + sec;
        } }
    private Text time;

    void Start()
    {
        Value = 0;
        time = GetComponent<Text>();
    }

    
    void Update()
    {
        if(Menu.IsActive == false)
        {
            Value += Time.deltaTime;
            // отобразить value в формате
            time.text = StringValue;
        }
    }
}
