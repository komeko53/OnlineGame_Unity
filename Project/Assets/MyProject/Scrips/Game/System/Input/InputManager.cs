using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public bool OnReady { get; private set; }
    public bool OnConfirm { get; private set; }
    public bool OnCancel { get; private set; }
    public bool OnFire { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        OnReady = Ready();
        OnConfirm = Confirm();
        OnCancel = Cancel();
        OnFire = Fire();
    }

    public bool Ready()
    {
        return Input.GetKeyDown(KeyCode.Return) ||
               Input.GetKeyDown(KeyCode.JoystickButton1);
    }

    public bool Confirm()
    {
        return Input.GetKeyDown(KeyCode.Return) ||
               Input.GetKeyDown(KeyCode.JoystickButton1);
    }

    public bool Cancel()
    {
        return Input.GetKeyDown(KeyCode.Q) ||
               Input.GetKeyDown(KeyCode.JoystickButton0);
    }

    public bool Fire()
    {
        return Input.GetKeyDown(KeyCode.Space) ||
               Input.GetKeyDown(KeyCode.JoystickButton1);
    }
}
