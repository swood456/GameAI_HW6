using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class Point
{
    public int x;
    public int y;
    public float f;
    public float g;
    public float h;

    public Point parent;
    // TODO: parnets | C# doesn't like a point having a point variable :(
    public Point(int _x, int _y, float _f = 0.0f, float _g = 0.0f, float _h = 0.0f)
    {
        x = _x;
        y = _y;
        f = _f;
        g = _g;
        h = _h;
        parent = null;
    }

    // equality nonsense | maybe don't need??
    //public override bool Equals(object obj)
    //{
    //    return obj is Point && this == (Point)obj;
    //}

    //public static bool operator ==(Point a, Point b)
    //{
    //    return a.x == b.x && a.y == b.y;
    //}

    //public static bool operator !=(Point a, Point b)
    //{
    //    return !(a == b);
    //}
}

public class CreateWorldFromMap : MonoBehaviour {

    public enum HeuristicType { Euclidian, Manhattan};
    public HeuristicType m_heruistic;

    [SerializeField]
    private TextAsset input_map;

    [SerializeField]
    private GameObject void_square;

    [SerializeField]
    private GameObject tree_square;

    [SerializeField]
    private GameObject empty_square;

    [SerializeField]
    private float image_square_size;

    [SerializeField]
    private int num_squares_per_tile_x;

    [SerializeField]
    private int num_squares_per_tile_y;

    public GameObject startPoint;
    public GameObject endPoint;
    public GameObject tile_outline;
    public GameObject redDot;

    [SerializeField]
    private float tile_threshold_to_move = 0.5f;

    string type;
    int height;
    int width;
    int tile_map_width;
    int tile_map_height;
    float left_side;
    float top;
    GameObject map_root;
    float[,] map_representation;
    bool start_placed = false;
    bool end_placed = false;

