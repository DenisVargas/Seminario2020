using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandMenu : MonoBehaviour
{
    public event Action<OperationType, IInteractable> executeCommand = delegate { };
    [HideInInspector]
    public IInteractable interactionTarget;

    //Actúa como database de commandos, carga los sciptable objects y los asigna.
    //Realiza todos los comandos de un CommandMenu.
    [Header("References")]
    [SerializeField] GameObject _optionPrefab = null;
    [SerializeField] Scrollbar _verticalScroll = null;
    [SerializeField] Transform _Content = null;

    [Header("Options")]
    [SerializeField] Vector2 posOffset = new Vector2(0, 0);

    [SerializeField]
    public CommandMenuItemData[] presetDataBase = new CommandMenuItemData[0];
    public Dictionary<OperationType, GameObject> display = new Dictionary<OperationType, GameObject>();

    [Header("Estado del contexto")]
    [SerializeField] bool _sliderContextOn           = false;
    [SerializeField] bool _viewportContextOn         = false;

    bool _limitedActive = false;
    [SerializeField] float _remainingActiveTime = 5f;

    private void Update()
    {
        if (_limitedActive)
        {
            _remainingActiveTime -= Time.deltaTime;
            print("Remaining Active Time = " + _remainingActiveTime);

            if (_remainingActiveTime <= 0)
            {
                _remainingActiveTime = 0;
                gameObject.SetActive(false);
            }
        }
    }

    public void LoadData()
    {
        //Cargo la data y instancio el preset visual por cada uno.
        foreach (var data in presetDataBase)
        {
            var presetInstance = Instantiate(_optionPrefab);
            presetInstance.transform.SetParent(_Content);
            presetInstance.transform.localPosition = Vector3.zero;

            var commandItem = presetInstance.GetComponent<CommandMenuItem>();
            commandItem.Data = data;
            commandItem.OnOperationSelected += ActivateCommand;

            display.Add(commandItem.Data.Operation, presetInstance);
        }
    }

    public void Emplace(Vector2 mouseScreenPosition)
    {
        //posicionar el MultiCommandMenu();

        //Necesito saber el width y hight de mi scroolview.
        //El punto pivot esta en el centro asi que el width es size/2.
        float sizeY = GetComponent<RectTransform>().rect.height;
        float sizeX = GetComponent<RectTransform>().rect.width;

        //float xValue = Input.mousePosition.x + size.x + posOffset.x;
        //float yValue = Input.mousePosition.y + size.y / 2 + posOffset.y;
        GetComponent<RectTransform>().position = new Vector3(mouseScreenPosition.x + posOffset.x,
                                                             mouseScreenPosition.y + posOffset.y, 0);

        //print(string.Format("Posición en x del Mouse es {0} y la posicion en Y es {1}",
        //    Input.mousePosition.x, Input.mousePosition.y));

        //Vector2 screenDimentions = new Vector2(Screen.width, Screen.height); // va de 0 a max width, y de 0 a max Hight

        //TODO: Reposicionar el menu, si se sale de la pantalla.
    }

    public void ActivateCommand(OperationType command)
    {
        //print(string.Format("Activo el comando {0}", command.ToString()));
        //Aquí es donde ejecutamos la acción en si.
        executeCommand(command, interactionTarget);
        //Limpiamos los residuos.
        executeCommand = delegate { };
        interactionTarget = null;
        gameObject.SetActive(false);
    }

    public void FillOptions(InteractionParameters Interaction, IInteractable interactionTarget, Action<OperationType, IInteractable> callBack)
    {
        executeCommand += callBack;
        this.interactionTarget = interactionTarget;
        _limitedActive = Interaction.LimitedDisplay;
        if (_limitedActive)
        {
            _remainingActiveTime = Interaction.ActiveTime;
        }
        _verticalScroll.size = 1;
        foreach (var item in display)
            item.Value.SetActive(Interaction.SuportedOperations.Contains(item.Key));
    }

    public void OnSliderContext(bool isInsideSlider)
    {
        _sliderContextOn = isInsideSlider;
        _viewportContextOn = !isInsideSlider;
        print(string.Format("Slider context is {0}", isInsideSlider ? "On" : "off"));
    }

    public void OnViewportContext(bool isInsideViewport)
    {
        _viewportContextOn = isInsideViewport;
        _sliderContextOn = !isInsideViewport;

        if (!_viewportContextOn)
        {
            executeCommand = delegate { };
            interactionTarget = null;
            gameObject.SetActive(false);
        }

        //print(string.Format("Viewport context is {0}", isInsideViewport ? "On" : "off"));
    }
}
