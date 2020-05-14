using UnityEngine;

public class DisableView : PooleableComponent
{
    [SerializeField] float MinPlayerDistance = 0.5f;

    [SerializeField] Transform player;

    private void Awake()
    {
        var searched = FindObjectOfType<NMA_Controller>();
        if (searched != null)
            player = searched.transform;
        else
            Debug.LogError("DisableView::ERROR\nTe olvidaste de poner o activar el Player en la Escena Salamín!");

    }
    private void Update()
    {
        Vector3 dst = player.transform.position - transform.position;
        if (dst.magnitude < MinPlayerDistance)
            Dispose();
    }

    public override void Enable()
    {
        gameObject.SetActive(true);
        //if (DisablesInTime)
        //{
        //    StartCoroutine(DelayedDisable());
        //}
    }
    public override void Disable()
    {
        gameObject.SetActive(false);
    }

    public void SetPlayerDistance(float dst)
    {
        MinPlayerDistance = dst;
    }
}
