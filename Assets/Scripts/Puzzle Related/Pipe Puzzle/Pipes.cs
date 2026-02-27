using UnityEngine;

public enum PipeType
{
    right_angle,
    t_pipe,
    straight,
    cross
}

public class Pipes
{
    public PipeType type { get; private set; }
    public string tempRepresentation { get; private set; }
    public int rotations { get; set; } = 0;
    public Vector2Int[] exits { get; private set; } // Exits stores an (X, Y) displacement integer, (0, 1) means go up 1 stay in the same column. This is inverted by i and j.
                                                    // Num of Exits is the exits.Length() or exits.Count

    //Constructor, determines the base pipe information
    public Pipes()
    {
        PipeType pipe_template = (PipeType)Random.Range(0, 4); //Generates 0-3
        switch (pipe_template)
        {
            case PipeType.right_angle:
                type = PipeType.right_angle;
                tempRepresentation = "R";
                exits = new Vector2Int[]
                {
                    new Vector2Int(0, -1),
                    new Vector2Int(1, 0)
                };
                break;
            case PipeType.t_pipe:
                type = PipeType.t_pipe;
                tempRepresentation = "T";
                exits = new Vector2Int[]
                {
                    new Vector2Int(0, -1),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1)
                };
                break;
            case PipeType.straight:
                type = PipeType.straight;
                tempRepresentation = "S";
                exits = new Vector2Int[]
                {
                    new Vector2Int(-1, 0),
                    new Vector2Int(1, 0)
                };
                break;
            case PipeType.cross:
                type = PipeType.cross;
                tempRepresentation = "+";
                exits = new Vector2Int[]
                {
                    new Vector2Int(0, -1),
                    new Vector2Int(-1, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1)
                };
                break;
            default:
                Debug.LogError("PIPES - The pipe template was not a valid matching enum or template");
                break;
        }
        rotations = Random.Range(0, 3); // chooses an 
        tempRepresentation += rotations.ToString();
        Debug.Log("PIPES - the pipe generated is: " + tempRepresentation);
    }

    public string CurrRepresentation()
    {
        string representation = "";
        switch (type)
        {
            case PipeType.right_angle:
                representation += "R";
                break;
            case PipeType.t_pipe:
                representation += "T";
                break;
            case PipeType.straight:
                representation += "S";
                break;
            case PipeType.cross:
                representation += "+";
                break;
            default:
                representation += "D";
                Debug.LogError("PIPES - type is not a valid enum type");
                break;
        }

        representation += rotations.ToString();

        return representation;
    }

    public Vector2Int RotatedExit(int exitIndex)
    {
        Vector2Int ret = new Vector2Int();

        int old_x = exits[exitIndex].x;
        int old_y = exits[exitIndex].y;

        int new_x = old_x;
        int new_y = old_y;
        for(int i = 0; i < rotations % 4; i++)
        {
            Debug.Log("PIPES - current x/y " + new_x + "/" + new_y);
            int temp = new_x;
            new_x = new_y;
            new_y = -temp;
            Debug.Log("PIPES - new x/y " + new_x + "/" + new_y);
        }

        ret.Set(new_x, new_y);
        return ret;
    }

}
