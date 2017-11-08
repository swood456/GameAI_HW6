using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollowGridPath : MonoBehaviour {

    [SerializeField]
    private float move_speed;

    List<Vector2> cur_path;
    int index = 1;
    Vector2 end;

    bool is_active = false;

	public void FollowPath(List<Vector2> path, Vector2 end_pos)
    {
        cur_path = path;
        end = end_pos;
        index = 1;
    }

    private void Update()
    {
        if(index < cur_path.Count - 1)
        {
            //print("dest: " + new Vector2(cur_path[index].x, cur_path[index].y));
            if (Vector2.Distance(transform.position, new Vector2(cur_path[index].x, cur_path[index].y)) < move_speed * Time.deltaTime)
            {
                // at the end of a section of path
                transform.position = new Vector3(cur_path[index].x, cur_path[index].y, transform.position.z);
                index++;
                //print("moving on to " + cur_path[index]);
            }
            else
            {
                transform.position += (new Vector3(cur_path[index].x, cur_path[index].y, transform.position.z) - transform.position ).normalized * move_speed * Time.deltaTime;
            }
        }
        else
        {
            // move towards the very end
            if (Vector2.Distance(end, transform.position) > move_speed * Time.deltaTime)
                transform.position += (Vector3)(end - (Vector2) transform.position).normalized * move_speed * Time.deltaTime;
            else
            {
                transform.position = (Vector3)end;
                enabled = false;
            }

        }
    }
}
