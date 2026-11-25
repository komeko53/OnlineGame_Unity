using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    private NetworkRunner m_Runner = null;
    public NetworkRunner Runner => m_Runner;

    public string NowSceneName { get ; set; }

    [SerializeField]
    private SceneBase m_NowScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Init(NetworkRunner runner)
    {
        m_Runner = runner;
    }

    public NetworkRunner GetRunner()
    {
        return m_Runner;
    }

    public SceneBase GetNowScene()
    {
        return m_NowScene;
    }

    public void ChangeScene(string sceneName)
    {
        if (m_Runner == null) return;

        // サーバー側だけが実行
        if (!m_Runner.IsServer) return;

        int index = UnityEngine.SceneManagement.SceneUtility
            .GetBuildIndexByScenePath($"Assets/Scenes/{sceneName}.unity");

        NowSceneName = sceneName;

        if (index < 0) return;

        m_Runner.LoadScene(SceneRef.FromIndex(index), LoadSceneMode.Single);
    }
}
