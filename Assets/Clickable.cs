using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clickable : MonoBehaviour {

	/*

	float x;
	void Update(){
		if (IsMouseOver (out x)) {
			print ("clicked");
		} else
			print ("not");
	}
		



	public bool IsMouseOver(out float dst)
	{
		print ("entered");
		dst = 0;
		RaycastHit rh;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		List <Collider> coll = new List<Collider> (gameObject.GetComponents<MeshCollider> ());
		coll.AddRange (gameObject.GetComponentsInChildren<MeshCollider> ());
		bool b = Physics.Raycast (ray, out rh, float.MaxValue);
		print ("pass");
		if (b && coll.Contains (rh.collider)) {
			dst = rh.distance;
			return true;

		}

		return false;
	}

*/


	void Update(){
		print ("entered");
		//0 left
		//1 right
		//2 middle
		if (Input.GetMouseButtonDown(0)){ // if left button pressed...

			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			RaycastHit hit;
			if (Physics.Raycast (ray, out hit)) {
				print ("clicked");

				// the object identified by hit.transform was clicked
				// do whatever you want
			}
		}
	}
}


/*
	// when click add window and doors

fo}r (int i = 0; i < floorColliders.Count; i++)
{
	floorColliders[i].enabled = true;
}

if (SelectedItem.itemType == type.Window || SelectedItem.itemType == type.Door)
{
	WallFace wallface = getSelectedWallFace();
	if (wallface != null)
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, float.MaxValue) && !EventSystem.current.IsPointerOverGameObject())
		{
			Vector2 location;
			Vector2? correctedLocation;
			//if (wallface.RelatedLine.LocateItemInWall (hit.point, SelectedItem, out location, 100, out correctedLocation)) 
			{

				if (tempObjectPlaceholder == null)
					tempObjectPlaceholder = Instantiate(SelectedItem.prefabItem.gameObject);
				if (wallface.RelatedLine.LocateItemInWall(hit.point, SelectedItem, out location, 100, out correctedLocation))
				{
					MeshRenderer[] components = tempObjectPlaceholder.GetComponentsInChildren<MeshRenderer>();
					for (int i = 0; i < components.Length; i++)
					{
						Material[] tempMaterial = new Material[components[i].materials.Length];

						for (int j = 0; j < tempMaterial.Length; j++)
							tempMaterial[j] = PlaceholderMaterial;
						components[i].materials = tempMaterial;
					}
				}
			}


			*/