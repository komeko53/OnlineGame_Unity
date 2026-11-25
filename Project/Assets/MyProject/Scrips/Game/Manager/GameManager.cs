using Fusion;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {  get; private set; }

    public struct PlayerRankData
    {
        public int rank;
        public string color;
    }

    private List<PlayerRankData> m_RankDatas = new List<PlayerRankData>();

    private float m_Timer = 30.0f;
    private bool m_IsGameStart = false;
    private bool m_IsGameEnd = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        string sceneName = SceneManager.Instance.NowSceneName;
        if(sceneName == "PlayScene")
        {
            if (IsAllReady() && !m_IsGameStart)
            {
                m_IsGameStart = true;
                TimerManager.Instance.StartTimer(m_Timer);
            }

            // ゲーム終了条件にヒットしたら、リザルト画面へ遷移
            if(IsGameEnd() && !m_IsGameEnd)
            {
                m_IsGameEnd = true;

                GameEnd();
                SortRank();
                SceneManager.Instance.ChangeScene("ResultScene");
            }
        }
    }

    /// <summary>
    /// 全員が準備完了したか
    /// </summary>
    /// <returns>true : 準備完了, false : 準備未完了</returns>
    public bool IsAllReady()
    {
        NetworkRunner runner = SceneManager.Instance.GetRunner();
        int playerCount = 0;

        if (!runner || !PlayerManager.Instance) return false;

        foreach (PlayerRef playerRef in runner.ActivePlayers)
        {
            Player player = PlayerManager.Instance.GetPlayer(playerRef);

            // Playerが未定、または準備未完了
            if(player == null || !player.IsReady)
            {
                return false;
            }
            playerCount++;
        }
        return true;
    }

    public void AddRankData(Player player)
    {
        PlayerRankData data = new PlayerRankData();
        data.rank = GetRank();
        data.color = player.PlayerColor;

        m_RankDatas.Add(data);
    }

    private int GetRank()
    {
        // ゲームが終了している場合
        if(m_IsGameEnd)
        {
            return 1;
        }

        // データリストの中が空だったら、プレイヤーの最大人数を返す
        if(m_RankDatas.Count == 0)
        {
            return NetworkManager.Instance.GetPlayerMax();
        }
        // データの中にデータがある場合
        else
        {
            int minRank = NetworkManager.Instance.GetPlayerMax();
            // リストの中身から、一番小さいRankを検索する
            foreach(PlayerRankData data in m_RankDatas)
            {
                if(data.rank < minRank)
                {
                    minRank = data.rank;
                }
            }

            // 一番小さいランクから-1を引いた値を返す
            minRank = minRank - 1;
            return minRank;
        }
    }

    public void ResetGame()
    {
        m_IsGameStart = false;
        m_IsGameEnd = false;
    }

    private bool IsGameEnd()
    {
        if (!TimerManager.Instance) return false;

        // タイマーの秒数が0秒になったら
        if (TimerManager.Instance.IsTimerExpired())
        {
            return true;
        }
        // 生存者が1人になったら
        else if(IsLastPlayer())
        {
            return true;
        }

        return false;
    }

    // 最後の一人か
    private bool IsLastPlayer()
    {
        NetworkRunner runner = SceneManager.Instance.GetRunner();
        int activePlayer = 0;

        if (runner == null) return false;

        foreach (PlayerRef playerRef in runner.ActivePlayers)
        {
            Player player = PlayerManager.Instance.GetPlayer(playerRef);

            // Playerが未定、または準備未完了
            if (player == null || !player.IsDead)
            {
                activePlayer++;
            }
        }

        // 残ってるのが最後の一人だったら
        if(activePlayer == 1)
        {
            return true;
        }

        return true;
    }

    private void GameEnd()
    {
        // 生き残ったPlayerの順位を追加する
        NetworkRunner runner = SceneManager.Instance.GetRunner();
        if (runner == null) return;

        foreach (PlayerRef playerRef in runner.ActivePlayers)
        {
            Player player = PlayerManager.Instance.GetPlayer(playerRef);

            // Playerが未定、または準備未完了
            if (player == null || !player.IsDead)
            {
                AddRankData(player);
            }
        }
    }

    public void SortRank()
    {
        m_RankDatas.Sort();
    }

    public List<PlayerRankData> GetPlayerRnaks()
    {
        return m_RankDatas;
    }

}
