using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class InspectionMenu : MonoBehaviour
{
    public event Action<bool> OnSetInspection = delegate { };

    static InspectionMenu _instance = null;
    public static InspectionMenu main
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType<InspectionMenu>();

                if (!_instance)
                    _instance = new GameObject("InspectionMenu").AddComponent<InspectionMenu>();
            }

            return _instance;
        }
    }

    [SerializeField] Animator _anim          = null;
    [SerializeField] TMP_Text _mainText      = null;
    [SerializeField] TMP_Text _buttonText    = null;
    [SerializeField] float _displayDelay     = 0.1f;
    [SerializeField] string Next_ButtonText = "Next";
    [SerializeField] string Close_ButtonText = "Close";

    private bool locked                      = false;
    bool confirmExit                         = false;
    Coroutine _mainDisplay                   = null;

    int AnimStage = 0;

    private void Awake()
    {
        if (_anim == null)
            _anim = GetComponent<Animator>();
    }

    public void OnPressedMainButton()
    {
        confirmExit = true;
    }

    public void DisplayText(string[] text, Action OnFinish)
    {
        //Debug.LogWarning("DisplayText!!");

        if (_mainText == null)
            Debug.LogError("La referencia al Texto Principal no esta seteada");

        if (!isActiveAndEnabled)
            gameObject.SetActive(true);

        if (!locked)
        {
            _anim.SetBool("Enabled", true);
            _mainDisplay = StartCoroutine(DelayedShow(text, OnFinish));
            OnSetInspection(true);
        }
    }

    public void OnEnabledFinished_AnimEvent()
    {
        //Debug.Log("Animacion de activación completada");
        AnimStage = 1;
    }
    public void OnDisablingStarted_AnimEvent()
    {
        //Debug.Log("Animacion de desactivación iniciada");
    }
    public void OnDisablingEnded_AnimEvent()
    {
        //Debug.Log("Animacion de desactivación finalizada");
        OnSetInspection(false);
    }

    IEnumerator DelayedShow(string[] text, Action OnFinish)
    {
        locked = true;
        confirmExit = false;
        _mainText.text = "";

        int toRender = text.Length;
        if (toRender > 1 && _buttonText)
            _buttonText.text = Next_ButtonText;
        if (toRender == 1 && _buttonText)
            _buttonText.text = Close_ButtonText;

        //Activación del menú.
        while (AnimStage == 0)
            yield return null;

        int CurrentRenderedIndex = 0;
        string ToDisplayText = text[CurrentRenderedIndex];
        while(CurrentRenderedIndex < toRender)
        {
            foreach (var character in ToDisplayText)
            {
                _mainText.text += character;

                if (Input.GetKeyDown(KeyCode.Space) || confirmExit)
                {
                    _mainText.text = ToDisplayText;
                    confirmExit = false;
                    break;
                }

                yield return new WaitForSeconds(_displayDelay);
            }

            yield return new WaitForSeconds(0.1f);

            CurrentRenderedIndex++;
            if (CurrentRenderedIndex >= toRender ) //Termine TODAS las renderizaciones.
            {
                if (_buttonText)
                    _buttonText.text = Close_ButtonText;

                while (true)
                {
                    //print("Esperando que confirme la wea ctm");

                    if (Input.GetKeyDown(KeyCode.Space) || confirmExit)
                    {
                        _anim.SetBool("Enabled", false);
                        break;
                    }

                    yield return null;
                }

                break;
            }

            //Todavía hay renderizaciones disponibles.
            while(true)
            {
                //print("Esperando confirmacióne para saltar al siguiente!");

                if (Input.GetKeyDown(KeyCode.Space) || confirmExit)
                    break;

                yield return null;
            }
        }

        //Closing
        locked = false;
        AnimStage = 0;
        _mainDisplay = null;
    }
}
