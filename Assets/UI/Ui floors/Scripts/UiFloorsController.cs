using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class UiFloorsController : MonoBehaviour
{
    // Use this for initialization
    public GameObject prefab;
    public Transform contentPanel;
    private int Floornumber = 0;
	private int SlapNumber = 0;
	private bool isSlap = false;
	public EditableBuilding editableBuilding;

    void Start()
    {

    }




    public void SetButtonLast(GameObject btn)
    {
        btn.transform.SetAsLastSibling();
    }

    public void SetButtonFirst(GameObject btn)
    {
        btn.transform.SetAsFirstSibling();
    }

    public void AddNewButton()
    {
        GameObject newButton = Instantiate(prefab) as GameObject;
        Floornumber++;
		int floorID = Floornumber;
		if (isSlap) {
			newButton.GetComponentInChildren<Text>().text = "Slap " + (Floornumber / 2).ToString();

			isSlap = false;
		} else {
			newButton.GetComponentInChildren<Text>().text = "Floor " + (Floornumber / 2 + 1).ToString();

			isSlap = true;
		}

		newButton.GetComponent<Button>().onClick.AddListener (new UnityEngine.Events.UnityAction (delegate() {
			editableBuilding.SelectLayer(floorID);
		}));
        newButton.transform.SetParent(contentPanel.transform, false);
        newButton.transform.SetAsFirstSibling();
    }


}
