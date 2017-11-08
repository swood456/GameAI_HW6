using System.Collections;
using System.Collections.Generic;
using UnityEngine;
class wPoint
{
    public Waypoint waypoint;
    public float f;
    public float g;
    public float h;
    public wPoint parent;

    public wPoint(Waypoint wp, float _f = 0.0f, float _g = 0.0f, float _h = 0.0f)
    {
        waypoint = wp;
        f = _f;
        g = _g;
        h = _h;
    }
}
public class FindPathWaypoint : MonoBehaviour {

    public enum HeuristicType { Euclidian, Manhattan };
    public HeuristicType m_heruistic;

    public GameObject startPoint;
    public GameObject endPoint;

    public float heuristic_weight = 1.0f;

    bool start_placed = false;
    bool end_placed = false;


    private void Update()
    {
        // set start and end points
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.E))
        {
            Camera c = Camera.main;
            Vector2 mouse_pos_world;
            mouse_pos_world.x = Input.mousePosition.x;
            mouse_pos_world.y = Input.mousePosition.y;

            Vector3 f_pos = c.ScreenToWorldPoint(new Vector3(mouse_pos_world.x, mouse_pos_world.y, c.nearClipPlane));
            f_pos.z = -1.0f;

            // TODO: make sure the space is within the map

            if (Input.GetKeyDown(KeyCode.S))
            {
                startPoint.transform.position = f_pos;
                start_placed = true;
            }
            else
            {
                endPoint.transform.position = f_pos;
                end_placed = true;
            }
        }
    }

    public void FindAndGivePath()
    {
        List<Vector2> l = FindPath();

        if (l == null)
            return;

        foreach(Vector2 v in l)
        {
            print(v);
        }

        // Give the player object a path that it will follow
        PlayerFollowGridPath player = FindObjectOfType<PlayerFollowGridPath>();

        // set up the player to move around
        player.enabled = true;
        player.transform.position = startPoint.transform.position;
        player.FollowPath(l, endPoint.transform.position);
    }

    public List<Vector2> FindPath()
    {
        if (!start_placed || !end_placed)
            return null;

        Waypoint[] all_waypoints = FindObjectsOfType<Waypoint>();

        Waypoint first = null;
        Waypoint end = null;
        float closest_dist = float.MaxValue;
        float end_dist = float.MaxValue;

        foreach(Waypoint w in all_waypoints)
        {
            float dist = Vector2.Distance(w.transform.position, startPoint.transform.position);
            if(dist < closest_dist)
            {
                closest_dist = dist;
                first = w;
            }
            dist = Vector2.Distance(w.transform.position, endPoint.transform.position);
            if(dist < end_dist)
            {
                end_dist = dist;
                end = w;
            }
        }

        print("start: " + first);
        print("end: " + end);

        // Do A*
        List<wPoint> open = new List<wPoint>();
        open.Add(new wPoint(first));

        List<wPoint> close = new List<wPoint>();

        while(open.Count > 0)
        {
            // Consider the point with the smallest f value
            wPoint best_point = FindSmallestInList(open);

            //if(best_point.x - end.transform.position.x < float.Epsilon && best_point.y - end.transform.position.y < float.Epsilon)
            if(Vector2.Distance(best_point.waypoint.transform.position, end.transform.position) < float.Epsilon)
            {
                // found the end
                print("reached the end, found a path");
                List<Vector2> path = new List<Vector2>();

                wPoint p = best_point;

                while (p != null)
                {
                    // equations to get the x and y pos in worldspace for our tiles.
                    
                    // push front of list
                    path.Insert(0, new Vector2(p.waypoint.transform.position.x, p.waypoint.transform.position.y));

                    // go to next node
                    p = p.parent;
                }

                return path;
            }

            open.Remove(best_point);
            foreach(Waypoint w in best_point.waypoint.neighbors)
            {
                wPoint cur_point = new wPoint(w);
                cur_point.g = best_point.g + Vector2.Distance(best_point.waypoint.transform.position, w.transform.position);
                cur_point.h = Heuristic(w.transform.position.x, w.transform.position.y, end.transform.position.x, end.transform.position.y);
                cur_point.f = cur_point.g + cur_point.h;
                cur_point.parent = best_point;

                //PROF SLIDE PSUEDOCODE:
                //if this neighbor is in the closed list and our current g value is lower
                // update the neighbor with the new, lower g value
                // change the neighbor's parent to our current node

                // See if there is a point in the close_points list with the same xy
                wPoint same_close = FindXYInPointList(close, cur_point.waypoint.transform.position);
                // if there is and it has a higher g than the sucessor node we created
                if (same_close != null)
                {
                    if (cur_point.g < same_close.g)
                    {
                        // update that node we found's g value
                        same_close.g = cur_point.g;

                        // update its parent
                        same_close.parent = best_point;
                        // don't do the other steps
                        continue;
                    }
                }
                // else if this neighbor is in the open list and our current g value is lower
                // update the neighbor with the new, lower g value
                // change the neighbor's parent to our current node
                wPoint same_open = FindXYInPointList(open, cur_point.waypoint.transform.position);
                if (same_open != null)
                {
                    if (cur_point.g < same_open.g)
                    {
                        same_open.g = cur_point.g;
                        same_open.parent = best_point;
                        continue;
                    }
                }

                // else the neighbor is not in either the open or closes list
                // add the neighbor to the open list and set its g value
                // making us the parent
                if (same_close == null && same_open == null)
                {
                    open.Add(cur_point);
                }
            }
            close.Add(best_point);
        }
        print("could not find path :(");
        return null;
    }

    wPoint FindXYInPointList(List<wPoint> l, Vector3 pos)
    {
        foreach(wPoint wp in l)
        {
            if (Vector2.Distance(wp.waypoint.transform.position, pos) < float.Epsilon)
                return wp;
        }
        return null;
    }

    // function that calculates the heuristic value for a tile at x,y given end at endX, endY
    //  this uses the m_heuristic variable to determine which function to use
    private float Heuristic(float x, float y, float endX, float endY)
    {
        // note: here is where we multiply by the weight, which can be set in the GUI
        switch (m_heruistic)
        {
            case HeuristicType.Euclidian:
                return heuristic_weight * HeuristicEuclidianDist(x, y, endX, endY);
            case HeuristicType.Manhattan:
                return heuristic_weight * HeuristicManhattanDist(x, y, endX, endY);
        }
        // in case something broke?
        return 0.0f;
    }

    // simple distance formula
    private float HeuristicEuclidianDist(float x, float y, float endX, float endY)
    {
        return Mathf.Sqrt((x - endX) * (x - endX) + (y - endY) * (y - endY));
    }

    // equation taken from slides
    private float HeuristicManhattanDist(float x, float y, float endX, float endY)
    {
        return Mathf.Abs(endX - x) + Mathf.Abs(endY - y);
    }

    private wPoint FindSmallestInList(List<wPoint> l)
    {
        // never will have an empty list
        wPoint smallest_point = null;
        float smallest_cost = float.MaxValue;
        foreach (wPoint p in l)
        {
            if (p.f < smallest_cost)
            {
                smallest_point = p;
                smallest_cost = p.f;
            }
        }
        return smallest_point;
    }
}
