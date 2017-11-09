using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeLevel : MonoBehaviour {
    public string next_level_name;
	
	public void GoToNextLevel()
    {
        SceneManager.LoadScene(next_level_name);
    }
}
