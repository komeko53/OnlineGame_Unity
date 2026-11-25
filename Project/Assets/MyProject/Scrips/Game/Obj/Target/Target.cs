using Fusion;
using UnityEngine;

public class Target : NetworkBehaviour
{
    private Player m_Owner;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Spawned()
    {
        m_Owner = GetComponentInParent<Player>();
    }

    /// <summary>
    /// “–‚½‚Á‚½’e‚ª‚Ç‚±‚É‚ ‚é‚©
    /// </summary>
    /// <returns>’e‚ªƒvƒŒƒCƒ„[‚ÌŒã‚ë‚É‚ ‚é‚©‚Ì”»’èŒ‹‰Ê</returns>
    private bool IsHitFromBehind(Bullet bullet)
    {
        if(!bullet) return false;

        Vector3 toBullet = 
            (bullet.transform.position - this.transform.position).normalized;

        float dot = Vector3.Dot(this.transform.forward, toBullet);

        return dot < 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ƒT[ƒo[‚Ì‚İˆ—‚ğ‚·‚é
        if (!Object.HasStateAuthority) return;

        Bullet otherBullet = other.GetComponent<Bullet>();

        //if(otherBullet)
        //{
        //    // ©•ª‚Ì’e‚¶‚á‚È‚¢ ‚©‚Â ’e‚ªPlayer‚ÌŒã‚ë‚É‚ ‚éê‡
        //    if(otherBullet.Owner != m_Owner.Object.InputAuthority &&
        //       IsHitFromBehind(otherBullet))
        //    {
        //        // Player‚Æ’e‚ğÁ‚·
        //        m_Owner.Dead();
        //    }
        //    else
        //    {
        //        // ’e‚¾‚¯‚ªÁ‚¦‚é
        //        otherBullet.Destroy();
        //    }
        //}
    }
}
