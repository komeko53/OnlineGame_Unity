using Fusion;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class BulletManager : MonoBehaviour
{
    [SerializeField]
    NetworkObject m_BulletPrefab_1 = null;

    [SerializeField]
    NetworkObject m_BulletPrefab_2 = null;

    [SerializeField]
    NetworkObject m_BulletPrefab_3 = null;

    [SerializeField]
    NetworkObject m_BulletPrefab_4 = null;

    public static BulletManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Bullet Fire(Vector3 pos, Quaternion rot, PlayerRef owner)
    {
        NetworkRunner runner = SceneManager.Instance.GetRunner();
        if (!runner) return null;

        Player player = PlayerManager.Instance.GetPlayer(owner);

        NetworkObject bulletObj = runner.Spawn(GetBulletObj(player.PlayerIndex));
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.Fire(pos, rot);

        bullet.Owner = owner;

        return bullet;
    }

    private NetworkObject GetBulletObj(int playerIndex)
    {
        if(playerIndex == 1)
        {
            return m_BulletPrefab_1;
        }
        else if(playerIndex == 2)
        {
            return m_BulletPrefab_2;
        }
        else if (playerIndex == 3)
        {
            return m_BulletPrefab_3;
        }
        else if (playerIndex == 4)
        {
            return m_BulletPrefab_4;
        }

        return m_BulletPrefab_1;
    }
}
