using Fusion;
using UnityEngine;

public class Rall : NetworkBehaviour
{
    // レールタイプ
    public enum RallType
    {
        NORMAL,
        TURN,
        END,
    }

    [SerializeField]
    private RallType type;

    public RallType Type => type;
}
