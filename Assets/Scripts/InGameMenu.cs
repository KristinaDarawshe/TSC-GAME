using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameMenu : MonoBehaviour {
	public GameObject panel1;

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1;
		panel1.SetActive (false);
    }

	public void ExitGame()
	{
		Debug.Log ("quit");
		Application.Quit();
	}
}
