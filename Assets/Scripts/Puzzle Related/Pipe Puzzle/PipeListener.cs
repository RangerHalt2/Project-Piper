using UnityEngine;

//This piece of code represents the physical element of the pipe for the player to touch.
public class PipeListener : MonoBehaviour
{

    private PipePuzzleManager puzzelManager;
    private Vector2Int respectiveCoordinate;

    public Pipes thisPipe;

    [SerializeField] private Sprite[] Sprite;
    [SerializeField] private SpriteRenderer spriteRenderer;


    public void SetSprite(PipeType pipeSprite)
    {
        spriteRenderer.sprite = Sprite[((int)pipeSprite)];
    }

    public void ToggleRotation()
    {
        thisPipe.rotations++;
        UpdateRotation();
    }

    public void UpdateRotation()
    {
        gameObject.transform.rotation = Quaternion.Euler(0, 0, (thisPipe.rotations % 4) * -90);
    }

}
