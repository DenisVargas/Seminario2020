using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsyncLoading : MonoBehaviour
{
    [SerializeField] GameObject _loadingIcon = null;
    [SerializeField] GameObject _loadingCompleteAdvice = null;

    AsyncOperation op = null;

    private void Awake()
    {
        _loadingIcon.SetActive(true);
        _loadingCompleteAdvice.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel()
    {
        op = SceneManager.LoadSceneAsync(AsyncSceneLoadOptions.LevelBuildIndex);
        op.allowSceneActivation = false;
        AsyncSceneLoadOptions.LoadActive = true;

        while(!op.isDone)
        {
            if (op.progress >= 0.9f)
            {
                if (_loadingIcon.activeSelf)
                    _loadingIcon.SetActive(false);
                if (!_loadingCompleteAdvice.activeSelf)
                    _loadingCompleteAdvice.SetActive(true);

                if (Input.anyKeyDown)
                    op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}

public static class AsyncSceneLoadOptions
{
    public static int LevelBuildIndex = 0;
    public static bool LoadActive = false;
}
