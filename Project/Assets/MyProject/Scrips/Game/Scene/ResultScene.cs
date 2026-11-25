using TMPro;
using UnityEngine;
using System.Collections.Generic;
using static NetworkManager;
using static GameManager;


public class ResultScene : SceneBase
{
    [SerializeField]
    private TextMeshProUGUI m_PlayerMaxText;

    [SerializeField]
    private List<TextMeshProUGUI> m_RankTexts;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.Instance.PlayBGM("BGM_Result");
        SetPlayerMaxText();



    }

    public void OnTitle()
    {
        LogoutAsync();
        SceneManager.Instance.ChangeScene("TitleScene");
    }

    public void OnRandomBattle()
    {

    }

    public void OnRestartBattle()
    {

    }

    public void SetBattle()
    {
        // 再戦処理

        // 部屋のタイプによって、ボタンのグレースケール


        // 部屋名がPracticeじゃなければ、さっき使ってたルームキーで再戦


        // 部屋名がPracticeだったら、再度ランダムマッチ

        EMatchType type = NetworkManager.Instance.GetMatchType();

        if (EMatchType.RANDOM == type)
        {

        }
        else if(EMatchType.PRIVATE == type)
        {
            
        }
    }

    public void SetPlayerMaxText()
    {
        m_PlayerMaxText.text =
            $"現在の参加人数：" +
            $"{NetworkManager.Instance.GetPlayerMax()}" +
            $"人";
    }

    private void SetPlayerRankText()
    {
        List<PlayerRankData> rankDatas = GameManager.Instance.GetPlayerRnaks();

        for(int i = 0; i < rankDatas.Count; i++)
        {
            m_RankTexts[i].text = $"{rankDatas[i].rank}位： {rankDatas[i].color}";
            m_RankTexts[i].gameObject.SetActive(true);
        }
    }

    private async void LogoutAsync()
    {
        if (NetworkManager.Instance != null)
        {
            await NetworkManager.Instance.Logout();
        }
    }

}
