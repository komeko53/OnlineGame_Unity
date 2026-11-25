using Fusion;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Bullet : NetworkBehaviour
{
    [Networked]
    public PlayerRef Owner {  get; set; }

    [SerializeField]
    private float m_Life = 10.0f;

    [SerializeField]
    private float m_Speed = 5.0f;

    [SerializeField]
    private Renderer m_Renderer;

    public override void FixedUpdateNetwork()
    {
        // 移動処理はサーバーのみ
        if (!Object.HasStateAuthority) return;

        UpdateMove();
        UpdateLife();
    }

    private void UpdateMove()
    {
        // 前進
        this.transform.position += this.transform.forward * m_Speed * Runner.DeltaTime;
    }

    private void UpdateLife()
    {
        m_Life -= Runner.DeltaTime;
        if(m_Life <= 0.0f)
        {
            Destroy();
        }
    }

    private void SetMaterial()
    {
        Player owner = PlayerManager.Instance.GetPlayer(Owner);
        if (!owner) return;

        m_Renderer.material = PlayerManager.Instance.GetMaterial(owner.ListIndex);
    }

    public void Fire(Vector3 pos, Quaternion rot)
    {
        this.transform.position = pos;
        this.transform.rotation = rot;
    }

    public void Destroy()
    {
        Runner.Despawn(Object);
    }

    public override void Spawned()
    {
        // 弾のPlayerの色に変更
        //SetMaterial();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 当たり判定はサーバーのみ
        if (!Object.HasStateAuthority) return;

        Player otherPlayer = other.GetComponent<Player>();
        if (otherPlayer)
        {
            // 当たったのが撃ったプレイヤーではないか
            if (Owner != otherPlayer.Object.InputAuthority)
            {
                otherPlayer.Dead();
                Destroy();
                SoundManager.Instance.PlaySE("SE_ShotHit");
            }
        }
    }
}
