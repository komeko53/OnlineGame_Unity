using UnityEngine;

public class RallDetector : MonoBehaviour
{
    [SerializeField]
    Player m_Owner;

    private void OnTriggerEnter(Collider other)
    {
        if (!m_Owner) return;

        // サーバーのみ処理する
        if (!m_Owner.Object.HasStateAuthority) return;

        if(other.CompareTag("Rail"))
        {
            Debug.Log("Railに当たった");
        }
    }
}
