using UnityEngine;

public class SceneBase : MonoBehaviour
{
    [SerializeField]
    private string SceneName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SceneManager.Instance.NowSceneName = SceneName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
