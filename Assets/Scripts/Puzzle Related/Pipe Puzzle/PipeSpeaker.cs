using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

//This code will speak to all PipeListeners and read instructions from the pipepuzzlemanager.
public class PipeSpeaker : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform puzzleSpawn;
    [SerializeField] private float displacement;
    [SerializeField] private GameObject PipePrefab;
    [SerializeField] private GameObject PipeEnds;
    [SerializeField] private Sprite[] startEndSprites;
    [SerializeField] private Canvas puzzleCanvas;

    [SerializeField] private TextMeshProUGUI winText;

    private List<GameObject> spawnedPipes = new List<GameObject>();

    private bool PuzzleSpawned = false;
    private bool PuzzleVisible = false;

    private InputManager inputManager;

    private int size;
    private PipePuzzleManager manager;

    public bool canInteract { get; set; } = true;

    //Initalize all required components
    private void Start()
    {
        inputManager = GameObject.FindAnyObjectByType<InputManager>();
        manager = GetComponent<PipePuzzleManager>();
        manager = GameObject.FindAnyObjectByType<PipePuzzleManager>();
        size = manager.size;
        
    }

    //Manages the interaction when the player walks up and presses E
    public void Interact()
    {
        if(!canInteract) return;
        if(!PuzzleSpawned) SpawnPuzzle();
        if(PuzzleSpawned) ToggleVisibility();
    }

    private void SpawnPuzzle()
    {
        manager.GenerateNewPuzzle();
        PuzzleSpawned = true;
        SpawnEnds();
        SpawnListeners();
    }

    private void ToggleVisibility()
    {
        if (!PuzzleVisible)
        {
            foreach (GameObject go in spawnedPipes)
            {
                go.SetActive(true);
                puzzleCanvas.gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject go in spawnedPipes)
            {
                go.SetActive(false);
                puzzleCanvas.gameObject.SetActive(false);
            }
        }
        PuzzleVisible = !PuzzleVisible;
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

        spawnedPipes.Add(StartPipe);
        spawnedPipes.Add(EndPipe);
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
                spawnedPipes.Add(pipe);
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
                canInteract = false;
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
