using System.Collections.Generic;
using UnityEngine;

public class PipePuzzleManager : MonoBehaviour
{
    #region Serial Variables
    [Tooltip("How big the grid is; size = 3 then there will be 9 spaces as a 3x3 grid")]
    public int size;
    [Tooltip("There is no implementation as of this current moment for a custom puzzle, leave this false")]
    [SerializeField] private bool isCustomPuzzle = false; //Default Assumption is the puzzle is computer generated
    #endregion

    #region Non-Serial Variables
    [HideInInspector] public Vector2Int startPoint;
    [HideInInspector] public Vector2Int endPoint;

    [HideInInspector] public bool isVertical;

    [HideInInspector] public Pipes[,] grid;
    #endregion


    private void Awake()
    {
        string[,] miniGrid = new string[size, size];
        for (int y = size - 1; y >= 0; y--) // print top row first
        {
            for (int x = 0; x < size; x++)
            {
                miniGrid[x, y] = "x/y: " + x + "/" + y;
            }
        }

        PrintMiniGrid(miniGrid);


        InitializeGrid();
        InitializePuzzle();
    }

    private void InitializeGrid()
    {
        grid = new Pipes[size, size];
        isVertical = ((int)Random.Range(0, 2) == 1); //The puzzle is vertical if the number is 1. Range is 0-1.
    }

    private void InitializePuzzle()
    {
        if (!isVertical)
        {
            Debug.Log("PIPE PUZZLE MANAGER - Puzzle is Horizontal");
            startPoint = new Vector2Int(0 - 1, Random.Range(0, size));
            endPoint = new Vector2Int(size, Random.Range(0, size));
        }
        else
        {
            Debug.Log("PIPE PUZZLE MANAGER - Puzzle is Vertical");
            startPoint = new Vector2Int(Random.Range(0, size), 0 - 1);
            endPoint = new Vector2Int(Random.Range(0, size), size);
        }

        Debug.Log("PIPE PUZZLE MANAGER - Start Point Coordinate: " + startPoint.ToString() + "; End Point Coordinate: " + endPoint.ToString());

        PlaceSolution();
        PrintGrid();
        PopulateRemainingGrid();
        while (CheckPuzzleIsSolved())
        {
            Debug.Log("PIPE PUZZLE MANAGER - The puzzle is solved!");
            RandomlyRotateGrid();
        }

        PrintGrid();
    }

    //This function will return true if the puzzle is in a solved state, and false otherwise
    public bool CheckPuzzleIsSolved()
    {
        bool ret = false;

        // Get the starting square to begin the solution to the puzzle
        Vector2Int startingCoordinate = startPoint;
        startingCoordinate.x = Mathf.Clamp(startingCoordinate.x, 0, size - 1);
        startingCoordinate.y = Mathf.Clamp(startingCoordinate.y, 0, size - 1);

        Vector2Int currentCoordinate = startingCoordinate;

        ret = RecursiveNavigation(currentCoordinate, new HashSet<Vector2Int>());

        return ret;
    }

    //Helper function to the CheckPuzzleIsSolved, recursively navigates through the grid confirming the pipe layout.
    private bool RecursiveNavigation(Vector2Int coords, HashSet<Vector2Int> visited)
    {
        if (!IsInGrid(coords))
            return coords == endPoint;
        if (grid[coords.x, coords.y] == null)
            return false;
        if (visited.Contains(coords))
            return false;

        visited.Add(coords);

        for (int i = 0; i < grid[coords.x, coords.y].exits.Length; i++)
        {
            Vector2Int next = coords + grid[coords.x, coords.y].RotatedExit(i);
            if (RecursiveNavigation(next, visited))
                return true;
        }

        visited.Remove(coords); // backtrack
        return false;
    }

