using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRep : MonoBehaviour {

    //base map info
    int mapHeight, mapWidth;

    //[SerializeField]
    public GameObject genericTile;
    GameObject[,] wholeMap;

    [Header("Initialization Info")]
    public TextAsset input_map;

    [Header("Execution Variables")]
    public int tileSize;

    public GameObject startPoint;
    public GameObject endPoint;

    bool start_placed = false;
    bool end_placed = false;

    float heuristic_weight;

    //Compressed map info
    //int Height, Width;
    //GameObject[,] world;

    // Use this for initialization
    void Start () {
        GetMap();
        //CreateWorldRep();
	}

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

    // create a representation of all map spaces
    private void GetMap()
    {
        // split the input text into lines
        string[] lines = input_map.text.Split('\n');

        // type I think is useless?
        //type = lines[0].Split(' ')[1];

        // get the height and width of the map
        mapHeight = int.Parse(lines[1].Split(' ')[1]);
        mapWidth = int.Parse(lines[2].Split(' ')[1]);

        // make a float 2d array to store the % of each tile that is covered
        wholeMap = new GameObject[mapWidth, mapHeight];

        // go through all the stuff in the input file
        for (int j = 4; j < lines.Length; ++j)
        {
            for (int i = 0; i < mapWidth; ++i)
            {
                // determine which of the tiles we are making

                wholeMap[i, j-4] = Instantiate(genericTile, new Vector3(i, j-4, 0), Quaternion.identity);
                switch (lines[j][i])
                {
                    case '@':
                        wholeMap[i, j - 4].GetComponent<TileInfo>().CreateBlocked(i, j-4);
                        break;
                    case 'T':
                        wholeMap[i, j - 4].GetComponent<TileInfo>().CreateBlocked(i, j - 4);
                        break;
                    default:
                        wholeMap[i, j - 4].GetComponent<TileInfo>().CreateEmpty(i, j - 4);
                        break;
                }
                
            }
        }
    }

    //void CreateWorldRep()
    //{
    //    Width = mapWidth % tileSize == 0 ? mapWidth / tileSize: mapWidth / tileSize + 1;
    //    Height = mapHeight % tileSize == 0 ? mapHeight / tileSize : mapHeight / tileSize + 1;

    //    world = new GameObject[Width, Height];
    //    for (int i = 0; i < Width; i++)
    //    {
    //        for (int j = 0; j < Height; j++)
    //        {
    //            world[i,j] = Instantiate(genericTile, new Vector3(i, j - 4, 0), Quaternion.identity);
    //            world[i, j].GetComponent<TileInfo>().CreateFromChildren(i, j, GetChildren(tileSize, i, j));
    //        }
    //    }
    //}

    TileInfo[] GetChildren(int size, int x, int y)
    {
        TileInfo[] children = new TileInfo[size*size];
        int xStart = size * x;
        int yStart = size * y;

        for (int k = 0; k < tileSize; k++)
        {
            int thisx = xStart + k;
            for (int l = 0; l < tileSize; l++)
            {
                int thisy = yStart + l;
                int childnum = k * size + l;

                // if the child is in wholeMap, it is added
                if (thisx < mapWidth && thisy < mapHeight){
                    children[childnum] = wholeMap[thisx, thisy].GetComponent<TileInfo>();
                }
                else
                {
                    //if no in wholeMap, assume child is impassable
                    TileInfo blockedTile = new TileInfo();
                    blockedTile.CreateBlocked(thisx, thisy);
                    children[childnum] = blockedTile;
                }
            }
        }
        return children;
    }

    // Function that is run when you press the "Find Path" button
    public void print_path()
    {
        List<Vector2> l = FindPath();

        // could not reach the end for some reason
        if (l == null)
            return;

        // Give the player object a path that it will follow
        PlayerFollowGridPath player = FindObjectOfType<PlayerFollowGridPath>();

        // set up the player to move around
        player.enabled = true;
        player.transform.position = startPoint.transform.position;
        player.FollowPath(l, endPoint.transform.position);
    }

    public List<Vector2> FindPath()
    {
        // make sure that the start and end points are located
        if (!start_placed || !end_placed)
            return null;

        // Determine the square that the start and end points
        int s_i, s_j, e_i, e_j;

        s_i = Mathf.RoundToInt(startPoint.transform.position.x);
        s_j = Mathf.RoundToInt(startPoint.transform.position.y);

        e_i = Mathf.RoundToInt(endPoint.transform.position.x);
        e_j = Mathf.RoundToInt(endPoint.transform.position.y);

        // error if the start or end is outside of the square map
        if (s_i < 0 || s_i >= mapWidth || s_j < 0 || s_j >= mapHeight)
        {
            print("error: placed start tile off map");
            return null;
        }

        if (e_i < 0 || e_i >= mapWidth || e_j < 0 || e_j >= mapHeight)
        {
            print("error: placed end tile off map");
            return null;
        }
        int e_x = e_i;
        int e_y = e_j;

        //// find the tile that the start space is in
        //int s_x = s_i / num_squares_per_tile_x, s_y = s_j / num_squares_per_tile_y;



        //if (map_representation[s_x, s_y] > tile_threshold_to_move)
        //{
        //    print("placed start tile on a tile deemed unmovable");
        //    return null;
        //}

        //// find the tile that the end space is in
        //int e_x = e_i / num_squares_per_tile_x, e_y = e_j / num_squares_per_tile_y;
        //if (map_representation[e_x, e_y] > tile_threshold_to_move)
        //{
        //    print("placed end tile on a tile deemed unmovable");
        //    return null;
        //}

        //run A*

        //create the open points list and add the start
        PriorityQueue order = new PriorityQueue();
        order.Add(0, new Vector2(s_i, s_j));

        // create the empty closed points list
        //List<Point> close_points = new List<Point>();

        while (order.size > 0)
        {
            // Consider the point with the smallest f value
            PriorityPair p_next = order.Next();
            Vector2 best_ = p_next.loc;

            // if this node is the goal, make out path and stop
            if (best_.x == e_x && best_.y == e_y)
            {
                // make a list of vector2s to store the path
                List<Vector2> path = new List<Vector2>();

                // go through all the points and push them to the front of the list, then go to their parent
                Vector2 p = best_;
                while (p != null)
                {
                    // equations to get the x and y pos in worldspace for our tiles.
                    //float x = left_side + (num_squares_per_tile_x - (1.0f + num_squares_per_tile_x) / 2) * image_square_size + p.x * num_squares_per_tile_x * image_square_size;
                    //float y = top - (num_squares_per_tile_y - (1.0f + num_squares_per_tile_y) / 2) * image_square_size - p.y * num_squares_per_tile_y * image_square_size;

                    // push front of list
                    path.Insert(0, p);

                    // go to next node
                    p = wholeMap[(int)p.x, (int)p.y].GetComponent<TileInfo>().prev;
                }

                return path;
            }

            // else move the current node to the closed list and conisder all of its neighbors

            // remove this tile from the open list. After, add it to closed list
            //open_points.Remove(best_point);

            // get all the valid neighbors for this tile that we are looking at
            List<Vector2> neighbors = GetAdjacentOpenTiles(best_);
            foreach (Vector2 n in neighbors)
            {
                TileInfo i = wholeMap[(int)n.x, (int)n.y].GetComponent<TileInfo>();
                float f, h, g;
                g = i.cost + 1;
                h = Heuristic((int)n.x, (int)n.y, e_x, e_y);
                f = h + g;

                if (i.searched)
                {
                    if (i.cost > g)
                    {
                        i.prev = best_;
                        i.cost = g;
                        i.searched = true;
                        order.Add(f, n);
                    }

                    //don't add to the order queue, there is a quicker way to that tile
                }
                else
                {
                    
                    i.prev = best_;
                    i.cost = g;
                    i.searched = true;
                    order.Add(f, n);
                }
                // make a new Point object for the successor
                //successor.g = best_.g + 1;
                //successor.h = Heuristic(successor.x, successor.y, e_x, e_y);
                //successor.f = successor.g + successor.h;
                //successor.parent = best_;

                //PROF SLIDE PSUEDOCODE:
                //if this neighbor is in the closed list and our current g value is lower
                // update the neighbor with the new, lower g value
                // change the neighbor's parent to our current node

                // See if there is a point in the close_points list with the same xy
                //Point same_close = FindXYInPointList(close_points, successor.x, successor.y);
                //// if there is and it has a higher g than the sucessor node we created
                //if (same_close != null)
                //{
                //    if (successor.g < same_close.g)
                //    {
                //        // update that node we found's g value
                //        same_close.g = successor.g;

                //        // update its parent
                //        same_close.parent = best_point;
                //        // don't do the other steps
                //        continue;
                //    }
                //}
                // else if this neighbor is in the open list and our current g value is lower
                // update the neighbor with the new, lower g value
                // change the neighbor's parent to our current node
                //Point same_open = FindXYInPointList(open_points, successor.x, successor.y);
                //if (same_open != null)
                //{
                //    if (successor.g < same_open.g)
                //    {
                //        same_open.g = successor.g;
                //        same_open.parent = best_point;
                //        continue;
                //    }
                //}

                // else the neighbor is not in either the open or closes list
                // add the neighbor to the open list and set its g value
                // making us the parent
                //if (same_close == null && same_open == null)
                //{
                //    open_points.Add(successor);
                //    // equations to get the x and y pos in worldspace for our tiles.
                //    float x = left_side + (num_squares_per_tile_x - (1.0f + num_squares_per_tile_x) / 2) * image_square_size + successor.x * num_squares_per_tile_x * image_square_size;
                //    float y = top - (num_squares_per_tile_y - (1.0f + num_squares_per_tile_y) / 2) * image_square_size - successor.y * num_squares_per_tile_y * image_square_size;

                //    // make a red dot that signifies that we have started to look at this node but not yet considered its neighbors
                //    GameObject g = Instantiate(redDot, new Vector3(x, y, -2), Quaternion.identity);
                //}
            }

            // Add tile to closed list
            //close_points.Add(best_point);
        }

        // this means that we have looked at every node in open list and couldn't find a path. Probably want a slightly better UI thing for this
        print("could not find path :(");
        return null;
    }

    // helper function that finds if ther is a point in the list with the given X Y co-ords
    //private Point FindXYInPointList(List<Point> l, int x, int y)
    //{
    //    foreach (Point p in l)
    //    {
    //        if (p.x == x && p.y == y)
    //        {
    //            return p;
    //        }
    //    }

    //    return null;
    //}

    // helper function that finds the point with the lowest F value in a list of points
    //private Point FindSmallestInList(List<Point> l)
    //{
    //    // never will have an empty list
    //    Point smallest_point = new Point(0, 0);
    //    float smallest_cost = float.MaxValue;
    //    foreach (Point p in l)
    //    {
    //        if (p.f < smallest_cost)
    //        {
    //            smallest_point = p;
    //            smallest_cost = p.f;
    //        }
    //    }
    //    return smallest_point;
    //}

    // helper function that determines all the valid neighbors to a given tile
    private List<Vector2> GetAdjacentOpenTiles(Vector2 pos)
    {
        // each tile holds a bool for if it is passable or not
        // this is a good place to do extra logic for seeing if a tile is valid
        int x = (int)pos.x;
        int y = (int)pos.y;

        List<Vector2> adjacent = new List<Vector2>();

        // left
        if (x > 0 && wholeMap[x - 1, y].GetComponent<TileInfo>().passable)
        {
            adjacent.Add(new Vector2(x - 1, y));
        }

        // right
        if (x < mapWidth - 1 && wholeMap[x + 1, y].GetComponent<TileInfo>().passable)
        {
            adjacent.Add(new Vector2(x + 1, y));
        }

        // up
        if (y > 0 && wholeMap[x, y - 1].GetComponent<TileInfo>().passable)
        {
            adjacent.Add(new Vector2(x, y - 1));
        }

        // down
        if (y < mapHeight - 1 && wholeMap[x, y + 1].GetComponent<TileInfo>().passable)
        {
            adjacent.Add(new Vector2(x, y + 1));
        }
        return adjacent;
    }

    // function that calculates the heuristic value for a tile at x,y given end at endX, endY
    //  this uses the m_heuristic variable to determine which function to use
    private float Heuristic(int x, int y, int endX, int endY)
    {
        //// note: here is where we multiply by the weight, which can be set in the GUI
        //switch (m_heruistic)
        //{
        //    case HeuristicType.Euclidian:
        //        return heuristic_weight * HeuristicEuclidianDist(x, y, endX, endY);
        //    case HeuristicType.Manhattan:
        //        return heuristic_weight * HeuristicManhattanDist(x, y, endX, endY);
        //}
        //// in case something broke?
        return 0.0f;
    }

    // simple distance formula
    private float HeuristicEuclidianDist(int x, int y, int endX, int endY)
    {
        return Mathf.Sqrt((x - endX) * (x - endX) + (y - endY) * (y - endY));
    }

    // equation taken from slides
    private float HeuristicManhattanDist(int x, int y, int endX, int endY)
    {
        return Mathf.Abs(endX - x) + Mathf.Abs(endY - y);
    }
}

public struct PriorityPair
{
    public float p;
    public Vector2 loc;
}

class PriorityQueue
{
    private List<PriorityPair> queue;
    public int size = 0;

    public void Add(float p, Vector2 l)
    {
        int index = IndexFor(p, 0, size);
        PriorityPair newPair = new PriorityPair();
        newPair.p = p;
        newPair.loc = l;
        queue.Insert(index, newPair);
        size++;
    }

    //quick binary search for the appropriote pos
    private int IndexFor(float p, int start, int end)
    {
        if(start == end)
        {
            return start;
        }

        int mid = start + end / 2;
        float test = queue[mid].p;

        if (test > p)
        {
            return IndexFor(p, mid, end);
        }
        else
        {
            return IndexFor(p, start, mid);
        }
    }

    public PriorityPair Next()
    {
        PriorityPair Value = queue[0];
        queue.Remove(Value);
        size--;
        return Value;
        
    }
}
