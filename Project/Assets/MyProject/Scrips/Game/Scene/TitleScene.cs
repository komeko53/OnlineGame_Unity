using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Fusion;

public class TitleScene : SceneBase
{
    [SerializeField]
    private GameObject m_StandbyScreen;

    [SerializeField]
    private GameObject m_RoomScreen;

    [SerializeField]
    private GameObject m_PlayerMaxScreen;

    [SerializeField]
    private List<TextMeshProUGUI> m_PlayerNameText = new List<TextMeshProUGUI>();

    [SerializeField]
    private TextMeshProUGUI m_PlayerCountText;

    [SerializeField]
    private TextMeshProUGUI m_RoomKeyText;

    [SerializeField]
    private TextMeshProUGUI m_PlayerMaxText;

    private bool m_IsPlayerMax; // 参加人数が最大数に達したか 
    private int m_PrevPlayerCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.Instance.PlayBGM("BGM_Title", 0.4f);
        SceneManager.Instance.NowSceneName = "TitleScene";

        m_IsPlayerMax = false;
        m_PrevPlayerCount = 0;
        SetPlayerMaxText();
    }

    // Update is called once per frame
    private void Update()
    {
        // Cancelが押され、画面が表示中なら閉じる
        if (InputManager.Instance != null &&
            InputManager.Instance.Cancel() &&
            m_StandbyScreen.activeSelf)
        {
            ChangeStandbyScreenActive();
            LogoutAsync();
            SoundManager.Instance.PlaySE("SE_Cancel");
        }

        // Cancelが押され、画面が表示中なら閉じる
        if (InputManager.Instance != null &&
            InputManager.Instance.Cancel() &&
            m_RoomScreen.activeSelf)
        {
            ChangeRoomScreenActive();
            LogoutAsync();
            SoundManager.Instance.PlaySE("SE_Cancel");
        }

        // Cancelが押され、画面が表示中なら閉じる
        if (InputManager.Instance != null &&
            InputManager.Instance.Cancel() &&
            m_PlayerMaxScreen.activeSelf)
        {
            ChangePlayerMaxScreenActive();
            SoundManager.Instance.PlaySE("SE_Cancel");
        }

        // マッチング中の処理
        UpdateMatchi();
    }

    private void UpdateMatchi()
    {
        if (m_StandbyScreen.activeSelf)
        {
            // テキストはマイフレーム更新
            SetRoomKeyText();
            SetPlayerCountText();
            SetPlayerNameText();

            // 参加人数が最大になったらPlaySceneへ
            if (m_IsPlayerMax)
            {
                SceneManager.Instance.ChangeScene("PlayScene");
            }
        }
    }

    public void ChangeStandbyScreenActive()
    {
        m_StandbyScreen.SetActive(!m_StandbyScreen.activeSelf);
    }

    public void ChangeRoomScreenActive()
    {
        m_RoomScreen.SetActive(!m_RoomScreen.activeSelf);
    }

    public void ChangePlayerMaxScreenActive()
    {
        m_PlayerMaxScreen.SetActive(!m_PlayerMaxScreen.activeSelf);
    }

    public void OnClick()
    {
        SoundManager.Instance.PlaySE("SE_Confirm");
    }

    public void GameEnd()
    {
        Application.Quit();
    }

    private void SetPlayerNameText()
    {
        NetworkRunner runner = SceneManager.Instance.GetRunner();
        if (!runner) return;

        // プレイヤーが準備完了だったら、そっちにチェンジ
        if (m_PrevPlayerCount == runner.SessionInfo.PlayerCount) return;

        for(int i = 0; i < runner.SessionInfo.PlayerCount; i++)
        {
            int playerIndex = i + 1;
            m_PlayerNameText[i].text = $"プレイヤー{playerIndex}が入室しました。";
        }

        m_PrevPlayerCount = runner.SessionInfo.PlayerCount;
    }

    private void SetRoomKeyText()
    {
        NetworkRunner runner = SceneManager.Instance.GetRunner();

        if (!runner ||
            m_RoomKeyText.text == runner.SessionInfo.Name) return;

        m_RoomKeyText.text = runner.SessionInfo.Name;
    }

    private void SetPlayerCountText()
    {
        NetworkRunner runner = SceneManager.Instance.GetRunner();

        if (!runner) return;

        m_PlayerCountText.text = 
            $"{runner.SessionInfo.PlayerCount} /" +
            $"{NetworkManager.Instance.GetPlayerMax()}";

        if (!NetworkManager.Instance) return;

        if(runner.SessionInfo.PlayerCount >= NetworkManager.Instance.GetPlayerMax() &&
           !m_IsPlayerMax)
        {
            m_IsPlayerMax = true;
        }
    }
    
    public void SetPlayerMaxText()
    {
        m_PlayerMaxText.text =
            $"現在の参加人数：" +
            $"{NetworkManager.Instance.GetPlayerMax()}" +
            $"人";
    }

    private async void LogoutAsync()
    {
        if(NetworkManager.Instance != null)
        {
            await NetworkManager.Instance.Logout();
        }
    }
}
