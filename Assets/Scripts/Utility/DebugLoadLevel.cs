using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugLoadLevel : MonoBehaviour
{
    [SerializeField] int debugLevelToLoad = 2;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SceneManager.LoadScene(debugLevelToLoad);
            Time.timeScale = 1;
        }
    }
}
