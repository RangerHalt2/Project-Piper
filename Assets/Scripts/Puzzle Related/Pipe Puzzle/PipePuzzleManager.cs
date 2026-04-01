using System.Collections.Generic;
using System.Linq;
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


    private List<Pipes> possiblePipes = new List<Pipes>();

    private bool recursionAborted = false;
    private int recursionCount = 0;
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

        //PrintMiniGrid(miniGrid);


        //InitializeGrid();
        //InitializePuzzle();
    }

    public void GenerateNewPuzzle()
    {
        GeneratePossiblePipes();
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
        if (!recursionAborted)
        {
            PopulateRemainingGrid();
            int maxAttempts = 100;
            int attempts = 0;
            while (CheckPuzzleIsSolved())
            {
                if (attempts >= maxAttempts) break;
                //Debug.Log("PIPE PUZZLE MANAGER - The puzzle is solved!");
                RandomlyRotateGrid();
                attempts++;
            }

            Debug.Log("PIPE PUZZLE MANAGER - Attempts used was: " + attempts);
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

        ret = RecursiveNavigation(currentCoordinate, startPoint, new HashSet<Vector2Int>());

        return ret;
    }

    //Helper function to the CheckPuzzleIsSolved, recursively navigates through the grid confirming the pipe layout.
    /*
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
    }*/

    private bool RecursiveNavigation(Vector2Int coords, Vector2Int cameFrom, HashSet<Vector2Int> visited)
    {
        if (!IsInGrid(coords))
            return coords == endPoint;
        if (grid[coords.x, coords.y] == null)
            return false;
        if (visited.Contains(coords))
            return false;


        // Ensure this pipe actually connects back to where we came from
        bool connectsToCameFrom = false;
        for (int i = 0; i < grid[coords.x, coords.y].exits.Length; i++)
        {
            Vector2Int exit = coords + grid[coords.x, coords.y].RotatedExit(i);
            if (exit == cameFrom)
            {
                connectsToCameFrom = true;
                break;
            }
        }
        if (!connectsToCameFrom) return false;

        visited.Add(coords);
        for (int i = 0; i < grid[coords.x, coords.y].exits.Length; i++)
        {
            Vector2Int next = coords + grid[coords.x, coords.y].RotatedExit(i);
            if (next == cameFrom) continue; // don't go back where we came from
            if (RecursiveNavigation(next, coords, visited))
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
        recursionCount = 0;
        bool done = RecursivePlaceSolution(currentCoordinate, previousCoordinate);
    }

    // Recursive Place Solution will attempt to place a pipe at the current coordinates, and check with the previous coordinate to ensure they're matching.
    // For this, the recursive placement needs to check and validate each valid pipe placement. There are 4 pipes with 4 rotations each for a total of 16?
    private bool RecursivePlaceSolution(Vector2Int currentCoordinate, Vector2Int previousCoordinates)
    {
        if (recursionAborted) return false;
        recursionCount++;
        if (recursionCount >= 100000)
        {
            Debug.LogError("PIPE PUZZLE MANAGER - Recursion limit hit, aborting");
            recursionAborted = true;
            return false;
        }
        if (currentCoordinate == endPoint) return true; // Base Case, if the puzzle is completed return true.
        if (!IsInGrid(currentCoordinate) || !IsEmptyGrid(currentCoordinate)) return false;

        List<Pipes> validPlacements = new List<Pipes>();
        foreach (Pipes pipe in possiblePipes)
        {
            grid[currentCoordinate.x, currentCoordinate.y] = new Pipes(pipe.type, pipe.rotations);
            if (ValidPipePlacement(currentCoordinate, previousCoordinates))
            {
                validPlacements.Add(grid[currentCoordinate.x, currentCoordinate.y]);
            }
            grid[currentCoordinate.x, currentCoordinate.y] = null;
        }

        // At this point in the code you've collected all the possible valid placements of pipes, this includes pipes that touch the ends or empty grid spaces

        while(validPlacements.Count > 0)
        {
            int random = Random.Range(0, validPlacements.Count);
            Pipes pipe = validPlacements[random];
            grid[currentCoordinate.x, currentCoordinate.y] = pipe;

            bool done = false;
            //Loop through all the possible pipe exits?
            for(int i = 0; i < grid[currentCoordinate.x, currentCoordinate.y].exits.Length; i++)
            {
                Vector2Int resultingCoordiante = currentCoordinate + grid[currentCoordinate.x, currentCoordinate.y].RotatedExit(i);
                done = RecursivePlaceSolution(resultingCoordiante, currentCoordinate);
                if (done) return done;
            }
            //grid[currentCoordinate.x, currentCoordinate.y] = null;
            validPlacements.Remove(pipe);
        }

        return false; // If it reaches this point, there were no valid pipe placements
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
    // Refactor and place possible pipes here, no need to remake them.
    private void GeneratePossiblePipes()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                switch (i)
                {
                    case 0:
                        possiblePipes.Add(new Pipes(PipeType.right_angle, j));
                        break;

                    case 1:
                        possiblePipes.Add(new Pipes(PipeType.cross, j));
                        break;

                    case 2:
                        possiblePipes.Add(new Pipes(PipeType.t_pipe, j));
                        break;

                    case 3:
                        possiblePipes.Add(new Pipes(PipeType.straight, j));
                        break;
                    default:
                        //Debug.LogError("PIPE PUZZLE MANAGER - Impossible value of i was passed in");
                        return;
                }
            }
        } // End of generating all possible pipes
    }

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