    private void PrintGrid()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int y = size - 1; y >= 0; y--) // print top row first
        {
            for (int x = 0; x < size; x++)
            {
                if (grid[x, y] != null)
                    sb.Append(grid[x, y].CurrRepresentation());
                else
                    sb.Append(" ? ");

                sb.Append(" | ");
            }

            sb.AppendLine();
            sb.AppendLine(new string('-', size * 6));
        }

        Debug.Log("PIPE PUZZLE MANAGER -\n" + sb.ToString());
    }

    private void PrintMiniGrid(string[,] miniGrid)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int y = size - 1; y >= 0; y--) // print top row first
        {
            for (int x = 0; x < size; x++)
            {
                sb.Append(miniGrid[x, y]);

                sb.Append(" | ");
            }

            sb.AppendLine();
            sb.AppendLine(new string('-', size * 6));
        }

        Debug.Log("PIPE PUZZLE MANAGER -\n" + sb.ToString());
    }

    #region Puzzle Set Up
    //This function ensures that there is a valid way to solve the puzzle.
    private void PlaceSolution()
    {
        // Get the starting square to begin the solution to the puzzle
        Vector2Int startingCoordinate = startPoint;
        startingCoordinate.x = Mathf.Clamp(startingCoordinate.x, 0, size-1);
        startingCoordinate.y = Mathf.Clamp(startingCoordinate.y, 0, size-1);

        //Get the ending square where the puzzle must be solved at.
        Vector2Int endCoordinate = endPoint;
        endCoordinate.x = Mathf.Clamp(endPoint.x, 0, size-1);
        endCoordinate.y = Mathf.Clamp(endPoint.y, 0, size-1);

        Debug.Log("PIPE PUZZLE MANAGER - Start Point Coordinate: " + startingCoordinate.ToString() + "; End Point Coordinate: " + endCoordinate.ToString());
        Vector2Int currentCoordinate = startingCoordinate;
        Vector2Int previousCoordinate = startPoint;

        int attempts = 0;
        const int maxAttempts = 100;

        bool done = false;
        do
        {
            do
            {
                //Debug.Log("PIPE PUZZLE MANAGER - Attempting to place a pipe at location: " + currentCoordinate.ToString());
                grid[currentCoordinate.x, currentCoordinate.y] = new Pipes();
                attempts++;
                if (attempts > maxAttempts)
                {
                    Debug.LogError("PIPE PUZZLE MANAGER - Failed to place valid pipe. Aborting.");
                    break;
                }
            } while (!ValidPipePlacement(currentCoordinate, previousCoordinate));
            if (attempts > maxAttempts)
                break;
            else
                attempts = 0;
            //Debug.Log("PIPE PUZZLE MANAGER - Successfully Placed a pipe at location: " + currentCoordinate.ToString());
            if (currentCoordinate == endCoordinate)
            {
                done = true;
                break;
            }
            previousCoordinate = currentCoordinate;
            currentCoordinate = GenerateNextPipePlacement(currentCoordinate);
        } while (!done);
    }



    private void PopulateRemainingGrid()
    {
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (grid[x, y] == null)
                {
                    float chance = Random.Range(0.0f, 1.0f);
                    if (chance < 0.7f)
                        grid[x, y] = new Pipes();
                }
            }
        }
    }

    private void RandomlyRotateGrid()
    {
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (grid[x, y] != null)
                {
                    int currentRotation = grid[x, y].rotations;
                    int randomRotation;

                    do
                    {
                        randomRotation = Random.Range(0, 4);
                    }
                    while (randomRotation == currentRotation);

                    grid[x, y].rotations = randomRotation;
                }
            }
        }
    }
    #endregion

    #region Pipe Placement Validation
    private bool ValidPipePlacement(Vector2Int currentCoordinate, Vector2Int previousCoordinate)
    {
        bool valid = false;
        bool connectsToPrevious = false;
        bool connectsToValidGrid = false;
        bool connectsToEndPoint = false;

        Vector2Int endCoordinate = endPoint;
        endCoordinate.x = Mathf.Clamp(endPoint.x, 0, size - 1);
        endCoordinate.y = Mathf.Clamp(endPoint.y, 0, size - 1);

        for (int i = 0; i < grid[currentCoordinate.x, currentCoordinate.y].exits.Length; i++)
        {   
            Vector2Int resultingCoordiante = currentCoordinate + grid[currentCoordinate.x, currentCoordinate.y].RotatedExit(i);
            //Debug.Log("PIPE PUZZLE MANAGER - resulting coordinates are: " + resultingCoordiante.ToString());
            //Debug.Log("PIPE PUZZLE MANAGER - necessary direction coordinates are: " + previousCoordinate.ToString());
            if(resultingCoordiante == previousCoordinate)
            {
                connectsToPrevious = true;
                //Debug.Log("PIPE PUZZLE MANAGER - the resulting pipe does connect to it's previous pipe");
            }
            else if(IsInGrid(resultingCoordiante) && IsEmptyGrid(resultingCoordiante))
            { //If this is true, the resulting coordinate should be inside the grid and NOT the previous coordinate.
                connectsToValidGrid = true;
                //Debug.Log("PIPE PUZZLE MANAGER - the resulting pipe does connect to a valid grid spot!");
            }
            else if(resultingCoordiante == endPoint)
            {
                connectsToEndPoint = true;
                //Debug.Log("PIPE PUZZLE MANAGER - the resulting pipe does connect to the end of the puzzle");
            }
        }

        if(currentCoordinate == endCoordinate) //Ensures that if we're in the final square that it actually connects to the end regardless of a valid additional square.
            valid = connectsToPrevious && connectsToEndPoint;
        else
            valid = connectsToPrevious && connectsToValidGrid;
        //Debug.Log("PIPE PUZZLE MANAGER - the resulting pipes overall validation is: " + valid);
        return valid;
    }

    //Returns a coordinate for the next pipe to be placed.
    private Vector2Int GenerateNextPipePlacement(Vector2Int currentCoordinate)
    {
        Vector2Int ret = new Vector2Int();

        List<Vector2Int> validSpots = new List<Vector2Int>();
        for(int i = 0; i < grid[currentCoordinate.x, currentCoordinate.y].exits.Length; i++)
        {
            Vector2Int resultingCoordiante = currentCoordinate + grid[currentCoordinate.x, currentCoordinate.y].RotatedExit(i);
            if(IsInGrid(resultingCoordiante) && IsEmptyGrid(resultingCoordiante))
            {
                validSpots.Add(resultingCoordiante);
                //Debug.Log("PIPE PUZZLE MANAGER - Found a valid next spot, adding to the list: " + resultingCoordiante.ToString());
            }
        }

        int randomIndex = Random.Range(0, validSpots.Count);
        ret = validSpots[randomIndex];
        //Debug.Log("PIPE PUZZLE MANAGER - Returning the next pipe spot: " + ret.ToString());
        return ret;
    }

    private bool IsInGrid(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < size &&
           coord.y >= 0 && coord.y < size;
    }

    private bool IsEmptyGrid(Vector2Int coord)
    {
        return (grid[coord.x, coord.y] == null);
    }
    #endregion
}
