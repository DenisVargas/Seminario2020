using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public struct WritteableText
{
    public float writteTick;
    [TextArea]
    public string Message;
    public bool insertNewLineAtStart;
    public bool insertNewLineAtEnd;
}
public class EndGameMenu : MonoBehaviour
{
    public List<WritteableText> Messages = new List<WritteableText>();
    public float timeBetweenMessages = 1f;
    public string ContinueInstruction = "";
    public TMP_Text EndText = null;

    public bool writting = false;

    private void Start()
    {
        EndText.text = "";
        StartCoroutine(writeText());
    }
    // Update is called once per frame
    void Update()
    {
        if (!writting && Input.anyKeyDown)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    public IEnumerator writeText()
    {
        writting = true;

        foreach (var message in Messages)
        {
            if (message.insertNewLineAtStart)
                EndText.text += "\n";
            foreach (char character in message.Message)
            {
                EndText.text += character;
                yield return new WaitForSeconds(message.writteTick);
            }
            if (message.insertNewLineAtEnd)
                EndText.text += "\n";
            yield return new WaitForSeconds(timeBetweenMessages);
        }
        EndText.text += $"\n{ContinueInstruction}";
        writting = false;
    }
}
