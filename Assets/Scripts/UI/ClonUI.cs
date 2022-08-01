using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClonUI : MonoBehaviour
{
    [Header("Screen Indicator")]
    public GameObject ClonIndicator;
    [SerializeField, HideInInspector]
    RectTransform ClonIndicatorRectTransform;
    [SerializeField] bool displayClonIndicator = false;
    [SerializeField, HideInInspector]
    Transform _clon;

    [Header("Cooldown")]
    public Image frontImage = null;
    public GameObject CoolDownDisplayHolder = null;
    [SerializeField, HideInInspector]
    public TMP_Text _cooldownText = null;
    public float MaxCooldown;
    [SerializeField, HideInInspector]
    bool _displayingCooldownText = true;
    float _cooldown;

    public float CoolDownDisplay
    {
        get => _cooldown;
        set
        {
            _cooldown = Mathf.Clamp(value, 0, MaxCooldown);
            float progress = (MaxCooldown - _cooldown) / MaxCooldown;
            frontImage.fillAmount = progress;
            print(progress);
            if(_cooldownText == null && CoolDownDisplayHolder != null)
                _cooldownText = CoolDownDisplayHolder.GetComponentInChildren<TMP_Text>();

            if (_cooldownText)
            {
                _cooldownText.text = System.Math.Round(_cooldown, 1).ToString();

                if (_cooldown == MaxCooldown || progress == 1)
                {
                    frontImage.fillAmount = 1;
                    CoolDownDisplayHolder.SetActive(false);
                    return;
                }
                if (_cooldown < MaxCooldown)
                {
                    CoolDownDisplayHolder.SetActive(true);
                }
            }
        }
    }

    public ClonUI SetMaxCooldownAs(float MaxCooldown)
    {
        this.MaxCooldown = MaxCooldown;
        CoolDownDisplay = MaxCooldown;
        return this;
    }
    public ClonUI SetClonReference(Transform clon)
    {
        this._clon = clon;
        return this;
    }


    // Start is called before the first frame update
    void Start()
    {
        //Indicador.
        if(_clon == null)
            _clon = FindObjectOfType<Controller>().Clon.transform;
        if (ClonIndicator == null)
            Debug.LogError("No asignaste el indicador del clon cabeza de nabo");
        else
        {
            ClonIndicatorRectTransform = ClonIndicator.GetComponent<RectTransform>();
            if (ClonIndicatorRectTransform == null)
                Debug.LogError("El objeto asignado como Indicador del clon no tiene un componente rectTransfrom, ste marmota");
        }
        EnableClonIndicator(displayClonIndicator);

        //Cooldown.
        if (CoolDownDisplayHolder)
        {
            _cooldownText = CoolDownDisplayHolder.GetComponentInChildren<TMP_Text>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (displayClonIndicator && ClonIndicatorRectTransform != null)
            UpdateClonIndicatorPosition();
    }

    public void EnableClonIndicator(bool on)
    {
        ClonIndicator.SetActive(on);
        displayClonIndicator = on;
    }
    private void UpdateClonIndicatorPosition()
    {
        var indicatorPos = Camera.main.WorldToScreenPoint(_clon.position);

        //Si el indicador esta detras de la camara
        if (indicatorPos.z < 0)
        {
            indicatorPos.x *= -1;
            indicatorPos.y *= -1;
        }
        var lastPosition = new Vector3(indicatorPos.x, indicatorPos.y, indicatorPos.z);

        indicatorPos.x = Mathf.Clamp(indicatorPos.x, ClonIndicatorRectTransform.rect.width / 2, Screen.width - ClonIndicatorRectTransform.rect.width / 2);
        indicatorPos.y = Mathf.Clamp(indicatorPos.y, ClonIndicatorRectTransform.rect.height / 2, Screen.height - ClonIndicatorRectTransform.rect.height / 2);
        indicatorPos.z = 0;

        //Actualizamos la posicion y la rotacion.
        ClonIndicatorRectTransform.up = (lastPosition - indicatorPos).normalized;
        ClonIndicatorRectTransform.position = indicatorPos;
    }
}
