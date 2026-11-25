using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class RailPoint : NetworkBehaviour
{
    public enum RailPointType
    {
        NORMAL, // 到達したら自動で方向転換
        WAIT,   // プレイヤーの入力を待ってから方向転換
    }

    // 方向の種類
    public enum DirectionType
    {
        NORTH,  // 北
        SOUTH,  // 南
        EAST,   // 東
        WEST,   // 西
    }

    // プレイヤーのデータを記録
    public struct RailPlayerData
    {
        public Player player;
        public DirectionType type;
        public DirectionType targetDir;
        public float prevDistance;
        public bool isRotating;
        public bool isWaiting;
    }


    [SerializeField]
    private List<DirectionType> m_Directions = new List<DirectionType>();

    [SerializeField]
    private RailPointType m_DirectionType;

    [SerializeField]
    private float m_RotateDistance;
    [SerializeField]
    private float m_RotateSpeed;
    [SerializeField]
    private float m_RayLength;

    // rayに当たってるPlayerリスト
    private List<RailPlayerData> m_PlayerDatas = new List<RailPlayerData>();

    public override void FixedUpdateNetwork()
    {
        // 移動処理はサーバーのみ行う
        if (!Object.HasStateAuthority) return;

        UpdateRay();
        UpdatePlayers();
    }

    private void UpdateRay()
    {
        Vector3 origin = this.transform.position;
        Vector3 dir = Vector3.zero;

        foreach (DirectionType type in m_Directions)
        {
            dir = GetDirVector3(type);

            if(Physics.Raycast(origin, dir, out RaycastHit hit, m_RayLength))
            {
                Player player = hit.collider.GetComponent<Player>();

                if (player != null && player.IsReady)
                {
                    AddPlayer(player, type);
                }
            }
        }
    }

    private void UpdatePlayers()
    {
        for (int i = m_PlayerDatas.Count - 1; i >= 0; i--)
        {
            RailPlayerData data = m_PlayerDatas[i];

            if (data.player == null)
            {
                m_PlayerDatas.RemoveAt(i);
                continue;
            }

            float currentDis = Vector3.Distance(
                data.player.transform.position,
                this.transform.position
            );

            // 回転中
            if (data.isRotating)
            {
                UpdateRotation(data.player, data.targetDir);

                if (IsRotationComplete(data.player, data.targetDir))
                {
                    data.player.SetStop(false);
                    m_PlayerDatas.RemoveAt(i);
                    continue;
                }
            }
            // 入力待ち中
            else if (data.isWaiting)
            {
                UpdateWaitPlayer(ref data);
            }
            // 回転開始判定
            else if (IsGettingCloset(currentDis))
            {
                if (m_DirectionType == RailPointType.NORMAL)
                {
                    data.isRotating = true;
                }
                else if (m_DirectionType == RailPointType.WAIT)
                {
                    // まず入力方向を確認
                    if (UpdateWaitPlayer(ref data))
                    {

                    }
                    else
                    {
                        // 入力がなければ、入ってきた方向の反対側を確認
                        Vector3 input = data.player.GetInput();

                        if (input.sqrMagnitude < 0.01f)
                        {
                            DirectionType opposite = GetOppositeDirection(data.type);

                            if (IsValidDirection(opposite))
                            {
                                // 反対側にレールがあれば直進
                                data.targetDir = opposite;
                                data.isRotating = true;
                            }
                            else
                            {
                                // 進める方向がないので停止
                                data.player.SetStop(true);
                                data.isWaiting = true;
                            }
                        }
                        else
                        {
                            // 入力はあるが、入力方向にレールがない
                            data.player.SetStop(true);
                            data.isWaiting = true;
                        }
                    }
                }
                else if (IsMovingAway(currentDis, data.prevDistance))
                {
                    data.player.SetStop(false);
                    m_PlayerDatas.RemoveAt(i);
                    continue;
                }

                data.prevDistance = currentDis;
                m_PlayerDatas[i] = data;
            }
        }
    }

    private void UpdateRotation(Player player, DirectionType direction)
    {
        Quaternion targetRotation = GetTargetRotation(direction);

        player.transform.rotation = Quaternion.RotateTowards(
            player.transform.rotation,
            targetRotation,
            m_RotateSpeed * Runner.DeltaTime
            );
    }

    private void AddPlayer(Player player, DirectionType type)
    {
        foreach(RailPlayerData data in m_PlayerDatas)
        {
            if(data.player == player)
            {
                return;
            }
        }

        float distance = Vector3.Distance(
            player.transform.position,
            this.transform.position
            );

        RailPlayerData newData = new RailPlayerData
        {
            player = player,
            type = type,
            targetDir = GetNextDirection(type),
            prevDistance = distance,
            isRotating = false,
            isWaiting = false
        };

        m_PlayerDatas.Add(newData);
    }

    private void RemovePlayer(Player player)
    {
        m_PlayerDatas.RemoveAll(data => data.player == player);
    }

    // 近づいているか
    private bool IsApproaching(float currentDis, float prevDis)
    {
        return currentDis < prevDis;
    }

    // 離れていってるか
    private bool IsMovingAway(float currentDis, float prevDis)
    {
        return currentDis > prevDis;
    }

    // 距離が近くなっているか
    private bool IsGettingCloset(float currentDis)
    {
        return currentDis <= m_RotateDistance;
    }

    private bool IsRotationComplete(Player player, DirectionType direction)
    {
        Quaternion targetRotation = GetTargetRotation(direction);

        return Quaternion.Angle(
            player.transform.rotation,
            targetRotation
        ) < m_RotateDistance;
    }

    private void OnDrawGizmos()
    {
        if (m_Directions == null) return;

        Gizmos.color = Color.green;
        Vector3 dir = Vector3.zero;

        foreach(DirectionType type in m_Directions)
        {
            dir = GetDirVector3(type);

            Gizmos.DrawRay(
                this.transform.position,
                dir * m_RayLength);
        }
    }

    private Vector3 GetDirVector3(DirectionType type)
    {
        Vector3 dir = Vector3.zero;

        switch (type)
        {
            case DirectionType.NORTH: return  this.transform.forward;
            case DirectionType.SOUTH: return -this.transform.forward;
            case DirectionType.EAST:  return  this.transform.right;
            case DirectionType.WEST:  return -this.transform.right;
        }

        return Vector3.zero;
    }

    private DirectionType GetNextDirection(DirectionType current)
    {
        foreach(DirectionType direction in m_Directions)
        {
            if (direction == current) continue;
            return direction;
        }

        return current;
    }

    private Quaternion GetTargetRotation(DirectionType direction)
    {
        Vector3 dir = GetDirVector3(direction);

        return Quaternion.LookRotation(dir);
    }

    private bool GetInputDirection(Player player, out DirectionType direction)
    {
        direction = DirectionType.NORTH;

        Vector3 input = player.GetInput();

        if (input.sqrMagnitude < 0.01f)
        {
            return false;
        }

        // 前後の入力
        if (Mathf.Abs(input.z) > Mathf.Abs(input.x))
        {
            direction = input.z > 0
                ? DirectionType.NORTH
                : DirectionType.SOUTH;
        }
        // 左右の入力
        else
        {
            direction = input.x > 0
                ? DirectionType.EAST
                : DirectionType.WEST;
        }

        return true;
    }

    private bool IsValidDirection(DirectionType direction)
    {
        return m_Directions.Contains(direction);
    }

    private bool UpdateWaitPlayer(ref RailPlayerData data)
    {
        Vector3 input = data.player.GetInput();

        DirectionType direction;

        // 前後左右の入力値を判定
        if (Mathf.Abs(input.x) > Mathf.Abs(input.z))
        {
            if (input.x > 0.1f)
            {
                direction = DirectionType.EAST;
            }
            else if (input.x < -0.1f)
            {
                direction = DirectionType.WEST;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (input.z > 0.1f)
            {
                direction = DirectionType.NORTH;
            }
            else if (input.z < -0.1f)
            {
                direction = DirectionType.SOUTH;
            }
            else
            {
                return false;
            }
        }

        // 接続されている方向か確認
        if (!IsValidDirection(direction))
        {
            return false;
        }

        data.targetDir = direction;
        data.isWaiting = false;
        data.isRotating = true;

        return true;
    }

    private DirectionType GetOppositeDirection(DirectionType direction)
    {
        switch (direction)
        {
            case DirectionType.NORTH:
                return DirectionType.SOUTH;

            case DirectionType.SOUTH:
                return DirectionType.NORTH;

            case DirectionType.EAST:
                return DirectionType.WEST;

            case DirectionType.WEST:
                return DirectionType.EAST;
        }

        return direction;
    }
}
