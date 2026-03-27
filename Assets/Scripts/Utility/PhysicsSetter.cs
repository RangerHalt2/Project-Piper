using UnityEngine;

public class PhysicsSetter : MonoBehaviour
{

    private void Start()
    {
        int idOne = LayerMask.NameToLayer("Player");
        int idTwo = LayerMask.NameToLayer("PassThrough");
        Physics2D.IgnoreLayerCollision(idOne, idTwo);
    }

}
