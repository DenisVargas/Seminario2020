using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class destroyme : MonoBehaviour, IDestructible
{
    [SerializeField] UnityEvent OnDestroy = null;
    [SerializeField] UnityEvent OnReset = null;

    [SerializeField] GameObject mainViewMesh = null;
    [SerializeField] GameObject destructedVersion = null;
    [SerializeField] float destroyForce = 30f;

    struct transformValues
    {
        public Vector3 position;
        public Quaternion rotation;
    }
    Rigidbody[] Rigidbodies;

    Collider _mainCollider;

    public Vector3 position => transform.position;

    void Awake()
    {
        _mainCollider = GetComponent<Collider>();
        Rigidbodies = GetComponentsInChildren<Rigidbody>();
        destructedVersion.SetActive(false);
    }

    public void destroyMe()
    {
        if (mainViewMesh != null && destructedVersion != null)
        {
            _mainCollider.enabled = false;
            mainViewMesh.SetActive(false);
            destructedVersion.SetActive(true);

            foreach (var rb in Rigidbodies)
            {
                rb.AddExplosionForce(destroyForce, transform.position, 4);
            }

            StartCoroutine(DeactivateRigidbodies());
        }
    }

    void ResetDestroyedVersion()
    {
        for (int i = 0; i < Rigidbodies.Length; i++)
        {
            Rigidbodies[i].isKinematic = false;
            Rigidbodies[i].gameObject.GetComponent<Collider>().enabled = true;
            var trans = Rigidbodies[i].gameObject.transform;
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
        }

        OnReset.Invoke();
    }

    IEnumerator DeactivateRigidbodies()
    {
        yield return new WaitForSeconds(3);

        for (int i = 0; i < Rigidbodies.Length; i++)
        {
            Rigidbodies[i].isKinematic = true;
            Rigidbodies[i].gameObject.GetComponent<Collider>().enabled = false;
        }

        OnDestroy.Invoke();
    }
}