    // Use this for initialization
    void Start () {
        map_root = new GameObject("Map_Root");
        CreateMap();
	}

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.E))
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

    public void print_path()
    {
        List<Vector2> l = FindPath();
        if (l == null)
            return;
        foreach (Vector2 p in l)
        {
            print(p.x + ", " + p.y);
        }
        PlayerFollowGridPath player = FindObjectOfType<PlayerFollowGridPath>();
        player.enabled = true;
        player.transform.position = startPoint.transform.position;
        player.FollowPath(l, endPoint.transform.position);
    }

    //public List<Point> FindPath()
    public List<Vector2> FindPath()
    {
        if (!start_placed || !end_placed)
            return null;

        // first, locate what square the start and end is in
        int s_i, s_j, e_i, e_j;
        
        s_i = Mathf.RoundToInt((startPoint.transform.position.x - left_side) / image_square_size);
        s_j = -Mathf.RoundToInt((startPoint.transform.position.y - top) / image_square_size);

        e_i = Mathf.RoundToInt((endPoint.transform.position.x - left_side) / image_square_size);
        e_j = -Mathf.RoundToInt((endPoint.transform.position.y - top) / image_square_size);

        if(s_i < 0 || s_i >= width || s_j < 0 || s_j >= height)
        {
            print("error: placed start tile off map");
            return null;
        }

        if (e_i < 0 || e_i >= width || e_j < 0 || e_j >= height)
        {
            print("error: placed end tile off map");
            return null;
        }

        int s_x = s_i / num_squares_per_tile_x, s_y = s_j / num_squares_per_tile_y;
        if(map_representation[s_x, s_y] > tile_threshold_to_move)
        {
            print("placed start tile on a tile deemed unmovable");
            return null;
        }

        int e_x = e_i / num_squares_per_tile_x, e_y = e_j / num_squares_per_tile_y;
        print(s_i + ", " + s_j + "|" + e_i + ", " + e_j);
        print(s_x + ", " + s_y + "|" + e_x + ", " + e_y);
        if (map_representation[e_x, e_y] > tile_threshold_to_move)
        {
            print("placed end tile on a tile deemed unmovable");
            return null;
        }

        // now actually do a*
        List<Point> open_points = new List<Point>();
        open_points.Add(new Point(s_x, s_y));

        List<Point> close_points = new List<Point>();

        while(open_points.Count > 0)
        {
            // consider the best node in the open list
            Point best_point = FindSmallestInList(open_points);

            // if this node is the goal
            if(best_point.x == e_x && best_point.y == e_y)
            {
                List<Vector2> path = new List<Vector2>();

                Point p = best_point;

                while(p != null)
                {
                    float fx = left_side + p.x * num_squares_per_tile_x * image_square_size;
                    float fy = top - p.y * num_squares_per_tile_y * image_square_size;
                    path.Insert(0, new Vector2(fx, fy));
                    //path.Insert(0, new Vector2(p.x, p.y));
                    p = p.parent;
                }

                return path;
            }

            // else move the current node to the closest list and conisder all of its neighbors
            open_points.Remove(best_point);

            List<Point> neighbors = GetAdjacentOpenTiles(best_point.x, best_point.y);
            foreach(Point p in neighbors)
            {
                
                Point successor = p;
                successor.g = best_point.g + 1;
                successor.h = Heuristic(successor.x, successor.y, e_x, e_y);
                successor.f = successor.g + successor.h;
                successor.parent = best_point;

                //MIT psuedocode
                /*
                Point same = FindXYInPointList(open_points, successor.x, successor.y);
                if(same != null)
                {
                    //if a node with the same position as successor is in the OPEN list \
                    //which has a lower f than successor, skip this successor
                    if(same.f < successor.f)
                    {
                        continue;
                    }
                    
                }
                //if a node with the same position as successor is in the CLOSED list \ 
                //    which has a lower f than successor, skip this successor
                same = FindXYInPointList(close_points, successor.x, successor.y);
                if (same != null && same.f < successor.f)
                {
                    continue;
                }
                
                //otherwise, add the node to the open list
                open_points.Add(successor);
                */

                //PROF SLIDE PSUEDOCODE:
                //if this neighbor is in the closed list and our current g value is lower
                // update the neighbor with the new, lower g value
                // change the neighbor's parent to our current node
                Point same_close = FindXYInPointList(close_points, successor.x, successor.y);
                if(same_close != null)
                {
                    if(successor.g < same_close.g)
                    {
                        same_close.g = successor.g;
                        same_close.parent = best_point;
                        continue;
                    }
                }
                // else if this neighbor is in the open list and our current g value is lower
                // update the neighbor with the new, lower g value
                // change the neighbor's parent to our current node
                Point same_open = FindXYInPointList(open_points, successor.x, successor.y);
                if (same_open != null){
                    if(successor.g < same_open.g)
                    {
                        same_open.g = successor.g;
                        same_open.parent = best_point;
                        continue;
                    }
                }

                // else the neighbor is not in either the open or closes list
                // add the neighbor to the open list and set its g value
                // making us the parent
                if(same_close == null && same_open == null)
                {
                    open_points.Add(successor);
                    float x = left_side + successor.x * num_squares_per_tile_x * image_square_size;
                    float y = top - successor.y * num_squares_per_tile_y * image_square_size;
                    Instantiate(redDot, new Vector3(x, y, -2), Quaternion.identity);
                }
            }

            close_points.Add(best_point);


        }

        print("could not find path :(");
        return null;
    }

    private Point FindXYInPointList(List<Point> l, int x, int y)
    {
        foreach(Point p in l)
        {
            if(p.x == x && p.y == y)
            {
                return p;
            }
        }

        return null;
    }

    private Point FindSmallestInList(List<Point> l)
    {
        // never will have an empty list
        Point smallest_point = new Point(0,0);
        float smallest_cost = float.MaxValue;
        foreach(Point p in l)
        {
            if(p.f < smallest_cost)
            {
                smallest_point = p;
                smallest_cost = p.f;
            }
        }
        return smallest_point;
    }

    private List<Point> GetAdjacentOpenTiles(int x, int y)
    {
        List<Point> adjacent = new List<Point>();

        // left
        if(x > 0 && map_representation[x-1, y] < tile_threshold_to_move)
        {
            adjacent.Add(new Point(x-1,y));
        }

        // right
        if (x < width - 1 && map_representation[x + 1, y] < tile_threshold_to_move)
        {
            adjacent.Add(new Point(x + 1, y));
        }

        // up
        if (y > 0 && map_representation[x, y-1] < tile_threshold_to_move)
        {
            adjacent.Add(new Point(x, y - 1));
        }

        // down
        if (y < height-1 && map_representation[x, y + 1] < tile_threshold_to_move)
        {
            adjacent.Add(new Point(x, y + 1));
        }
        return adjacent;
    }

    private float Heuristic(int x, int y, int endX, int endY)
    {
        switch (m_heruistic)
        {
            case HeuristicType.Euclidian:
                return HeuristicEuclidianDist(x, y, endX, endY);
            case HeuristicType.Manhattan:
                return HeuristicManhattanDist(x, y, endX, endY);
        }
        // in case something broke?
        return 0.0f;
    }

    private float HeuristicEuclidianDist(int x, int y, int endX, int endY)
    {
        return Mathf.Sqrt((x - endX) * (x - endX) + (y - endY) * (y - endY));
    }

    private float HeuristicManhattanDist(int x, int y, int endX, int endY)
    {
        return Mathf.Abs(endX - x) + Mathf.Abs(endY - y);
    }

    private void CreateMap()
    {
        string[] lines = input_map.text.Split('\n');

        type = lines[0].Split(' ')[1];
        height = int.Parse(lines[1].Split(' ')[1]);
        width = int.Parse(lines[2].Split(' ')[1]);

        tile_map_width = width % num_squares_per_tile_x == 0 ? width / num_squares_per_tile_x : width / num_squares_per_tile_x + 1;
        tile_map_height = height % num_squares_per_tile_y == 0 ? height / num_squares_per_tile_y : height / num_squares_per_tile_y + 1;

        map_representation = new float[tile_map_width, tile_map_height];
        int tile_area = num_squares_per_tile_x * num_squares_per_tile_y;

        left_side = -width * image_square_size / 2;
        top = height * image_square_size / 2;
        for (int j = 4; j < lines.Length; ++j)
        {
            for(int i = 0; i < width; ++i )
            {
                
                GameObject to_spawn;
                int m_x = i / num_squares_per_tile_x, m_y = (j-4) / num_squares_per_tile_y;
                switch(lines[j][i])
                {
                    case '@':
                        to_spawn = void_square;
                        map_representation[m_x, m_y] += 1.0f / tile_area;
                        break;
                    case 'T':
                        to_spawn = tree_square;
                        map_representation[m_x, m_y] += 1.0f / tile_area;
                        break;
                    default:
                        to_spawn = empty_square;
                        break;
                }

                Instantiate(to_spawn, new Vector3(left_side + i * image_square_size, top - (j-4) * image_square_size, 0), Quaternion.identity, map_root.transform);
            }
        }

        for(int j = 0; j < tile_map_height; ++j)
        {
            for(int i = 0; i < tile_map_width; ++i)
            {
                float x = left_side + i * num_squares_per_tile_x * image_square_size;
                float y = top - j * num_squares_per_tile_y * image_square_size;
                Instantiate(tile_outline, new Vector2(x,y), Quaternion.identity);
            }
        }

        //for(int j = 0; j < tile_map_width; ++j)
        //{
        //    for(int i = 0; i < tile_map_height; ++i)
        //    {
        //        print(map_representation[i,j]);
        //    }
        //}
    }
}
