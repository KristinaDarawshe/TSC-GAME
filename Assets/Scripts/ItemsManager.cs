using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
	public EditableBuilding Building;
    public List<item> allObjectList;
    public GameObject MenuItem;
    public GameObject MenuMaterialItem;
    public GameObject windwoPanel;
    public GameObject doorPanel;
    public GameObject wallPanel;
    public GameObject furniturePanel;
    public GameObject systemPanel;
    public GameObject roofPanel;
    // Use this for initialization


    void Start()
    {
        ToggleGroup tg = GetComponent<ToggleGroup>();
        for (int i = 0; i < allObjectList.Count; i++)
        {
			if (allObjectList[i].itemType != type.Roof )//&& allObjectList[i].itemType != type.Wall)
            {

                GameObject itemObj = (GameObject)Instantiate(MenuItem);
				Toggle toggle = itemObj.GetComponent<Toggle>();
				Button but = itemObj.GetComponent<Button> ();

				Image buttonImage = System.Array.Find(itemObj.GetComponentsInChildren<Image>(), delegate (Image img)
                {
                    return img.name == "Image";
                });

                buttonImage.sprite = allObjectList[i].image;
                //button.GetComponentInChildren<Text>(true).text = allObjectList[i].itemName;
				itemObj.GetComponentInChildren<Text>(true).text = "";
				if (toggle != null)
					toggle.group = tg;
                int currentI = i;
				if (toggle != null) {
					toggle.onValueChanged.AddListener (new UnityEngine.Events.UnityAction<bool> (delegate (bool arg0) {
						if (arg0)
							itemSelected (allObjectList [currentI]);
						else
							itemSelected (null);

					}));
				} else {
					
					but.onClick.AddListener (new UnityEngine.Events.UnityAction (delegate () {
						ApplyItem(allObjectList[currentI]);

					}));
				}

				if (allObjectList [i].itemType == type.Wall) {
					itemObj.transform.SetParent(wallPanel.transform);//Setting button parent
				}
					else if (allObjectList[i].itemType == type.Window)
                {
					itemObj.transform.SetParent(windwoPanel.transform);//Setting button parent
                }

                else if (allObjectList[i].itemType == type.Door)
                {
					itemObj.transform.SetParent(doorPanel.transform);//Setting button parent
                }

                else if (allObjectList[i].itemType == type.Furniture)
                {
					itemObj.transform.SetParent(furniturePanel.transform);//Setting button parent
                }

                else if (allObjectList[i].itemType == type.System)
                {
					itemObj.transform.SetParent(systemPanel.transform);//Setting button parent
                }
				itemObj.transform.localRotation = Quaternion.identity;
				itemObj.transform.localPosition = new Vector3(100, -25, 0);
				itemObj.transform.localScale = Vector3.one;
            }
            else
            {
                GameObject button = (GameObject)Instantiate(MenuMaterialItem);
                Image buttonImage = System.Array.Find(button.GetComponentsInChildren<Image>(), delegate (Image img)
                {
                    return img.name == "Image";
                });
                buttonImage.sprite = allObjectList[i].image;
               // button.GetComponentInChildren<Text>(true).text = allObjectList[i].itemName;
				button.GetComponentInChildren<Text>(true).text = "";

                button.GetComponent<Button>().onClick.AddListener(() => OnButtonClick("xxxx"));
                

                if (allObjectList[i].itemType == type.Wall)
                {
                    button.transform.SetParent(wallPanel.transform);//Setting button parent
                }

                else if (allObjectList[i].itemType == type.Roof)
                {
                    button.transform.SetParent(roofPanel.transform);//Setting button parent
                }

                button.transform.localRotation = Quaternion.identity;
                button.transform.localPosition = new Vector3(100, -25, 0);
                button.transform.localScale = Vector3.one;
            }








        }
    }

	void ApplyItem(item item)
	{
		Building.ApplyItem(item);
	}

    void itemSelected(item item)
    {
		Building.SetSelectedItem(item);
    }

    public void OnButtonClick(string imageName)
    {

    }
}