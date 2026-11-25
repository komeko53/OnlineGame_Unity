using UnityEngine;
using System.Collections.Generic;
using System.Timers;

public class TitleRailPoint : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> m_RailPoints = new List<GameObject>();

    [SerializeField]
    private TitlePlayer m_Player;

    [SerializeField]
    private float m_RotationDis;

    private int m_Index;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Index = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Player == null || m_RailPoints.Count == 0) return;

        if(m_Index >= m_RailPoints.Count)
        {
            m_Index = 0;
        }

        // レールの
        GameObject point = m_RailPoints[m_Index];
        if (point == null) return;

        // 移動方角を計算
        Vector3 dir = point.transform.position - m_Player.transform.position;
        dir.y = 0.0f;

        // ポイントまでの距離
        float distance = dir.magnitude;
        dir.Normalize();

        // 回転処理
        if (m_Player.IsRotating)
        {
            UpdateRotating(dir);
            return;
        }

        // 一定距離以内なら次のポイントへ
        if(distance <= m_RotationDis)
        {
            m_Player.IsRotating = true;
            m_Index++;
            return;
        }

        // 移動
        m_Player.transform.position += dir * m_Player.Speed * Time.deltaTime;
    }

    void UpdateRotating(Vector3 dir)
    {
        if (m_Player == null || !m_Player.IsRotating) return;

        // 回転値
        Quaternion targetRot = Quaternion.LookRotation(dir);

        m_Player.transform.rotation = Quaternion.RotateTowards(
            m_Player.transform.rotation,
            targetRot,
            m_Player.RotateSpeed * Time.deltaTime
            );

        // 回転完了判定
        if(Quaternion.Angle(m_Player.transform.rotation, targetRot) < 0.1f)
        {
            m_Player.transform.rotation = targetRot;
            m_Player.IsRotating= false;
        }
    }
}
