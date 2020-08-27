using UnityEngine;

public class WallTrap : MonoBehaviour
{
    [SerializeField] Animator _anim = null;

    public void ActivateTrap()
    {
        _anim.SetBool("Active", true);
    }

    public void DeactivateTrap()
    {
        _anim.SetBool("Active", false);
    }
}
