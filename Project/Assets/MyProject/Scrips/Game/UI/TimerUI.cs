using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_TimerText;

    // Update is called once per frame
    void Update()
    {
        TimerManager timer = TimerManager.Instance;

        if (timer == null) return;
        if (!timer.Object || !timer.Runner) return;

        m_TimerText.text = $"{Mathf.CeilToInt(TimerManager.Instance.GetRemaingTime())}";
    }
}
