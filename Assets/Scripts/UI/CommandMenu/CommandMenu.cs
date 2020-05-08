using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandMenu : MonoBehaviour
{
    public event Action<OperationOptions, IInteractable> executeCommand = delegate { };
    [HideInInspector]
    public IInteractable interactionTarget;

    //Actúa como database de commandos, carga los sciptable objects y los asigna.
    //Realiza todos los comandos de un CommandMenu.
    [Header("References")]
    [SerializeField] GameObject _optionPrefab = null;
    [SerializeField] Scrollbar _verticalScroll = null;
    [SerializeField] Transform _Content = null;

    [Header("Options")]
    [SerializeField] Vector2 posOffset = new Vector2(100, 0);

    public CommandMenuItemData[] presetDataBase = new CommandMenuItemData[0];
    public Dictionary<OperationOptions, GameObject> display = new Dictionary<OperationOptions, GameObject>();

    [Header("Estado del contexto")]
    [SerializeField] bool _sliderContextOn           = false;
    [SerializeField] bool _viewportContextOn         = false;

    public void LoadDisplay()
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Emplace(Vector2 mouseScreenPosition)
    {
        //posicionar el MultiCommandMenu();

        //Necesito saber el width y hight de mi scroolview.
        //El punto pivot esta en el centro asi que el width es size/2.
        Vector2 size = GetComponent<RectTransform>().sizeDelta;

        //float xValue = Input.mousePosition.x + size.x + posOffset.x;
        //float yValue = Input.mousePosition.y + size.y / 2 + posOffset.y;
        GetComponent<RectTransform>().position = new Vector3(mouseScreenPosition.x + size.x / 2 + posOffset.x,
                                                                               mouseScreenPosition.y + posOffset.y, 0);

        //print(string.Format("Posición en x del Mouse es {0} y la posicion en Y es {1}",
        //    Input.mousePosition.x, Input.mousePosition.y));

        //Vector2 screenDimentions = new Vector2(Screen.width, Screen.height); // va de 0 a max width, y de 0 a max Hight

        //TODO: Reposicionar el menu, si se sale de la pantalla.
    }

    public void ActivateCommand(OperationOptions command)
    {
        print(string.Format("Activo el comando {0}", command.ToString()));
        //Aquí es donde ejecutamos la acción en si.
        executeCommand(command, interactionTarget);
        //Limpiamos los residuos.
        executeCommand = delegate { };
        interactionTarget = null;
        gameObject.SetActive(false);
    }

    public void FillOptions(List<OperationOptions> operations, IInteractable interactionTarget, Action<OperationOptions, IInteractable> callBack)
    {
        //Registro el evento
        executeCommand += callBack;
        //Registro el target de las operaciones.
        this.interactionTarget = interactionTarget;

        //Obtengo los childs
        _verticalScroll.size = 1;

        //Recorro opcion por opción y activo solamente aquellos que sean válidos.
        foreach (var item in display)
            item.Value.SetActive(operations.Contains(item.Key));
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

        print(string.Format("Viewport context is {0}", isInsideViewport ? "On" : "off"));
    }
}
