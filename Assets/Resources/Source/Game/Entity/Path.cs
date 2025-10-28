using System.Linq;
using System.Collections.Generic;

using static Core;

public class Path
{
    public static Path ShortestPath(Entity forWho, int startX, int startY, int endX, int endY, bool neighboring = false)
    {
        var map = Save.save.map;
        var path = new Path
        {
            startCell = map.cells[startX, startY],
            endCell = map.cells[endX, endY],
            forWho = forWho
        };
        path.movementMap = path.FindPath(neighboring);
        return path;
    }

    //For who was this path made
    public Entity forWho;

    //Starting cell and the end cell;
    public Cell startCell, endCell;

    //Pathfinding information for the cells
    public static Dictionary<Cell, PathingNode> nodeInformation;

    //Nodes that still need to be considered
    public static List<Cell> openNodes;

    //Finds the best path between two points
    public List<Point> FindPath(bool neighboring)
    {
        //List of points the entity has to travel across
        //that will be the result of the algorithm
        List<Point> path = new();

        //Initialise the list of nodes that will serve as info for pathfinding
        //At the beginning add the starting cell
        nodeInformation = new() { { startCell, new PathingNode(startCell, endCell) } };

        //If we are already at the destination finish the search
        if (!neighboring && nodeInformation[startCell].H == 0) return new();
        else if (neighboring && nodeInformation[startCell].H == 1 && PathingNode.CanReach(startCell, endCell)) return new();

        //Initialise the list of open nodes
        openNodes = new() { startCell };

        while (openNodes.Count > 0)
        {
            var q = openNodes.Aggregate((min, current) => nodeInformation[current].F() < nodeInformation[min].F() ? current : min);
            openNodes.Remove(q);
            var newReachable = q.GetAdjacentLocations(true, false).Where(x => PathingNode.CanReach(q, x));
            var newTraversable = newReachable.Where(x => PathingNode.CanTraverse(forWho, q, x));
            foreach (var cell in newReachable)
                if (!nodeInformation.ContainsKey(cell))
                {
                    nodeInformation.Add(cell, new PathingNode(cell, endCell));
                    if ((!neighboring && nodeInformation[cell].H == 0 || neighboring && nodeInformation[cell].H == 1 && newTraversable.Contains(cell)) && PathingNode.CanReach(cell, endCell))
                    {
                        endCell = cell;
                        nodeInformation[cell].SetParentNode(nodeInformation[q]);
                        openNodes = new();
                        break;
                    }
                    if (nodeInformation[cell].F() > 300) continue;
                    if (newTraversable.Contains(cell))
                    {
                        nodeInformation[cell].SetParentNode(nodeInformation[q]);
                        openNodes.Add(cell);
                    }
                }
                else
                {
                    float traversalCost = PathingNode.AproxTraversalCost(q, cell);
                    float gTemp = nodeInformation[q].G + traversalCost;
                    if (gTemp < nodeInformation[cell].G && openNodes.Contains(cell))
                        nodeInformation[cell].SetParentNode(nodeInformation[q]);
                    else if (gTemp < nodeInformation[cell].G && nodeInformation[cell].isClosed)
                    {
                        nodeInformation[cell].SetParentNode(nodeInformation[q]);
                        openNodes.Add(cell);
                    }
                }
            nodeInformation[q].isClosed = true;
        }

        //If the end node wasn't reached, it means search was failed
        if (!nodeInformation.ContainsKey(endCell)) return null;

        //Otherwise trace the path from the end node to the starting
        //one to determine the shortest path to the destination
        Cell node = endCell;
        while (nodeInformation[node].parentNode != null)
        {
            path.Add(new Point(node.x, node.y));
            node = nodeInformation[node].parentNode.cell;
        }
        path.Reverse();

        //Return the built path
        return path;
    }

    //Path drawn for an entity to move
    public List<Point> movementMap;
}

public struct Point
{
    //Constructor
    public Point(int X, int Y) => (x, y) = (X, Y);

    //Coordinates
    public int x, y;
}
