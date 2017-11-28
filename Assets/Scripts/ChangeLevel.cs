using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeLevel : MonoBehaviour {

	public void change_level(string level_name)
    {
        SceneManager.LoadScene(level_name);
    }
}
