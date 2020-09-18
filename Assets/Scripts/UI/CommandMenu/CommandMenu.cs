using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.Interaction;
using Core.InventorySystem;
using Utility.ObjectPools.Generic;

public class CommandMenu : MonoBehaviour
{
    public Action<OperationType, IInteractionComponent> commandCallback = delegate { };
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
    public List<CommandMenuItemData> presetDataBase = new List<CommandMenuItemData>();
    public GenericPool<GameObject> AbviableDisplay = null;

    List<GameObject> _currentDisplay = new List<GameObject>();

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
            if (_remainingActiveTime <= 0)
            {
                _remainingActiveTime = 0;
                gameObject.SetActive(false);
            }
        }
    }

    public void LoadData()
    {
        //Inicializo el displayPool. el pool presupone que el factory inicializa los objetos como desactivados.
        AbviableDisplay = new GenericPool<GameObject>
        (
            presetDataBase.Count,
            createDisplayPreset,
            enableDisplayPreset,
            disableDisplayPreset,
            true
        );
    }

    public GameObject createDisplayPreset()
    {
        var presetInstance = Instantiate(_optionPrefab);
        presetInstance.transform.SetParent(_Content);
        presetInstance.transform.localPosition = Vector3.zero;

        var commandItem = presetInstance.GetComponent<CommandMenuItem>();
        commandItem.OnOperationSelected += ActivateCommand;

        presetInstance.SetActive(false);

        return presetInstance;
    }
    public void enableDisplayPreset(GameObject selectedPreset)
    {
        //Básicamente muestra el preset.
        selectedPreset.SetActive(true);
    }
    public void disableDisplayPreset(GameObject selectedPreset)
    {
        //Oculta el preset.
        selectedPreset.SetActive(false);
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

    /// <summary>
    /// Activate Command se llama al presionar el botón de interacción específico.
    /// </summary>
    /// <param name="operation">Es el tipo de operación que se va a ejecutar</param>
    /// <param name="component">Una referencia al componente seleccionado</param>
    public void ActivateCommand(OperationType operation, IInteractionComponent component)
    {
        commandCallback(operation, component);
        commandCallback = delegate { };
        interactionTarget = null;

        foreach (var obj in _currentDisplay)
            AbviableDisplay.DisablePoolObject(obj);

        _currentDisplay = new List<GameObject>();

        gameObject.SetActive(false);
    }

    public void FillOptions
    ( 
      IInteractable interactionTarget,
      Inventory inventory,
      Action<OperationType, IInteractionComponent> callback
    )
    {
        commandCallback += callback;
        this.interactionTarget = interactionTarget;

        //Tomamos nuestro target y le pedimos los displaySettings de acuerdo a nuestro inventario.
        var DisplaySettings = interactionTarget.GetInteractionDisplaySettings(inventory);
        _limitedActive = DisplaySettings.LimitedDisplay;
        if (_limitedActive)
            _remainingActiveTime = DisplaySettings.ActiveTime;

        _verticalScroll.size = 1;

        foreach (var pair in DisplaySettings.SuportedOperations)
        {
            var display = AbviableDisplay.GetObjectFromPool();
            var displaySetting = display.GetComponent<CommandMenuItem>();

            displaySetting.Data = presetDataBase.Find(x => x.Operation == pair.Item1);
            displaySetting.referenceComponent = pair.Item2;
            _currentDisplay.Add(display);
        }
    }

    public void OnMouseOver_SliderContext(bool isInsideSlider)
    {
        _sliderContextOn = isInsideSlider;
        _viewportContextOn = !isInsideSlider;
        print(string.Format("Slider context is {0}", isInsideSlider ? "On" : "off"));
    }

    public void OnMouseOver_Viewport(bool isInsideViewport)
    {
        _viewportContextOn = isInsideViewport;
        _sliderContextOn = !isInsideViewport;

        if (!_viewportContextOn)
        {
            commandCallback = delegate { };
            interactionTarget = null;

            foreach (var obj in _currentDisplay)
                AbviableDisplay.DisablePoolObject(obj);
            _currentDisplay = new List<GameObject>();

            gameObject.SetActive(false);
        }
    }
}
