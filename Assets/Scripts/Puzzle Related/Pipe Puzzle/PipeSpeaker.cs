using TMPro;
using Unity.VisualScripting;
using UnityEngine;

//This code will speak to all PipeListeners and read instructions from the pipepuzzlemanager.
public class PipeSpeaker : MonoBehaviour
{
    [SerializeField] private Transform puzzleSpawn;
    [SerializeField] private float displacement;
    [SerializeField] private GameObject PipePrefab;
    [SerializeField] private GameObject PipeEnds;
    [SerializeField] private Sprite[] startEndSprites;

    [SerializeField] private TextMeshProUGUI winText;

    private InputManager inputManager;

    private int size;
    private PipePuzzleManager manager;
    private void Start()
    {
        Debug.Log("PIPE SPEAKER - The process of spawning the pipes is starting");
        inputManager = GameObject.FindAnyObjectByType<InputManager>();
        manager = GetComponent<PipePuzzleManager>();
        size = manager.size;
        SpawnEnds();
        SpawnListeners();
    }

    private void SpawnEnds()
    {
        Vector2Int startPoint = manager.startPoint;
        Vector2Int endPoint = manager.endPoint;

        Vector3 spawnPos = puzzleSpawn.position +
                           new Vector3(startPoint.x * displacement,
                                       startPoint.y * displacement,
                                       0f);
        GameObject StartPipe = Instantiate(PipeEnds, spawnPos, Quaternion.identity);
        SpriteRenderer spriteRenderer = StartPipe.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = startEndSprites[0];

        spawnPos = puzzleSpawn.position +
                           new Vector3(endPoint.x * displacement,
                                       endPoint.y * displacement,
                                       0f);
        GameObject EndPipe = Instantiate(PipeEnds, spawnPos, Quaternion.identity);
        spriteRenderer = EndPipe.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = startEndSprites[1];

        if (manager.isVertical)
        {
            StartPipe.transform.rotation = Quaternion.Euler(0, 0, 90);
            EndPipe.transform.rotation = Quaternion.Euler(0, 0, 90);
        }

    }

    private void SpawnListeners()
    {

        for (int y = size - 1; y >= 0; y--)
        {
            for (int x = 0; x < size; x++)
            {
                Vector3 spawnPos = puzzleSpawn.position +
                           new Vector3(x * displacement,
                                       y * displacement,
                                       0f);
                GameObject pipe = Instantiate(PipePrefab, spawnPos, Quaternion.identity);
                PipeListener listener = pipe.GetComponent<PipeListener>();
                if (manager.grid[x, y] == null)
                {
                    Debug.Log("PIPE SPEAKER - there is no pipe at x/y: " + x + "/" + y);
                    listener.SetSprite(PipeType.empty);
                    continue;
                }
                Pipes currentPipe = manager.grid[x, y];
                listener.thisPipe = currentPipe;
                listener.UpdateRotation();
                listener.SetSprite(currentPipe.type);
            }
        }

    }

    public void CheckWin()
    {
        if(manager != null)
        {
            bool isWon = manager.CheckPuzzleIsSolved();
            if (isWon)
            {
                winText.text = "Game Is Won";
            }
            else
                winText.text = "Game Is Not Won";
        }
    }

    private void Update()
    {
        RaycastClick();
    }

    private void RaycastClick()
    {
        if (inputManager.TouchPressInput)
        {
            //Debug.Log("Screen Position: " + inputManager.TouchPosInput);
            Vector3 screenPos = inputManager.TouchPosInput;
            screenPos.z = Mathf.Abs(Camera.main.transform.position.z);

            Vector3 mouseWorldPos3D = Camera.main.ScreenToWorldPoint(screenPos);

            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos3D);

            if (hit != null)
            {
                PipeListener listener = hit.GetComponent<PipeListener>();

                if (listener != null && listener.thisPipe != null)
                {
                    //Debug.Log("PIPE SPEAKER - Clicked pipe!");
                    //Debug.Log("PIPE SPEAKER - item hit: " + hit.name + " hit coordinates " + hit.gameObject.transform.position);
                    listener.ToggleRotation();
                }
                inputManager.TouchPressInput = false;
            }
        }
    }

}
