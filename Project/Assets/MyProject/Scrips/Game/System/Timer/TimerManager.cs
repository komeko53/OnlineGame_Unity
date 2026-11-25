using Fusion;
using UnityEngine;

public class TimerManager : NetworkBehaviour
{
    [Networked]
    public TickTimer GameTimer { get; set; }

    public static TimerManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ゲーム開始時にサーバーから呼ぶ
    public void StartTimer(float seconds)
    {
        if (!HasStateAuthority) return;
        if (Runner == null) return;

        GameTimer = TickTimer.CreateFromSeconds(
            Runner,
            seconds
            );
    }

    // 残り時間を取得
    public float GetRemaingTime()
    {
        if (Runner == null) return 0f;

        return GameTimer.RemainingTime(Runner) ?? 0f;
    }

    // タイマー終了判定
    public bool IsTimerExpired()
    {
        if (Runner == null) return false;

        return GameTimer.Expired(Runner);
    }
}
