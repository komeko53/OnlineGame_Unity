using Fusion;
using UnityEngine;
public class ManagerSpawn : MonoBehaviour
{
    [SerializeField]
    NetworkObject m_TimerMgPrefab = null;

    public static ManagerSpawn Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Spawn()
    {
        NetworkRunner runner = SceneManager.Instance.GetRunner();
        if (!runner) return;

        NetworkObject timerMgObj = runner.Spawn(
            m_TimerMgPrefab,
            new Vector3(0.0f, 20.0f, 0.0f),
            Quaternion.identity
            );

        if(!timerMgObj)
        {
            Debug.Log("TimerManager‚ªnull");
        }
    }
}
