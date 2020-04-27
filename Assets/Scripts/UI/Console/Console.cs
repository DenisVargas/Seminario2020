using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Debuging
{
    public class Console : MonoBehaviour
    {
        public static Console c; //Singleton.
        public enum DebugLevel
        {
            warning,
            error
        }

        #region Texto y Consola
        [Header("Componentes")]
        [SerializeField] TMP_Text _consoleText;
        [SerializeField] TMP_InputField _inputField;
        [SerializeField] Scrollbar _scroll;

        [SerializeField]  KeyCode _openKey;
        [SerializeField]  GameObject _activeObject;
        #endregion

        [Header("Debug Level Colours")]
        [SerializeField] Color normalLevelColor = Color.red;
        [SerializeField] Color WarningLevelColor = Color.red;
        [SerializeField] Color errorLevelColor = Color.red;


        Dictionary<string, Action> _commands = new Dictionary<string, Action>();

        private void Awake()
        {
            if (c == null) c = this;
            else Destroy(gameObject);

            _commands.Add("cs", () => { _consoleText.text = ""; });
            _commands.Add("close", () => { _activeObject.SetActive(false); });

            _consoleText.text = "Bienvenido a la consola de comandos!\n";
        }

        // Update is called once per frame
        void Update()
        {
            _scroll.value = 0;
            if (Input.GetKeyDown(_openKey))
                _activeObject.SetActive(!_activeObject.activeSelf);
        }

        public void OnEndEdit(string input)
        {
            if (_commands.ContainsKey(_inputField.text))
                _commands[_inputField.text]();
            else
                Print("This command doesn't exist", DebugLevel.warning);

            _inputField.text = "";
        }

        //Registro.
        public void RegisterComand(string commandName, Action commandFunc)
        {
            if (!_commands.ContainsKey(commandName))
                _commands.Add(commandName, commandFunc);
            else
                _commands[commandName] = commandFunc;
        }

        public void Print(string text, bool defaultPrint = false)
        {
            _consoleText.text += text + "\n";

            if (defaultPrint)
                print(text);
        }
        public void Print(string text, DebugLevel level, bool defaultPrint = false)
        {
            Color debugColor = Color.white;
            switch (level)
            {
                case DebugLevel.warning:
                    debugColor = WarningLevelColor;
                    break;
                case DebugLevel.error:
                    debugColor = errorLevelColor;
                    break;
                default:
                    break;
            }
            _consoleText.text += string.Format("<color=#{0}> {1} </color>\n", ColorUtility.ToHtmlStringRGB(debugColor), text);

            if (defaultPrint)
                print(text);
        }
    }
}

