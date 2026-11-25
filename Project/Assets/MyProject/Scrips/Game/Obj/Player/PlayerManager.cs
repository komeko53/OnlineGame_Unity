using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField, Header("スポーンリスト")]
    private List<GameObject> m_SpawnPoss = new List<GameObject>();

    [SerializeField, Header("マテリアルリスト")]
    private List<Material> m_Materials = new List<Material>();

    [SerializeField, Header("プレイヤーの色")]
    private List<string> m_PlayerColors = new List<string>();

    private Dictionary<PlayerRef, Player> m_Players = new Dictionary<PlayerRef, Player>();

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    public GameObject GetSpawnPos(int listIndex)
    {
        if (listIndex < 0 ||
            listIndex >= m_SpawnPoss.Count) return m_SpawnPoss[0];
        return m_SpawnPoss[listIndex];
    }

    public Material GetMaterial(int listIndex)
    {
        if (listIndex < 0 ||
            listIndex >= m_Materials.Count) return m_Materials[0];
        return m_Materials[listIndex];
    }
    public string GetPlayerColor(int listIndex)
    {
        if (listIndex < 0 ||
            listIndex >= m_PlayerColors.Count) return m_PlayerColors[0];
        return m_PlayerColors[listIndex];
    }

    public void AddPlayer(Player player)
    {
        PlayerRef playerRef = player.Object.InputAuthority;

        if (m_Players.ContainsKey(playerRef)) return;
        m_Players[playerRef] = player;
    }

    public void RemovePlayer(PlayerRef playerRef)
    {
        m_Players.Remove(playerRef);
    }

    public Player GetPlayer(PlayerRef playerRef)
    {
        m_Players.TryGetValue(playerRef, out Player player);
       return player;
    }

    public Dictionary<PlayerRef, Player> GetPlayers()
    {
        return m_Players;
    }
}
