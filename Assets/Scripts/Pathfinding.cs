﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour
{
    PathRequestManager requestManager;
    Grid grid;
    public int diagonalValue;
    public int straightValue;

    private void Awake()
    {
        grid = GetComponent<Grid>();
        requestManager = GetComponent<PathRequestManager>();
    }

    public void StartFindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        StartCoroutine(FindPath(startPosition, targetPosition));
    }
    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if(startNode.walkable && targetNode.walkable) { 
        Heap<Node> openSet = new Heap<Node>(grid.MaxHeapSize);
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();

                closedSet.Add(currentNode);

                //if its the target, its reached the goal
                if (currentNode == targetNode)
                {
                    sw.Stop();
                    print(sw.ElapsedMilliseconds);
                    pathSuccess = true;
                    break;
                }
                //otherwise loop through all the neighbors
                foreach (Node neighbor in grid.GetNeighbors(currentNode))
                {
                    //ignore neighbors that are either unwalkable or are already in our closed set
                    if (neighbor.walkable == false || closedSet.Contains(neighbor))
                    {
                        continue;
                    }
                    //generate movement cost to neighbor
                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor) + neighbor.movementPenalty;

                    //if distance is shorter than the neighbors or it is not in the open set
                    if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, targetNode);

                        neighbor.parent = currentNode;
                        //if not in the open set, add it to the open set
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                        //otherwise, update its value in the open set
                        else
                        {
                            openSet.UpdateItem(neighbor);
                        }
                    }
                }

            }
        }
        yield return null;
        //if a path is found, retrace the path
        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }
    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;

    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for(int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if(directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);

            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
        
    }
    public int GetDistance(Node nodeA, Node nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distanceY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if(distanceX > distanceY)
        {
            return diagonalValue * distanceY + straightValue * (distanceX - distanceY);
        }
        else
        {
            return diagonalValue * distanceX + straightValue * (distanceY - distanceX);
        }
    }
}
