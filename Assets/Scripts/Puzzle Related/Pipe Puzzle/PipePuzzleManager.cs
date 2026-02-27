using System.Collections.Generic;
using UnityEngine;

public class PipePuzzleManager : MonoBehaviour
{
    #region Serial Variables
    [Tooltip("How big the grid is; size = 3 then there will be 9 spaces as a 3x3 grid")]
    [SerializeField] private int size;
    [Tooltip("There is no implementation as of this current moment for a custom puzzle, leave this false")]
    [SerializeField] private bool isCustomPuzzle = false; //Default Assumption is the puzzle is computer generated
    #endregion

    #region Non-Serial Variables
    private Vector2Int startPoint; //The i/j for the start position, this should be offset by 1 to be outside of the strict limits. Same applies for the end points
    private Vector2Int endPoint;

    private bool isVertical;

    private Pipes[,] grid;
    #endregion


    private void Start()
    {

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
        if (isVertical)
        {
            startPoint = new Vector2Int(0 - 1, Random.Range(0, size));
            endPoint = new Vector2Int(size, Random.Range(0, size));
        }
        else
        {
            startPoint = new Vector2Int(Random.Range(0, size), 0 - 1);
            endPoint = new Vector2Int(Random.Range(0, size), size);
        }

        Debug.Log("PIPE PUZZLE MANAGER - Start Point Coordinate: " + startPoint.ToString() + "; End Point Coordinate: " + endPoint.ToString());

        PlaceSolution();
    }

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
        const int maxAttempts = 50;

        bool done = false;
        do
        {
            do
            {
                Debug.Log("PIPE PUZZLE MANAGER - Attempting to place a pipe at location: " + currentCoordinate.ToString());
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
            Debug.Log("PIPE PUZZLE MANAGER - Successfully Placed a pipe at location: " + currentCoordinate.ToString());
            if (currentCoordinate == endCoordinate)
            {
                done = true;
                break;
            }
            previousCoordinate = currentCoordinate;
            currentCoordinate = GenerateNextPipePlacement(currentCoordinate);
        } while (!done);

        string print = "";

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (grid[i, j] != null)
                    print += grid[i, j].tempRepresentation + "|";
                else
                    print += " " + "|";
            }
            print += "\n";
            for(int j = 0; j < size; j++)
            {
                print += "-";
            }
            print += "\n";
        }
        Debug.Log("PIPES \n" + print);

    }

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
            Debug.Log("PIPE PUZZLE MANAGER - resulting coordinates are: " + resultingCoordiante.ToString());
            Debug.Log("PIPE PUZZLE MANAGER - necessary direction coordinates are: " + previousCoordinate.ToString());
            if(resultingCoordiante == previousCoordinate)
            {
                connectsToPrevious = true;
                Debug.Log("PIPE PUZZLE MANAGER - the resulting pipe does connect to it's previous pipe");
            }
            else if(IsInGrid(resultingCoordiante) && IsEmptyGrid(resultingCoordiante))
            { //If this is true, the resulting coordinate should be inside the grid and NOT the previous coordinate.
                connectsToValidGrid = true;
                Debug.Log("PIPE PUZZLE MANAGER - the resulting pipe does connect to a valid grid spot!");
            }
            else if(resultingCoordiante == endPoint)
            {
                connectsToEndPoint = true;
                Debug.Log("PIPE PUZZLE MANAGER - the resulting pipe does connect to the end of the puzzle");
            }
        }

        if(currentCoordinate == endCoordinate) //Ensures that if we're in the final square that it actually connects to the end regardless of a valid additional square.
            valid = connectsToPrevious && connectsToEndPoint;
        else
            valid = connectsToPrevious && connectsToValidGrid;
        Debug.Log("PIPE PUZZLE MANAGER - the resulting pipes overall validation is: " + valid);
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
                Debug.Log("PIPE PUZZLE MANAGER - Found a valid next spot, adding to the list: " + resultingCoordiante.ToString());
            }
        }

        int randomIndex = Random.Range(0, validSpots.Count);
        ret = validSpots[randomIndex];
        Debug.Log("PIPE PUZZLE MANAGER - Returning the next pipe spot: " + ret.ToString());
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
