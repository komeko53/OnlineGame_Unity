using Fusion;
using UnityEngine;
using UnityEngine.Windows;

public class Player : NetworkBehaviour
{
    [Networked, HideInInspector]
    public int PlayerIndex { get; set; }

    [Networked, HideInInspector]
    public int ListIndex { get; set; }

    [Networked, HideInInspector]
    public string PlayerColor { get; set; }

    [Networked, HideInInspector]
    public bool IsReady { get; private set; }

    [Networked, HideInInspector]
    public bool IsDead { get; private set; }

    [SerializeField]
    private float m_Speed;

    [SerializeField]
    private float m_RotateSpeed;
    [SerializeField]
    private float m_RotateDistance;

    [SerializeField]
    private float m_ShotInterval;
    private float m_ShotTimer;

    [SerializeField]
    private Renderer m_CubeRenderer;

    [SerializeField]
    private Canvas m_PlayerLabelPrefab;

    private Canvas m_PlayerLabel;

    Quaternion m_TargetRotation;
    Vector3 m_Move = Vector3.zero;
    Vector3 m_Input = Vector3.zero;
    CharacterController m_Controller = null;
    private bool m_IsStop = false;
    private bool m_IsTurning = false; // 反転中
    private bool m_IsNormal = false;

    private void Awake()
    {
        m_Controller = GetComponent<CharacterController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_IsNormal = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        // 移動処理はサーバーのみ行う
        if (!Object.HasStateAuthority) return;
        if (IsDead) return;

        if (GameManager.Instance.IsAllReady())
        {
            // UIを削除
            m_PlayerLabel.gameObject.SetActive(false);

            if(!m_IsStop)
            {
                // 常にPlayerの前進方向へ進む
                m_Move = this.transform.forward * m_Speed * Runner.DeltaTime;
                m_Controller.Move(m_Move);
            }
        }

        // NetworkManagerのCharacterInputで設定された入力データを受け取る
        // 入力情報はDataの中に格納される
        if (GetInput(out NetworkManager.PlayerInputData data))
        {
            m_Input.x = data.horizontal;
            m_Input.z = data.vertical;

            Debug.Log($"X:{m_Input.x}, Z:{m_Input.z}");

            // ゲームスタート
            if (data.isReady && !IsReady)
            {
                IsReady = true;
            }

            // 弾発射
            if (data.isfire &&
                m_ShotTimer <= 0.0f)
            {
                Bullet bullet =
                    BulletManager.Instance.Fire(this.transform.position, this.transform.rotation, Object.InputAuthority);

                SoundManager.Instance.PlaySE("SE_Shot");
                m_ShotTimer = m_ShotInterval;
            }
        }

        if(m_ShotTimer > 0.0f)
        {
            m_ShotTimer -= Runner.DeltaTime;
        }

        if(m_IsTurning)
        {
            // 回転処理
            UpdateRotation();

            if (IsRotationComplete())
            {
                m_IsTurning = false;
                m_IsStop = false;
            }
        }
    }
    public Vector3 GetInput()
    {
        return new Vector3(m_Input.x, 0.0f, m_Input.z);
    }

    private void UpdateRotation()
    {
        if (!m_IsTurning) return;

        this.transform.rotation = Quaternion.RotateTowards(
            this.transform.rotation,
            m_TargetRotation,
            m_RotateSpeed * Runner.DeltaTime
            );
    }

    private bool IsRotationComplete()
    {
        return Quaternion.Angle(
            this.transform.rotation,
            m_TargetRotation
        ) < m_RotateDistance;
    }

    private Quaternion GetTargetRotation()
    {
        Vector3 dir = -this.transform.forward;
        return Quaternion.LookRotation(dir);
    }

    public bool IsTurning()
    {
        return m_IsTurning;
    }

    public void SetStop(bool isStop)
    {
        m_IsStop = isStop;
    }

    public void SetNormal(bool isNormal)
    {
        m_IsNormal = isNormal;
    }

    public void MoveToPointer(Vector3 dir)
    {
        dir.y = 0.0f;

        m_Move = dir * m_Speed * Runner.DeltaTime;
        m_Controller.Move(m_Move);
    }

    public void SetTargetRotation(Quaternion rot)
    {
        if (!m_IsTurning) return;
        m_TargetRotation = rot;
    }

    public void SetMaterial()
    {
        m_CubeRenderer.material = PlayerManager.Instance.GetMaterial(ListIndex);
    }

    public void Dead()
    {
        IsDead = true;
        GameManager.Instance.AddRankData(this);
        Runner.Despawn(Object);
    }

    /// <summary>
    /// スポーンした後の処理
    /// </summary>
    public override void Spawned()
    {
        // マテリアルを設定する
        SetMaterial();
        IsDead = false;

        PlayScene scene = SceneManager.Instance.GetNowScene() as PlayScene;
        if(scene)
        {
            PlayerColor = PlayerManager.Instance.GetPlayerColor(ListIndex);
            scene.SetPlayreColorText(PlayerColor);
        }

        // UIの生成
        m_PlayerLabel = Instantiate(m_PlayerLabelPrefab, this.transform);
        m_PlayerLabel.transform.localPosition = new Vector3(0.0f, 10.0f, 0.0f);
        m_PlayerLabel.transform.localRotation = Quaternion.identity;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // サーバーのみ処理
        if (!HasStateAuthority) return;

        Player otherPlayer = hit.gameObject.GetComponent<Player>();

        if (otherPlayer)
        {
            SoundManager.Instance.PlaySE("SE_PlayerHit");

            // 反転先を保存
            m_TargetRotation = GetTargetRotation();

            m_IsTurning = true;
            m_IsStop = true;
        }
    }
}
