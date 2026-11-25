using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    // 入力データ
    public struct PlayerInputData : INetworkInput
    {
        public float horizontal;
        public float vertical;

        public bool isReady;
        public bool isConfirm;
        public bool isCancel;
        public bool isfire;
    }

    // 部屋の名前候補
    private List<string> m_RoomNames = new List<string>
    {
        "Practice01",
        "Practice02",
        "Practice03",
        "Practice04",
        "Practice05",
        "Practice06",
        "Practice07",
        "Practice08",
        "Practice09",
        "Practice10",
    };

    private const int PLAYER_MAX = 4;

    // マッチングタイプ
    public enum EMatchType
    {
        RANDOM,
        PRIVATE,
    }

    private EMatchType m_MatchType;
    private string m_RoomKey;

    // プレイヤーが操作するプレファブ
    [SerializeField]
    NetworkPrefabRef m_PlayerPrefab;

    // ネットワークシステムの根幹であるランナー
    private NetworkRunner m_Runner;

    bool m_IsReady = false;
    bool m_IsConfirm = false;
    bool m_IsCancel = false;
    bool m_IsFire = false;

    [SerializeField]
    private int m_PlayerMax;

    public static NetworkManager Instance { get; private set; }

    private bool m_IsStarting = false;
    private bool m_IsConnected = false;
    private NetworkSceneManagerDefault m_SceneManager;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public async Task Logout()
    {
        NetworkRunner runner = m_Runner;

        if (runner == null) return;

        // 二重で終了処理を行わないよう参照を先に外す
        m_Runner = null;
        m_IsConnected = false;

        await runner.Shutdown();
        Destroy(runner);

        if(m_SceneManager != null)
        {
            Destroy(m_SceneManager);
            m_SceneManager = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        string sceneName = SceneManager.Instance.NowSceneName;

        // 入力処理
        if (InputManager.Instance.OnReady)
        {
            m_IsReady = true;
        }

        if (InputManager.Instance.OnConfirm)
        {
            m_IsConfirm = true;
        }
        if(InputManager.Instance.OnCancel)
        {
            m_IsCancel = true;
        }
        if(InputManager.Instance.OnFire)
        {
           m_IsFire = true;
        }
    }

    public async void StartRandomMatchi()
    {
        m_MatchType = EMatchType.RANDOM;

        // ランナーの設定
        CreateRunner();
        // ClientServerロビーへの参加を要求
        await m_Runner.JoinSessionLobby(SessionLobby.ClientServer);

        // 初期化
        SceneManager.Instance.Init(m_Runner);
    }

    public void StartPrivateMatchi()
    {
        m_MatchType = EMatchType.PRIVATE;
    }

    public async void StartHost(string roomKey)
    {
        // ゲーム設定して通信開始
        await StartNetwork(GameMode.Host, roomKey);
    }

    public async void StartClient(string roomKey)
    {
        // ゲーム設定して通信開始
        await StartNetwork(GameMode.Client, roomKey);
    }

    private async Task StartNetwork(GameMode mode, string roomKey)
    {
        // 接続中・接続済みなら二重実行しない
        if (m_IsStarting || m_IsConnected) return;

        m_IsStarting = true;

        try
        {
            m_SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

            var result = await m_Runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = roomKey,
                SceneManager = m_SceneManager,  // ネットワーク用のシーン
                PlayerCount = m_PlayerMax,      // 接続人数の最大数
            });

            Debug.Log($"Result:{result}");
            Debug.Log($"StartGame:{result.Ok}");
            Debug.Log($"Finish:{result.ShutdownReason}");
            Debug.Log($"Ex:{result.ErrorMessage}");

            if(result.Ok)
            {
                m_IsConnected = true;
            }
            else
            {
                await Logout();
            }
        }
        catch(Exception e)
        {
            Debug.LogException(e);
            await Logout();
        }
        finally
        {
            m_IsStarting = false;
        }
    }

    // 未使用の部屋を探す
    private string FindUnusedRoom(List<SessionInfo> sessionList)
    {
        foreach(string roomName in m_RoomNames)
        {
            // 同じ名前の部屋が存在するか確認
            bool isUsed = sessionList.Any(
                session => session.Name == roomName
                );

            if(!isUsed)
            {
                return roomName;
            }
        }

        // 未使用の部屋名がない
        return "";
    }

    public void SpawnPlayers()
    {
        // 参加した全てのPlayerを生成
        if (!m_Runner.IsServer || !PlayerManager.Instance) return;

        int listIndex = 0;

        // 現在接続している全Playerを取得
        foreach (PlayerRef player in m_Runner.ActivePlayers)
        {
            PlayerManager playerMg = PlayerManager.Instance;
            GameObject spawnObj = playerMg.GetSpawnPos(listIndex);

            // プレイヤー生成
            NetworkObject obj = m_Runner.Spawn(
                m_PlayerPrefab,
                spawnObj.transform.position,
                spawnObj.transform.rotation,
                player);

            // PlayerにIndexを設定
            Player playerObj = obj.GetComponent<Player>();
            playerObj.PlayerIndex = listIndex + 1;
            playerObj.ListIndex = listIndex;
            playerObj.PlayerColor = playerMg.GetPlayerColor(listIndex);

            playerObj.SetMaterial();

            // PlayerManagerの中にPlayerを記録
            PlayerManager.Instance.AddPlayer(playerObj);

            m_Runner.SetPlayerObject(player, obj);
            listIndex++;
        }
    }



    public int GetPlayerMax()
    {
        return m_PlayerMax;
    }

    public EMatchType GetMatchType()
    {
        return m_MatchType;
    }

    public void SetRoomKey(string key)
    {
        m_RoomKey = key;
    }

    public void SetPlayerMax(int playerMax)
    {
        m_PlayerMax = playerMax;
    }

    private void CreateRunner()
    {
        if (m_Runner != null) return;

        // ランナーの設定
        m_Runner = gameObject.AddComponent<NetworkRunner>();

        m_Runner.ProvideInput = true;
        m_Runner.AddCallbacks(this);
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    /// <summary>
    /// プレイヤーが参加してきたら呼ばれる
    /// </summary>
    /// <param name="runner">ランナー</param>
    /// <param name="player">参加してきたプレイヤー参照</param>
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"P;ayer Joined : {player}");

        if (!m_Runner.IsServer) return;
    }

    /// <summary>
    /// プレイヤーが通信からいなくなったら呼ばれる
    /// </summary>
    /// <param name="runner">ランナー</param>
    /// <param name="player">いなくなったプレイヤー参照</param>
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // オブジェクトを取得して退場
        if (runner.TryGetPlayerObject(player, out NetworkObject obj))
        {
            // PlayerManagerの中に存在するPlayerを削除
            PlayerManager.Instance.RemovePlayer(player);
            runner.Despawn(obj);
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"OnShutdown: {shutdownReason}");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.LogError($"OnDisconnectedFromServer: {reason}");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"OnConnectFailed: {reason}");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    /// <summary>
    /// プレイヤーから入力があるたびに呼ばれる
    /// </summary>
    /// <param name="runner">ランナー</param>
    /// <param name="input">入力システム</param>
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // 入力データに入力状況を記録
        PlayerInputData data = new PlayerInputData();
        data.horizontal = Input.GetAxis("Horizontal");
        data.vertical   = Input.GetAxis("Vertical");

        data.isReady = m_IsReady;
        data.isConfirm = m_IsConfirm;
        data.isCancel = m_IsCancel;
        data.isfire = m_IsFire;

        input.Set(data);

        m_IsReady = false;
        m_IsConfirm = false;
        m_IsCancel = false;
        m_IsFire = false;
    }


    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    /// <summary>
    /// 参加可能なセッション（ルーム）の一覧が更新されたときに呼ばれる
    /// </summary>
    /// <param name="runner">ランナー</param>
    /// <param name="sessionList">ルーム一覧</param>
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        // 部屋一覧を取得
        foreach (var session in sessionList)
        {
            // 参加可能な部屋か確認
            if(session.IsOpen &&
                session.PlayerCount < session.MaxPlayers)
            {
                // 部屋がある場合
                if(m_MatchType == EMatchType.RANDOM)
                {
                    // ランダムなら一番最初の部屋に参加
                    StartClient(session.Name);
                    return;
                }
                else if(m_MatchType == EMatchType.PRIVATE)
                {
                    // プライベートルームならその中からRoomKeyを検索して存在すれば部屋に参加する

                    return;
                }
            }
        }

        // 参加可能な部屋がなければ新しく作成
        if (m_MatchType == EMatchType.RANDOM)
        {
            // ランダム使われてない部屋の名前を使用して部屋を作成
            string roomKey = FindUnusedRoom(sessionList);

            StartHost(roomKey);
            return;
        }
        else if (m_MatchType == EMatchType.PRIVATE)
        {
            // プライベートルームなら自分で作成

        }
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    /// <summary>
    /// ネットワークシーンのロードが完了したときに呼ばれる
    /// </summary>
    /// <param name="runner">ランナー</param>
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        //SceneManager.Instance.Init(m_Runner);

        if (!runner.IsServer) return;

        // PlaySceneならプレイヤーを生成
        if(UnityEngine.SceneManagement.SceneManager
           .GetActiveScene().name == "PlayScene")
        {
            // 現在参加しているプレイヤーを生成
            SpawnPlayers();
            SceneManager.Instance.Init(m_Runner);
            ManagerSpawn.Instance.Spawn();
        }

        // リザルトシーンなら、プレイヤーの接続を切る
        if (UnityEngine.SceneManagement.SceneManager
           .GetActiveScene().name == "ResultScene")
        {
            SceneManager.Instance.Init(m_Runner);
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}
