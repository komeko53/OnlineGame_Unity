using Fusion;
using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayScene : SceneBase
{
    [SerializeField]
    private TextMeshProUGUI m_PlayerColorText;

    [SerializeField]
    private List<Image> m_ReadyImages = new List<Image>();
    private List<TextMeshProUGUI> m_ReadyTexts = new List<TextMeshProUGUI>();

    // 前回の準備完了人数
    private int m_PrevPlayerCount = 0;

    [SerializeField]
    private float m_IntervalStart;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.Instance.PlayBGM("BGM_Play", 0.4f);

        foreach (Image image in m_ReadyImages)
        {
            TextMeshProUGUI text = image.GetComponentInChildren<TextMeshProUGUI>();
            m_ReadyTexts.Add(text);
        }
    }

    private void Update()
    {
        UpdateRadyImage();
    }

    public void SetPlayreColorText(string playerColor)
    {
        m_PlayerColorText.text = $"あなたの色は \n{playerColor} です";
    }

    private void UpdateRadyImage()
    {
        if (m_IntervalStart <= 0.0f) return;

        // 全員準備完了だったら、画像を非表示
        if(GameManager.Instance.IsAllReady())
        {
            m_IntervalStart -= Time.deltaTime;

            if (m_IntervalStart <= 0.0f)
            {
                foreach (Image image in m_ReadyImages)
                {
                    image.gameObject.SetActive(false);
                }
            }
        }

        NetworkRunner runner = SceneManager.Instance.GetRunner();
        if (!runner || !PlayerManager.Instance) return;

        // 準備完了プレイヤーを取得
        List<Player> readyPlayers = new List<Player>();

        foreach (PlayerRef playerRef in runner.ActivePlayers)
        {
            Player player = PlayerManager.Instance.GetPlayer(playerRef);

            if (player != null && player.IsReady)
            {
                readyPlayers.Add(player);
            }
        }

        // 前回と人数が同じなら更新しない
        if (m_PrevPlayerCount == readyPlayers.Count) return;

        m_PrevPlayerCount = readyPlayers.Count;

        // UI更新
        for (int i = 0; i < m_ReadyImages.Count; i++)
        {
            bool isActive = i < readyPlayers.Count;

            m_ReadyImages[i].gameObject.SetActive(isActive);

            if (isActive)
            {
                Player player = readyPlayers[i];
                m_ReadyTexts[i].text = $"{player.PlayerColor} の\n準備完了！";
            }
        }
    }
}
