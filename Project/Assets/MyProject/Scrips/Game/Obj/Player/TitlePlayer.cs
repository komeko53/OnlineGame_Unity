using System.Runtime.CompilerServices;
using UnityEngine;

public class TitlePlayer : MonoBehaviour
{
    [SerializeField]
    private float m_Speed;

    [SerializeField]
    private float m_RotateSpeed;

    public bool IsRotating {  get; set; }

    public float Speed => m_Speed;
    public float RotateSpeed => m_RotateSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IsRotating = false;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
