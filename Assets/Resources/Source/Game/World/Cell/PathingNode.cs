using UnityEngine;

public class PathingNode
{
    public PathingNode(Cell cell, Cell end)
    {
        this.cell = cell;
        H = AproxTraversalCost(this.cell, end);
        G = 0;
    }

    //Parent node in the pathfinding
    //Means the one that is currently thought
    //of as the previous node in the best path
    public PathingNode parentNode;

    //Cell that this node points at
    public Cell cell;

    //Real distance that this node
    public float G;

    //Heuristic used to calculate aproximate distance
    //from this node to the end node
    public float H;

    //State of this node in the calculation
    public bool isClosed;

    //Full distance predicted based on current distance and heuristic
    public float F() => G + H;

    //Set a new parent node for this node
    public void SetParentNode(PathingNode newParent)
    {
        parentNode = newParent;
        G = parentNode.G + AproxTraversalCost(cell, parentNode.cell);
    }

    //Tells the pathfinder the aproximate distance to a cell
    public static float AproxTraversalCost(Cell location, Cell otherLocation)
    {
        float deltaX = otherLocation.x - location.x;
        float deltaY = otherLocation.y - location.y;
        return (float)Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    //Tells the pathfinder whether a cell can traversed into from a different one
    public static bool CanTraverse(Entity entity, Cell location, Cell otherLocation)
    {
        if (otherLocation.IsWalkable() && otherLocation.WillFit(entity)) return true;
        return false;
    }

    //Tells the pathfinder whether a cell can be reached from a different one
    public static bool CanReach(Cell location, Cell otherLocation)
    {
        if (location == otherLocation) return true;
        return true;
    }
}