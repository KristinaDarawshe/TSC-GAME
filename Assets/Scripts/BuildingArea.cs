using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

// @testcase: prevent split wall in window/door

public enum BuildingEditMode
{
	None,
	Drawing,
	WallFaceSelected,
	WallFaceMoving,
	WallVertexMoving
};

public enum ViewingMode
{
	Interior,
	Exterior
};

public class BuildingArea : MonoBehaviour
{
	public float PublicFloatInsThick = 0.5f, PublicFloatWallThick = 0.1f;
	public InputField roofDepth, wallThick, valueIns, valueWall;
	public Text SelectedWallArea, TotalWallArea, TotalWindowArea, TotalDoorArea, roofArea;//, wallThicktxt;
	// public Text ;
	public static float NumToRemoveError;
	BuildingEditMode _mode;
	public float RoofDepth = 0.4f;
	public BuildingEditMode Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
			if (_mode == BuildingEditMode.WallFaceSelected)
			{
				//MaterialsPanel.SetActive (true);
				//WindowMaterialPanel.SetActive (true);
			}
			else
			{
				// MaterialsPanel.SetActive (false);
				//WindowMaterialPanel.SetActive (false);
			}
		}
	}


	ViewingMode _viewingMode;
	public ViewingMode viewingMode
	{
		get
		{
			return _viewingMode;
		}
		set
		{
			_viewingMode = value;
			if (_viewingMode == ViewingMode.Interior)
			{
				if (Roof != null)
					Roof.SetActive(false);
			}
			else
			{
				if (Roof != null)
					Roof.SetActive(true);
			}
			regeneratePath (false);
		}
	}


	// PARAMETERS
	public int snapGridDistance = 1;
	public GameObject VertexHandle; // red square
	public GameObject snapObject;// the ball to draw 
	public Material DraggedLineMaterial; // yellow when draw

	public Material LineMaterial; // change to white when finish
	public Material WallSelectedMaterial;
	public Material WallWireframeMaterial;//


	public GameObject MaterialsPanel;
	public GameObject WindowMaterialPanel;

	public Material DefaultOuterWallMaterial;
	public Material DefaultInnerWallMaterial;
	public Material DefaultSideMaterial;
	public Material DefaultRoofMaterial;
	public Material DefaultFloorMaterial;
	public Material NoDragge;

	public float Height = 2.0f;
	public bool isCeil = false;
	public Material CeilMaterial;
	public float DoubleClickCatchTime = 0.25f;
	public Material PlaceholderMaterial;
	public Material PlaceholderErrorMaterial;


	private WallFace _selectedWallFace = null;
	private WallFace selectedWallFace
	{
		get
		{
			return _selectedWallFace;
		}
		set
		{

			if (_selectedWallFace != null)
			{
				_selectedWallFace.Selected = false;
			}

			_selectedWallFace = value;

			if (_selectedWallFace != null)
			{
				// activate vertex handle


				_selectedWallFace.Selected = true;
				wallFaceHandleObject.transform.position = (_selectedWallFace.RelatedLine.a + _selectedWallFace.RelatedLine.b) * 0.5f;
				wallFaceHandleObject.transform.position += Vector3.up * _selectedWallFace.RelatedLine.Height;
				wallFaceHandleObject.SetActive(true);
				wallFaceHandleDraggable.Enabled = true;
				//				wallFaceHandleDraggable.FreezeY = false;

				vertexAHandleObject.transform.position = _selectedWallFace.RelatedLine.a;
				vertexAHandleObject.transform.position += Vector3.up * _selectedWallFace.RelatedLine.Height;
				vertexAHandleObject.SetActive(true);
				vertexAHandleDraggable.Enabled = true;
				//				vertexAHandleDraggable.FreezeY = false;

				vertexBHandleObject.transform.position = _selectedWallFace.RelatedLine.b;
				vertexBHandleObject.transform.position += Vector3.up * _selectedWallFace.RelatedLine.Height;
				vertexBHandleObject.SetActive(true);
				vertexBHandleDraggable.Enabled = true;
				//				vertexBHandleDraggable.FreezeY = false;

				DetachButton.interactable = true;
				DeleteButton.interactable = true;
			}
			else
			{
				// dectivate vertex handle

				wallFaceHandleDraggable.Enabled = false;
				wallFaceHandleObject.SetActive(false);

				vertexAHandleObject.SetActive(false);
				vertexAHandleDraggable.Enabled = false;

				vertexBHandleObject.SetActive(false);
				vertexBHandleDraggable.Enabled = false;
				DetachButton.interactable = false;
				DeleteButton.interactable = false;
				Mode = BuildingEditMode.None;

			}
		}
	}

	bool roofEnabled;
	public bool RoofEnabled
	{
		get
		{
			return roofEnabled;
		}
		set
		{
			roofEnabled = value;
			regeneratePath(false);
		}
	}

	public void CopyToNewLayer(BuildingArea ba)
	{
		//

		ba.BuildingAreaCollider = ba.GetComponent<Collider>();
		ba.snapObject = snapObject;
		ba.VertexHandle = VertexHandle;
		ba.DetachButton = GameObject.Find("Detach button").GetComponent<Button>();
		ba.DeleteButton = GameObject.Find("Delete button").GetComponent<Button>();
		ba.WallWireframeMaterial = WallWireframeMaterial;
		ba.WallSelectedMaterial = WallSelectedMaterial;

		ba.wallFaceHandleObject = GameObject.Instantiate(ba.VertexHandle);
		ba.wallFaceHandleDraggable = ba.wallFaceHandleObject.AddComponent<Draggable>();
		ba.wallFaceHandleDraggable.Enabled = false;
		//		ba.wallFaceHandleDraggable.XEnabled = true;
		//		ba.wallFaceHandleDraggable.YEnabled = false;
		//		ba.wallFaceHandleDraggable.ZEnabled = true;
		//		ba.wallFaceHandleDraggable.FreezeY = false;
		//		ba.wallFaceHandleDraggable.XSnapDistance = ba.snapGridDistance;
		//		ba.wallFaceHandleDraggable.YSnapDistance = ba.snapGridDistance;
		//		ba.wallFaceHandleDraggable.ZSnapDistance = ba.snapGridDistance;
		ba.wallFaceHandleDraggable.StartMoving += ba.WallFaceHandleDraggable_StartMoving;
		ba.wallFaceHandleDraggable.Moving += ba.WallFaceHandleDraggable_Moving;
		ba.wallFaceHandleDraggable.EndMoving += ba.WallFaceHandleDraggable_EndMoving;


		ba.vertexAHandleObject = GameObject.Instantiate(ba.VertexHandle);
		ba.vertexAHandleDraggable = ba.vertexAHandleObject.AddComponent<Draggable>();
		ba.vertexAHandleDraggable.Enabled = false;
		//		ba.vertexAHandleDraggable.XEnabled = true;
		//		ba.vertexAHandleDraggable.YEnabled = false;
		//		ba.vertexAHandleDraggable.ZEnabled = true;
		//		ba.vertexAHandleDraggable.FreezeY = false;
		//		ba.vertexAHandleDraggable.XSnapDistance = ba.snapGridDistance;
		//		ba.vertexAHandleDraggable.YSnapDistance = ba.snapGridDistance;
		//		ba.vertexAHandleDraggable.ZSnapDistance = ba.snapGridDistance;
		ba.vertexAHandleDraggable.StartMoving += ba.vertexAHandleDraggable_StartMoving;
		ba.vertexAHandleDraggable.Moving += ba.vertexAHandleDraggable_Moving;
		ba.vertexAHandleDraggable.EndMoving += ba.vertexAHandleDraggable_EndMoving;

		ba.vertexBHandleObject = GameObject.Instantiate(ba.VertexHandle);
		ba.vertexBHandleDraggable = ba.vertexBHandleObject.AddComponent<Draggable>();
		ba.vertexBHandleDraggable.Enabled = false;
		//		ba.vertexBHandleDraggable.XEnabled = true;
		//		ba.vertexBHandleDraggable.YEnabled = false;
		//		ba.vertexBHandleDraggable.ZEnabled = true;
		//		ba.vertexBHandleDraggable.FreezeY = false;
		//		ba.vertexBHandleDraggable.XSnapDistance = ba.snapGridDistance;
		//		ba.vertexBHandleDraggable.YSnapDistance = ba.snapGridDistance;
		//		ba.vertexBHandleDraggable.ZSnapDistance = ba.snapGridDistance;
		ba.vertexBHandleDraggable.StartMoving += ba.vertexBHandleDraggable_StartMoving;
		ba.vertexBHandleDraggable.Moving += ba.vertexBHandleDraggable_Moving;
		ba.vertexBHandleDraggable.EndMoving += ba.vertexBHandleDraggable_EndMoving;

		ba.DefaultFloorMaterial = DefaultFloorMaterial;
		ba.DefaultInnerWallMaterial = DefaultInnerWallMaterial;
		ba.DefaultOuterWallMaterial = DefaultOuterWallMaterial;
		ba.DefaultRoofMaterial = DefaultRoofMaterial;
		ba.DefaultSideMaterial = DefaultSideMaterial;
		ba.DoubleClickCatchTime = DoubleClickCatchTime;
		ba.lastClickTime = lastClickTime;
		//ba.floors auto generated
		ba.floors = new List<GameObject>();
		ba.items = new List<GameObject>(items);
		ba.LineMaterial = LineMaterial;
		ba.isCeil = isCeil;
		ba.CeilMaterial = CeilMaterial;
		ba.lines = new List<Line>();
		{
			ba.lineVertices = new List<Vector3> (lines [0].Vertices);

			for (int i = 0; i < lines.Count; i++) {
				Line l = new Line (ba.lineVertices, lines [i].aID, lines [i].bID, lines [i].InsulationThickness, lines [i].WallThickness, GameObject.Instantiate (lines [i].LineMaterial), GameObject.Instantiate (lines [i].InnerMaterial), GameObject.Instantiate (lines [i].OuterMaterial), GameObject.Instantiate (lines [i].SideMaterial));

				for (int j = 0; j < lines [i].Doors.Count; j++) {
					WallDoor wd = new WallDoor (l, lines [i].Doors [j].Position.x, lines [i].Doors [j].DoorWidth, lines [i].Doors [j].DoorHeight, GameObject.Instantiate (lines [i].Doors [j].Door));
					l.Doors.Add (wd);
				}
				for (int j = 0; j < lines [i].Windows.Count; j++) {
					WallWindow ww = new WallWindow (l, lines [i].Windows [j].Position, lines [i].Windows [j].WindowWidth, lines [i].Windows [j].WindowHeight, GameObject.Instantiate (lines [i].Windows [j].Window));
					l.Windows.Add (ww);
				}
				l.Height = lines [i].Height;
				l.LedgeHeight = lines [i].LedgeHeight;
				l.LineType = lines [i].LineType;
				l.Parent = ba.transform;
				l.ParentLine = null;
				l.WindowHeight = lines [i].WindowHeight;
				l.Enabled = lines [i].Enabled;
				ba.lines.Add (l);
			}

		}
		ba.Mode = Mode;
		ba.MouseStartDistance = MouseStartDistance;
		ba.MouseStartPosition = MouseStartPosition;
		ba.SelectedItem = SelectedItem;
		ba.selectedWallFace = null;
		ba.snapEnabled = snapEnabled;
		ba.pointASelected = false;
		ba.verticesSelected = new List<int>();


		ba.viewingMode = viewingMode;
		ba.cameraTarget = cameraTarget;
		ba.SelectedItem = null;
		ba.DraggedLineMaterial = DraggedLineMaterial;
		ba.gameCamera = Camera.main.GetComponent<ObjectFollowCamera>();
		ba.DraggedLine = new Line(new List<Vector3>() { Vector3.zero, Vector3.zero }, 0, 1, 0.2f, 0.2f, ba.DraggedLineMaterial, null, null, null);
		ba.DraggedLine.Enabled = false;
		ba.IsBasement = IsBasement;

		ba.PlaceholderMaterial = PlaceholderMaterial;
		ba.PlaceholderErrorMaterial = PlaceholderErrorMaterial;

		ba.SelectedWallArea = SelectedWallArea;
		ba.roofArea = roofArea;
		ba.roofDepth = roofDepth;
		ba.RoofDepth = RoofDepth;
		ba.TotalWallArea = TotalWallArea;
		ba.TotalDoorArea = TotalDoorArea;
		ba.TotalWindowArea = TotalWindowArea;
		ba.wallThick = wallThick;
		ba.valueIns = valueIns;
		ba.valueWall = valueWall;
	}
    List<Vector3> grid;

    public void SetWorkingHeight(float y)
	{
		 grid = new List<Vector3>(); // 11 * 11
		for (int i = -5; i <= 5; i++)
		{
			for (int j = -5; j <= 5; j++)
			{
				if (((MeshCollider)BuildingAreaCollider).sharedMesh.triangles.Length == 0 || IsWallOverBuildingArea(new Vector3(i, y + 0.001f, j), new Vector3(i, y + 0.001f, j)))
				{
					grid.Add(new Vector3(i, y, j));


				}
			}
		}
		Vector3[] aGrid = grid.ToArray();
		try{vertexAHandleDraggable.SetAllowedPoints(aGrid);
			vertexBHandleDraggable.SetAllowedPoints(aGrid);
			wallFaceHandleDraggable.SetAllowedPoints(aGrid);
		}catch{
		}

	}

	List<GameObject> items = new List<GameObject>();
	private List<WallFace> wallFaces = new List<WallFace>();

	private List<Vector3> lineVertices = new List<Vector3>();
	public List<Line> lines = new List<Line>();

	private List<GameObject> floors = new List<GameObject>();
	private List<Collider> floorColliders = new List<Collider>();
	private GameObject Roof;

	List<int> verticesSelected = new List<int>();

	bool pointASelected = false;
	bool snapEnabled = true;
	Vector3 pointA;

	Vector3 MouseStartPosition;
	float MouseStartDistance;

	//basement is rectangle only
	public bool IsBasement = false;
	public GameObject Basement;
	public float BasementHeight = 0.8f;

	Draggable wallFaceHandleDraggable;
	GameObject wallFaceHandleObject;
	/// <summary>
	/// to make a shadow to the window or door
	/// </summary>
	GameObject tempObjectPlaceholder;
	public static Vector3 PointAStatic;
	public static Vector3 PointBStatic;
	Vector3 pointB;
	void WallFaceHandleDraggable_StartMoving(GameObject sender, Vector3 oldPosition, Vector3 newPosition)
	{
		Mode = BuildingEditMode.WallFaceMoving;
		vertexAHandleDraggable.Enabled = false;
		vertexBHandleDraggable.Enabled = false;
	}

	void WallFaceHandleDraggable_Moving(GameObject sender, Vector3 oldPosition, Vector3 newPosition)
	{
		Vector3 dif = newPosition - oldPosition;

		//        if (IsWallOverBuildingArea(vertexAHandleObject.transform.position + dif, vertexBHandleObject.transform.position + dif))
		{
			wallFaceHandleObject.transform.position += dif;
			selectedWallFace.RelatedLine.a += dif;
			selectedWallFace.RelatedLine.b += dif;
			vertexAHandleObject.transform.position += dif;
			vertexBHandleObject.transform.position += dif;
			regeneratePath(false);
		}
	}

	void WallFaceHandleDraggable_EndMoving(GameObject sender, Vector3 oldPosition, Vector3 newPosition)
	{
		//        if (IsWallOverBuildingArea(vertexAHandleObject.transform.position, vertexBHandleObject.transform.position))
		{
			Mode = BuildingEditMode.WallFaceSelected;
			vertexAHandleDraggable.Enabled = true;
			vertexBHandleDraggable.Enabled = true;
			regeneratePath(true);
		}
	}

	Draggable vertexAHandleDraggable;
	GameObject vertexAHandleObject;

	Draggable vertexBHandleDraggable;
	GameObject vertexBHandleObject;

	void vertexAHandleDraggable_StartMoving(GameObject sender, Vector3 oldPosition, Vector3 newPosition)
	{
        //--------------------------------------------------------------------------------
        //00000000000000000000000000000000000000000000000000000000000000000000000000000000
        //This Code to solve the problem of dragging wall to zero area Named as Selection Wall Problem.
        List<Vector3> LocalGrid = new List<Vector3>(grid);
        for (int i = LocalGrid.Count - 1; i >= 0; i--)
        {
            if (LocalGrid[i].x == vertexBHandleObject.transform.position.x && LocalGrid[i].z == vertexBHandleObject.transform.position.z)
                LocalGrid.RemoveAt(i);
        }

        vertexAHandleDraggable.SetAllowedPoints(LocalGrid.ToArray());
        //00000000000000000000000000000000000000000000000000000000000000000000000000000000
        //--------------------------------------------------------------------------------
        Mode = BuildingEditMode.WallVertexMoving;
		vertexBHandleDraggable.Enabled = false;
		vertexBHandleObject.SetActive(false);
		wallFaceHandleObject.SetActive(false);
		wallFaceHandleDraggable.Enabled = false;
	}

	void vertexAHandleDraggable_Moving(GameObject sender, Vector3 oldPosition, Vector3 newPosition)
	{
		if (newPosition != vertexBHandleObject.transform.position)
		{
			Vector3 dif = newPosition - oldPosition;
			//            if (IsWallOverBuildingArea(vertexAHandleObject.transform.position + dif, vertexBHandleObject.transform.position))
			{

				vertexAHandleObject.transform.position += dif;
				wallFaceHandleObject.transform.position += dif * 0.5f;
				Vector3 pos = selectedWallFace.RelatedLine.a;

				for (int i = 0; i < lines.Count; i++)
				{
					if ((lines[i].a - pos).sqrMagnitude <= 0.00001f)
						lines[i].a += dif;

					if ((lines[i].b - pos).sqrMagnitude <= 0.00001f)
						lines[i].b += dif;
				}

				regeneratePath(false);
			}
		}
	}

	void vertexAHandleDraggable_EndMoving(GameObject sender, Vector3 oldPosition, Vector3 newPosition)
	{
		//        if (IsWallOverBuildingArea(vertexAHandleObject.transform.position, vertexBHandleObject.transform.position))
		{
            //--------------------------------------------------------------------------------
            //00000000000000000000000000000000000000000000000000000000000000000000000000000000
            //This Code is related to Selection Wall Problem : Zero Area.
            vertexBHandleDraggable.SetAllowedPoints(grid.ToArray());
            //00000000000000000000000000000000000000000000000000000000000000000000000000000000	
            //--------------------------------------------------------------------------------
            Mode = BuildingEditMode.WallFaceSelected;
			vertexBHandleDraggable.Enabled = true;
			vertexBHandleObject.SetActive(true);
			wallFaceHandleDraggable.Enabled = true;
			wallFaceHandleObject.SetActive(true);
			regeneratePath(true);
		}
	}


	void vertexBHandleDraggable_StartMoving(GameObject sender, Vector3 oldPosition, Vector3 newPosition)
	{
        //--------------------------------------------------------------------------------
        //00000000000000000000000000000000000000000000000000000000000000000000000000000000
        //This code to solve the problem of selection wall : Zero Area of Wall: When B moves to A and makes a Zero Area.
        List<Vector3> LocalGrid;
        Mode = BuildingEditMode.WallVertexMoving;
        LocalGrid = new List<Vector3>(grid);
        //LocalGrid.Remove(vertexAHandleObject.transform.position);
        for (int i = LocalGrid.Count - 1; i >= 0; i--)
        {
            if (LocalGrid[i].x == vertexAHandleObject.transform.position.x && LocalGrid[i].z == vertexAHandleObject.transform.position.z)
                LocalGrid.RemoveAt(i);
        }
        vertexBHandleDraggable.SetAllowedPoints(LocalGrid.ToArray());
        //00000000000000000000000000000000000000000000000000000000000000000000000000000000
        //--------------------------------------------------------------------------------
        Mode = BuildingEditMode.WallVertexMoving;
		vertexAHandleDraggable.Enabled = false;
		vertexAHandleObject.SetActive(false);
		wallFaceHandleDraggable.Enabled = false;
		wallFaceHandleObject.SetActive(false);
	}

	void vertexBHandleDraggable_Moving(GameObject sender, Vector3 oldPosition, Vector3 newPosition)
	{
		if (newPosition != vertexAHandleObject.transform.position)
		{
			Vector3 dif = newPosition - oldPosition;
			//            if (IsWallOverBuildingArea(vertexAHandleObject.transform.position, vertexBHandleObject.transform.position + dif))
			{

				vertexBHandleObject.transform.position += dif;
				wallFaceHandleObject.transform.position += dif * 0.5f;
				Vector3 pos = selectedWallFace.RelatedLine.b;

				for (int i = 0; i < lines.Count; i++)
				{
					if ((lines[i].a - pos).sqrMagnitude <= 0.00001f)
						lines[i].a += dif;

					if ((lines[i].b - pos).sqrMagnitude <= 0.00001f)
						lines[i].b += dif;
				}

				regeneratePath(false);
			}
		}
	}

	void vertexBHandleDraggable_EndMoving(GameObject sender, Vector3 oldPosition, Vector3 newPosition)
	{
		//        if (IsWallOverBuildingArea(vertexAHandleObject.transform.position, vertexBHandleObject.transform.position))
		{
            //--------------------------------------------------------------------------------
            //00000000000000000000000000000000000000000000000000000000000000000000000000000000
            //This Code is related to Selection Wall Problem : Zero Area.
            vertexAHandleDraggable.SetAllowedPoints(grid.ToArray());
            //00000000000000000000000000000000000000000000000000000000000000000000000000000000
            //--------------------------------------------------------------------------------
            Mode = BuildingEditMode.WallFaceSelected;
			vertexAHandleDraggable.Enabled = true;
			vertexAHandleObject.SetActive(true);
			wallFaceHandleDraggable.Enabled = true;
			wallFaceHandleObject.SetActive(true);
			regeneratePath(true);
		}
	}

	Button DetachButton;
	Button DeleteButton;

	public GameObject upperWallFace;

	public void DetachSelectedWall()
	{
		if (selectedWallFace != null) {
			selectedWallFace.RelatedLine.DetachA ();
			selectedWallFace.RelatedLine.DetachB ();

			regeneratePath (true);
		}
	}


	public void DeleteSelectedWall()
	{
		WallFace wf = selectedWallFace;
		selectedWallFace = null;
		lines.Remove(wf.RelatedLine);
		wf.RelatedLine.Destroy();
		//		wf.Destroy ();

		regeneratePath(true);
	}

	public void SetSelectedWallFaceMaterials(Material innerMaterial, Material outerMaterial, Material sideMaterial)
	{
		// this function will be called from material panel
		List<Vector3> endpoints = new List<Vector3>();
		endpoints.Add(selectedWallFace.RelatedLine.a);
		selectedWallFace.RelatedLine.InnerMaterial = innerMaterial;
		selectedWallFace.RelatedLine.OuterMaterial = outerMaterial;
		selectedWallFace.RelatedLine.SideMaterial = sideMaterial;
		for (int i = 0; i < endpoints.Count; i++)
		{

			for (int j = 0; j < lines.Count; j++)
			{
				if (lines[j] != selectedWallFace.RelatedLine)
				{

					if ((endpoints[i] - lines[j].a).sqrMagnitude <= 0.00001f && endpoints.FindIndex(delegate (Vector3 v) { return (lines[j].b - v).sqrMagnitude <= 0.00001f; }) == -1)
					{
						lines[j].InnerMaterial = innerMaterial;
						lines[j].OuterMaterial = outerMaterial;
						lines[j].SideMaterial = sideMaterial;
						endpoints.Add(lines[j].b);
					}
					else if ((endpoints[i] - lines[j].b).sqrMagnitude <= 0.00001f && endpoints.FindIndex(delegate (Vector3 v) { return (lines[j].a - v).sqrMagnitude <= 0.00001f; }) == -1)
					{
						lines[j].InnerMaterial = innerMaterial;
						lines[j].OuterMaterial = outerMaterial;
						lines[j].SideMaterial = sideMaterial;
						endpoints.Add(lines[j].a);
					}
				}
			}
		}
	}

	Collider BuildingAreaCollider;

	bool IsWallOverBuildingArea(Vector3 a, Vector3 b)
	{
		RaycastHit h1, h2 = new RaycastHit();
		return BuildingAreaCollider.Raycast(new Ray(a, Vector3.down), out h1, float.MaxValue) && BuildingAreaCollider.Raycast(new Ray(b, Vector3.down), out h2, float.MaxValue);
	}

	public void SetWindowMaterials(GameObject obj)
	{
		for (int i = 0; i < selectedWallFace.RelatedLine.Windows.Count; i++)
		{
			selectedWallFace.RelatedLine.Windows[i].Window = Instantiate(obj);
			//Vector3 start = selectedWallFace.RelatedSegment.a + (selectedWallFace.RelatedSegment.b - selectedWallFace.RelatedSegment.a).normalized * (selectedWallFace.RelatedSegment.Windows [i].Position.x + selectedWallFace.RelatedSegment.Windows [i].WindowWidth * 0.5f);


			//start += Vector3.up * selectedWallFace.RelatedSegment.Windows [i].Position.y;

			selectedWallFace.RelatedLine.Windows[i].Update();
		}
	}

	public void SetSelectedWallFaceWindowCount(string count)
	{
		SetSelectedWallFaceWindowCount(int.Parse(count));
	}

	public void SetSelectedWallFaceWindowCount(int count)
	{
		if (count == 0)
		{
			_selectedWallFace.RelatedLine.LineType = 0;
			_selectedWallFace.RelatedLine.Windows.Clear();
		}
		else
		{
			float frac = 1.0f / (2.0f * count + 1);
			_selectedWallFace.RelatedLine.LineType = LineType.Window;

			_selectedWallFace.RelatedLine.Windows.Clear();

			for (int i = 0; i < count; i++)
			{
				//              WallWindow window = new WallWindow (_selectedWallFace.RelatedLine, new Vector2((i * 2 + 1) * frac * (_selectedWallFace.RelatedLine.b - _selectedWallFace.RelatedLine.a).magnitude, _selectedWallFace.RelatedLine.Height * 0.2f),(_selectedWallFace.RelatedLine.b - _selectedWallFace.RelatedLine.a).magnitude * 1.0f / (count * 2 + 1), _selectedWallFace.RelatedLine.Height * 0.5f, null);
				//              _selectedWallFace.RelatedLine.Windows.Add (window);

				WallDoor w = new WallDoor(_selectedWallFace.RelatedLine, (i * 2 + 1) * frac * (_selectedWallFace.RelatedLine.b - _selectedWallFace.RelatedLine.a).magnitude, (_selectedWallFace.RelatedLine.b - _selectedWallFace.RelatedLine.a).magnitude * 1.0f / (count * 2 + 1), _selectedWallFace.RelatedLine.Height * 0.5f, null);
				_selectedWallFace.RelatedLine.Doors.Add(w);
			}
		}
		regeneratePath(false);
	}

	item selectedItem;
	public item SelectedItem
	{
		get
		{
			return selectedItem;
		}
		set
		{
			selectedItem = value;

			if (selectedItem == null)
			{
				GameObject.Destroy(tempObjectPlaceholder);
				tempObjectPlaceholder = null;
			}
		}
	}


	//public void SetSelectedItem(){




	ObjectFollowCamera gameCamera;
	// set on the first click or second click and used to set camera target on double click
	Vector3 cameraTarget;




	void Awake()
	{
//		Basement = GameObject.CreatePrimitive(PrimitiveType.Cube);
//		Basement.SetActive(true);
//
		BuildingAreaCollider = GetComponent<Collider>();
		viewingMode = ViewingMode.Exterior;
		SelectedItem = null;
		gameCamera = Camera.main.GetComponent<ObjectFollowCamera>();

		DetachButton = GameObject.Find("Detach button").GetComponent<Button>();
		DeleteButton = GameObject.Find("Delete button").GetComponent<Button>();

		if (snapObject != null) {
			snapObject = GameObject.Instantiate (snapObject);
			Debug.Log ("arrow appears");
		}
		DraggedLine = new Line(new List<Vector3>() { Vector3.zero, Vector3.zero }, 0, 1, 0.2f, 0.2f, DraggedLineMaterial, null, null, null);
		DraggedLine.Height = Height;
		DraggedLine.Enabled = false;

		if (VertexHandle != null)
		{
			wallFaceHandleObject = GameObject.Instantiate(VertexHandle);
			wallFaceHandleDraggable = wallFaceHandleObject.AddComponent<Draggable>();
			wallFaceHandleDraggable.Enabled = false;

			//        wallFaceHandleDraggable.XEnabled = true;
			//        wallFaceHandleDraggable.YEnabled = false;
			//        wallFaceHandleDraggable.ZEnabled = true;
			//        wallFaceHandleDraggable.FreezeY = false;
			//        wallFaceHandleDraggable.XSnapDistance = snapGridDistance;
			//        wallFaceHandleDraggable.YSnapDistance = snapGridDistance;
			//        wallFaceHandleDraggable.ZSnapDistance = snapGridDistance;
			wallFaceHandleDraggable.StartMoving += WallFaceHandleDraggable_StartMoving;
			wallFaceHandleDraggable.Moving += WallFaceHandleDraggable_Moving;
			wallFaceHandleDraggable.EndMoving += WallFaceHandleDraggable_EndMoving;


			vertexAHandleObject = GameObject.Instantiate(VertexHandle);
			vertexAHandleDraggable = vertexAHandleObject.AddComponent<Draggable>();
			vertexAHandleDraggable.Enabled = false;
			//        vertexAHandleDraggable.XEnabled = true;
			//        vertexAHandleDraggable.YEnabled = false;
			//        vertexAHandleDraggable.ZEnabled = true;
			//        vertexAHandleDraggable.FreezeY = false;
			//        vertexAHandleDraggable.XSnapDistance = snapGridDistance;
			//        vertexAHandleDraggable.YSnapDistance = snapGridDistance;
			//        vertexAHandleDraggable.ZSnapDistance = snapGridDistance;
			vertexAHandleDraggable.StartMoving += vertexAHandleDraggable_StartMoving;
			vertexAHandleDraggable.Moving += vertexAHandleDraggable_Moving;
			vertexAHandleDraggable.EndMoving += vertexAHandleDraggable_EndMoving;

			vertexBHandleObject = GameObject.Instantiate(VertexHandle);
			vertexBHandleDraggable = vertexBHandleObject.AddComponent<Draggable>();
			vertexBHandleDraggable.Enabled = false;
			//        vertexBHandleDraggable.XEnabled = true;
			//        vertexBHandleDraggable.YEnabled = false;
			//        vertexBHandleDraggable.ZEnabled = true;
			//        vertexBHandleDraggable.FreezeY = false;
			//
			//        vertexBHandleDraggable.XSnapDistance = snapGridDistance;
			//        vertexBHandleDraggable.YSnapDistance = snapGridDistance;
			//        vertexBHandleDraggable.ZSnapDistance = snapGridDistance;
			vertexBHandleDraggable.StartMoving += vertexBHandleDraggable_StartMoving;
			vertexBHandleDraggable.Moving += vertexBHandleDraggable_Moving;
			vertexBHandleDraggable.EndMoving += vertexBHandleDraggable_EndMoving;
		}
		SetWorkingHeight (0.0f);
	}






	Line DraggedLine = null;
	//used for basement
	Line[] DraggedAreaLines = new Line[4];

	float lastClickTime;

	Bounds? alignToFloor(Bounds aabb, int maxTries)
	{
		while (maxTries >= 0)
		{
			bool flag = false;
			int floorID = -1;
			RaycastHit hp = new RaycastHit();

			for (int i = 0; i < floorColliders.Count; i++)
			{
				bool wasEnabled = floorColliders[i].enabled;
				floorColliders[i].enabled = true;
				int tmp = 0;
				//				RaycastHit hp;
				if (!floorColliders[i].Raycast(new Ray(new Vector3(aabb.min.x, aabb.max.y, aabb.min.z), Vector3.down), out hp, float.MaxValue))
					tmp++;
				if (!floorColliders[i].Raycast(new Ray(aabb.max, Vector3.down), out hp, float.MaxValue))
					tmp++;
				if (!floorColliders[i].Raycast(new Ray(new Vector3(aabb.min.x, aabb.max.y, aabb.max.z), Vector3.down), out hp, float.MaxValue))
					tmp++;
				if (!floorColliders[i].Raycast(new Ray(new Vector3(aabb.max.x, aabb.max.y, aabb.min.z), Vector3.down), out hp, float.MaxValue))
					tmp++;

				if (tmp == 0)
				{
					floorID = i;
					flag = true;
					floorColliders[i].enabled = wasEnabled;
					break;
				}
				floorColliders[i].enabled = wasEnabled;
			}

			if (!flag)
				return null;



			//            if (!BuildingAreaCollider.Raycast(new Ray(aabb.min, Vector3.down), out hp, float.MaxValue))//mofeed
			//                return null;
			//            if (!BuildingAreaCollider.Raycast(new Ray(aabb.max, Vector3.down), out hp, float.MaxValue))
			//                return null;
			//            if (!BuildingAreaCollider.Raycast(new Ray(new Vector3(aabb.min.x, aabb.min.y, aabb.max.z), Vector3.down), out hp, float.MaxValue))
			//                return null;
			//            if (!BuildingAreaCollider.Raycast(new Ray(new Vector3(aabb.max.x, aabb.min.y, aabb.min.z), Vector3.down), out hp, float.MaxValue))
			//                return null;

			{
				bool wasEnabled = floorColliders[floorID].enabled;
				floorColliders[floorID].enabled = true;
				if (floorColliders[floorID].Raycast(new Ray(aabb.min, Vector3.down), out hp, float.MaxValue))
				{
					aabb.center += Vector3.down * hp.distance;
				}
				floorColliders[floorID].enabled = wasEnabled;
			}

			Bounds oldAABB = aabb;
			aabb = alignToFloor(aabb);
			if (oldAABB == aabb)
				return new Bounds?(aabb);

			maxTries--;
		}
		return null;
	}

	Bounds alignToFloor(Bounds aabb)
	{


		for (int i = 0; i < items.Count; i++)
		{
			Collider[] colliders = items[i].GetComponentsInChildren<Collider>();
			if (colliders.Length == 0)
				continue;

			Bounds aabb2 = colliders[0].bounds;
			for (int j = 0; j < colliders.Length; j++)
			{
				aabb2.Encapsulate(colliders[j].bounds);
			}

			if (aabb.Intersects(aabb2))
			{
				Vector3 dif = aabb.center - aabb2.center;
				if (Mathf.Abs(dif.x) > Mathf.Abs(dif.z))
				{
					// x
					if (dif.x > 0)
						aabb.center += Vector3.right * (aabb2.max.x - aabb.min.x);
					else
						aabb.center += Vector3.right * (aabb.max.x - aabb2.min.x);


				}
				else
				{
					// z
					if (dif.z > 0)
						aabb.center += Vector3.forward * (aabb2.max.z - aabb.min.z);
					else
						aabb.center += Vector3.forward * (aabb.max.z - aabb2.min.z);

				}

			}
		}
		return aabb;
	}

	public Button BasementButton;
	/// <summary>
	/// this will enable basement button (done) whenever basement area > 0
	/// </summary>
	void UpdateEnableBasementButton()
	{
		if (lines.Count > 0)
		{
			Vector3 tmp = lines[0].Vertices[0] - lines[0].Vertices[2];
			BasementButton.interactable = Mathf.Abs(tmp.x * tmp.z) > 0.001f;
		}
		else
		{
			BasementButton.interactable = false;
		}
	}

	void Update()
	{
		bool flagDoors = false, flagWindows = false;

		//selected wall area
		if (selectedWallFace != null)
			SelectedWallArea.text = returnWallArea().ToString();
		else
			SelectedWallArea.text = "0.0";

		//total walls
		if (lines.Count != 0)
			TotalWallArea.text = returnTotalWallsArea().ToString();
		else
			TotalWallArea.text = "0.0";

		//selected wall thickness
		if (selectedWallFace != null)
		{
			//setWallThickness();
			// print("thick before" + selectedWallFace.RelatedLine.Thickness);
			//  getWallThickness();
			//  print("thick after" + selectedWallFace.RelatedLine.Thickness);
		}
		else
		{
			//  wallThick.text = "0.0";
			//  wallThicktxt.text = "0.0";
		}

		//total doors
		for (int i = 0; i < lines.Count; i++)
		{
			if (lines[i].Doors.Count != 0)
			{
				flagDoors = true;
			}
		}
		if (flagDoors != false)
		{
			TotalDoorArea.text = returnAllDoorArea().ToString();
		}
		//    else
		//   TotalDoorArea.text = "0.0";

		// Total windows 
		for (int i = 0; i < lines.Count; i++)
		{
			if (lines[i].Windows.Count != 0)
			{
				flagWindows = true;
			}
		}
		if (flagWindows != false)
		{
			TotalWindowArea.text = returnAllWindowArea().ToString();
		}
		// else
		//     TotalDoorArea.text = "0.0";

		//roof area & depth
		if (Roof != null)
		{
			roofArea.text = returnRoofArea().ToString();
			GetRoofDepth();
		}
		else
		{
			roofArea.text = "0.0";
			roofDepth.text = "0.0";
		}


		if (!enabled)
			return;
		//
		//        if (IsBasement)
		//        {
		//
		//
		//            if ((Mode == BuildingEditMode.None || Mode == BuildingEditMode.Drawing) && SelectedItem == null)
		//            {
		//                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		//                Collider coll = GetComponent<MeshCollider>();
		//                RaycastHit hit;
		//                snapObject.SetActive(false);
		//                if (Physics.Raycast(ray, out hit, float.MaxValue) && hit.collider == coll && !EventSystem.current.IsPointerOverGameObject())
		//                {
		//
		//                    if (snapEnabled)
		//                    {
		//                        hit.point = snapToGrid(hit.point);
		//                    }
		//
		//
		//                    snapObject.SetActive(true);
		//                    snapObject.transform.position = hit.point;
		//                    if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
		//                    {
		//
		//                        if (!pointASelected)
		//                        {
		//                            pointA = hit.point;
		//                            if (DraggedAreaLines[0] == null)
		//                            {
		//                                DraggedAreaLines[0] = new Line(new List<Vector3>() { pointA, pointA, pointA, pointA }, 0, 1, 0.2f, 0.2f, DraggedLineMaterial, DefaultOuterWallMaterial, DefaultOuterWallMaterial, DefaultOuterWallMaterial);
		//                                for (int i = 0; i < 3; i++)
		//                                    DraggedAreaLines[i + 1] = new Line(DraggedAreaLines[0].Vertices, i + 1, (i + 2) % 4, 0.2f, 0.2f, DraggedLineMaterial, DefaultOuterWallMaterial, DefaultOuterWallMaterial, DefaultOuterWallMaterial);
		//                            }
		//
		//                            for (int i = 0; i < DraggedAreaLines[0].Vertices.Count; i++)
		//                            {
		//                                DraggedAreaLines[0].Vertices[i] = pointA;
		//                                DraggedAreaLines[i].Enabled = true;
		//                                DraggedAreaLines[i].aID = DraggedAreaLines[i].aID;
		//                                DraggedAreaLines[i].bID = DraggedAreaLines[i].bID;
		//                            }
		//
		//                            pointASelected = true;
		//                        }
		//                        else
		//                        {
		//                            DraggedAreaLines[0].Vertices[2] = hit.point;
		//
		//                            DraggedAreaLines[0].Vertices[1] = new Vector3(pointA.x, DraggedAreaLines[0].Vertices[1].y, hit.point.z);
		//                            DraggedAreaLines[0].Vertices[3] = new Vector3(hit.point.x, DraggedAreaLines[0].Vertices[1].y, pointA.z);
		//                            for (int i = 0; i < 4; i++)
		//                            {
		//                                DraggedAreaLines[i].aID = DraggedAreaLines[i].aID;
		//                                DraggedAreaLines[i].bID = DraggedAreaLines[i].bID;
		//                            }
		//
		//
		//                            //DraggedAreaLines [3].bID = 0;
		//                        }
		//                    }
		//                    else if (Input.GetMouseButtonUp(0) && DraggedAreaLines[0] != null && !EventSystem.current.IsPointerOverGameObject() && (Mathf.Abs(pointA.x - hit.point.x) >= .001f && Mathf.Abs(pointA.z - hit.point.z) >= .001f))
		//                    {
		//                        for (int i = 0; i < DraggedAreaLines.Length; i++)
		//                        {
		//                            DraggedAreaLines[i].Enabled = false;
		//                            DraggedAreaLines[i].Height = BasementHeight;
		//                        }
		//                        lines.Clear();
		//                        lines.AddRange(DraggedAreaLines);
		//
		//
		//
		//                        DraggedAreaLines = new Line[4];
		//
		//                        regeneratePath(true);
		//                        UpdateEnableBasementButton();
		//                        for (int i = wallFaces.Count - 1; i >= 0; i--)
		//                        {
		//                            if (wallFaces[i].WallFaceType == WallFaceType.Inner)
		//                            {
		//                                wallFaces[i].Destroy();
		//                                wallFaces.RemoveAt(i);
		//                            }
		//                        }
		//                        GameObject.Destroy(upperWallFace);
		//                        upperWallFace = null;
		//
		//                        upperWallFace = new GameObject("upper wall face");
		//                        upperWallFace.AddComponent<MeshFilter>().mesh = GetOuterCeil();
		//                        upperWallFace.AddComponent<MeshRenderer>();
		//                        upperWallFace.AddComponent<MeshCollider>();
		//
		//                        for (int i = 0; i < floors.Count; i++)
		//                        {
		//                            GameObject.Destroy(floors[i]);
		//                            GameObject.Destroy(floorColliders[i]);
		//                        }
		//                        floors.Clear();
		//                        floorColliders.Clear();
		//
		//
		//                        GameObject.Destroy(Roof);Generate3DWallFacesFromLines
		//                        Roof = null;
		//
		//                        //						Basement.transform.localScale = new Vector3(Mathf.Abs(DraggedAreaLines[0].Vertices[0].x - DraggedAreaLines[0].Vertices[2].x), BasementHeight, Mathf.Abs(DraggedAreaLines[0].Vertices[0].z - DraggedAreaLines[0].Vertices[2].z));
		//                        //						Basement.transform.position = (DraggedAreaLines [0].Vertices [0] + DraggedAreaLines [0].Vertices [2]) * 0.5f + Vector3.up * BasementHeight * 0.5f;
		//                        //						Basement.SetActive (true);
		//                        pointASelected = false;
		//                    }
		//                }
		//            }
		//
		//        }
		// else
		{
			if (viewingMode == ViewingMode.Interior)
			{
				for (int i = 0; i < wallFaces.Count; i++)
				{
					if (!wallFaces[i].IsFacingCamera || wallFaces[i].WallFaceType == WallFaceType.Outer)
					{
						wallFaces[i].Wireframe = true;
						wallFaces[i].Solid = false;
						for (int j = 0; j < wallFaces[i].RelatedLine.Doors.Count; j++)
						{
							wallFaces[i].RelatedLine.Doors[j].Door.SetActive(false);
						}
						for (int j = 0; j < wallFaces[i].RelatedLine.Windows.Count; j++)
						{
							wallFaces[i].RelatedLine.Windows[j].Window.SetActive(false);
						}
						wallFaces[i].gameObject.GetComponent<Collider>().enabled = true;
					}
					else
					{
						wallFaces[i].Wireframe = false;
						wallFaces[i].Solid = true;
						for (int j = 0; j < wallFaces[i].RelatedLine.Doors.Count; j++)
						{
							wallFaces[i].RelatedLine.Doors[j].Door.SetActive(true);
						}
						for (int j = 0; j < wallFaces[i].RelatedLine.Windows.Count; j++)
						{
							wallFaces[i].RelatedLine.Windows[j].Window.SetActive(true);
						}
						wallFaces[i].gameObject.GetComponent<Collider>().enabled = true;

					}
				}
			}
			else
			{

				for (int i = 0; i < wallFaces.Count; i++)
				{
					wallFaces[i].Wireframe = false;
					wallFaces[i].Solid = true;
					for (int j = 0; j < wallFaces[i].RelatedLine.Doors.Count; j++)
					{
						wallFaces[i].RelatedLine.Doors[j].Door.SetActive(true);
					}
					for (int j = 0; j < wallFaces[i].RelatedLine.Windows.Count; j++)
					{
						wallFaces[i].RelatedLine.Windows[j].Window.SetActive(true);
					}
					if(wallFaces[i].gameObject!=null&& wallFaces[i].gameObject.GetComponent<Collider>()!=null)
						wallFaces[i].gameObject.GetComponent<Collider>().enabled = true;

				}
			}

			//if (!planningMode) 
			{

				switch (Mode)
				{
				case BuildingEditMode.None:
					{
						if (Input.GetMouseButtonDown(0))
						{

							if (SelectedItem == null) {

								WallFace wallface = getSelectedWallFace ();
								if (wallface != null) {
									//Select Wall
									selectedWallFace = wallface;
									Mode = BuildingEditMode.WallFaceSelected;
								}
							} else {
								// when click add window and doors WHEN EXTRIOR ONLY
								if (viewingMode == ViewingMode.Exterior) {
									for (int i = 0; i < floorColliders.Count; i++) {
										floorColliders [i].enabled = true;
									}
									// add wall matrial 
									if (SelectedItem.itemType == type.Wall) {
										WallFace wallface = getSelectedWallFace ();
										if (wallface != null) {
										}

									}

									if (SelectedItem.itemType == type.Window || SelectedItem.itemType == type.Door) {
										WallFace wallface = getSelectedWallFace ();
										if (wallface != null) {
											Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
											RaycastHit hit;
											if (Physics.Raycast (ray, out hit, float.MaxValue) && !EventSystem.current.IsPointerOverGameObject ()) {
												Vector2 location;
												Vector2? correctedLocation;
												if (wallface.RelatedLine.LocateItemInWall (hit.point, SelectedItem, out location, 100, out correctedLocation)) {
													if (tempObjectPlaceholder != null) {
														GameObject.Destroy (tempObjectPlaceholder);
														tempObjectPlaceholder = null;
													}

													if (SelectedItem.itemType == type.Window) {
														wallface.RelatedLine.Windows.Add (new WallWindow (wallface.RelatedLine, location, SelectedItem.prefabItem.Size.z, SelectedItem.prefabItem.Size.y, Instantiate (SelectedItem.prefabItem.gameObject)));
														regeneratePath (false);
													} else if (SelectedItem.itemType == type.Door) {
														wallface.RelatedLine.Doors.Add (new WallDoor (wallface.RelatedLine, location.x, SelectedItem.prefabItem.Size.z, SelectedItem.prefabItem.Size.y, Instantiate (SelectedItem.prefabItem.gameObject)));
														regeneratePath (false);
													}
												}
												//											else if (correctedLocation.HasValue) {
												//												if (SelectedItem.itemType == type.Window) {
												//													wallface.RelatedLine.Windows.Add (new WallWindow (wallface.RelatedLine, correctedLocation.Value, SelectedItem.prefabItem.Size.z, SelectedItem.prefabItem.Size.y, Instantiate (SelectedItem.prefabItem.gameObject)));
												//													regeneratePath (false);
												//												} else if (SelectedItem.itemType == type.Door) {
												//													wallface.RelatedLine.Doors.Add (new WallDoor (wallface.RelatedLine, correctedLocation.Value.x, SelectedItem.prefabItem.Size.z, SelectedItem.prefabItem.Size.y, Instantiate (SelectedItem.prefabItem.gameObject)));
												//													regeneratePath (false);
												//												}
												//											}
											}
										}
									} else { // not window and not door

										Vector3 location;
										Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
										RaycastHit hit;
										if (Physics.Raycast (ray, out hit, float.MaxValue) && !EventSystem.current.IsPointerOverGameObject ()) {
											location = hit.point - ray.direction * SelectedItem.prefabItem.Size.z * 0.5f;
											if (SelectedItem.alignToFloor) {
												RaycastHit floorHit;
												if (hit.collider.Raycast (new Ray (location, Vector3.down), out floorHit, float.MaxValue)) {
													Bounds aabb = new Bounds (floorHit.point + Vector3.up * SelectedItem.prefabItem.Size.y, SelectedItem.prefabItem.Size);

													Bounds? nAABB = alignToFloor (aabb, 10);
													if (nAABB.HasValue) {
														GameObject go = Instantiate (SelectedItem.prefabItem.gameObject);
														PrefabItem pItem = go.GetComponent<PrefabItem> ();
														Draggable draggable = go.AddComponent<Draggable> ();
														//													draggable.XEnabled = true;
														//													draggable.YEnabled = false;
														//													draggable.ZEnabled = true;
														//													draggable.XSnapDistance = 0;
														//													draggable.ZSnapDistance = 0;
														draggable.Enabled = true;
														draggable.StartMoving += delegate (GameObject sender, Vector3 oldPosition, Vector3 newPosition) {
															Bounds _aabb = new Bounds (newPosition + Vector3.up * pItem.Size.y, pItem.Size);
															Debug.Log ("start " + newPosition);
															Bounds? _nAABB = alignToFloor (_aabb, 10);
															if (_nAABB != null)
																sender.transform.position = newPosition;
														};
														draggable.Moving += delegate (GameObject sender, Vector3 oldPosition, Vector3 newPosition) {
															Bounds _aabb = new Bounds (newPosition + Vector3.up * pItem.Size.y, pItem.Size);

															Bounds? _nAABB = alignToFloor (_aabb, 10);
															if (_nAABB != null)
																sender.transform.position = newPosition;
														};
														draggable.EndMoving += delegate (GameObject sender, Vector3 oldPosition, Vector3 newPosition) {
															Bounds _aabb = new Bounds (newPosition + Vector3.up * pItem.Size.y, pItem.Size);
															Debug.Log ("end " + newPosition);

															Bounds? _nAABB = alignToFloor (_aabb, 10);
															if (_nAABB != null)
																sender.transform.position = newPosition;
														};


														go.transform.position = nAABB.Value.center;
														items.Add (go);
													}
												}
											}
										}
									}

									for (int i = 0; i < floorColliders.Count; i++) {
										floorColliders [i].enabled = false;
									}
								}
							}
						}
						else
						{

							if (SelectedItem == null)
							{

								//								WallFace wallface = getSelectedWallFace ();
								//								if (wallface != null) {
								//									selectedWallFace = wallface;
								//								}
							}
							else
							{

								if (viewingMode == ViewingMode.Exterior && !IsBasement) {
									// when click add window and doors

									for (int i = 0; i < floorColliders.Count; i++) {
										floorColliders [i].enabled = true;
									}

									if (SelectedItem.itemType == type.Window || SelectedItem.itemType == type.Door) {
										WallFace wallface = getSelectedWallFace ();
										if (wallface != null) {
											Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
											RaycastHit hit;
											if (Physics.Raycast (ray, out hit, float.MaxValue) && !EventSystem.current.IsPointerOverGameObject ()) {
												Vector2 location;
												Vector2? correctedLocation;
												//if (wallface.RelatedLine.LocateItemInWall (hit.point, SelectedItem, out location, 100, out correctedLocation)) 
												{

													if (tempObjectPlaceholder == null)
														tempObjectPlaceholder = Instantiate (SelectedItem.prefabItem.gameObject);
													if (wallface.RelatedLine.LocateItemInWall (hit.point, SelectedItem, out location, 100, out correctedLocation)) {
														MeshRenderer[] components = tempObjectPlaceholder.GetComponentsInChildren<MeshRenderer> ();
														for (int i = 0; i < components.Length; i++) {
															Material[] tempMaterial = new Material[components [i].materials.Length];

															for (int j = 0; j < tempMaterial.Length; j++)
																tempMaterial [j] = PlaceholderMaterial;
															components [i].materials = tempMaterial;
														}
													} else {
														MeshRenderer[] components = tempObjectPlaceholder.GetComponentsInChildren<MeshRenderer> ();
														for (int i = 0; i < components.Length; i++) {
															Material[] tempMaterial = new Material[components [i].materials.Length];

															for (int j = 0; j < tempMaterial.Length; j++)
																tempMaterial [j] = PlaceholderErrorMaterial;
															components [i].materials = tempMaterial;
														}
													}
													if (SelectedItem.itemType == type.Window) {
														new WallWindow (wallface.RelatedLine, location, SelectedItem.prefabItem.Size.z, SelectedItem.prefabItem.Size.y, tempObjectPlaceholder);
													} else if (SelectedItem.itemType == type.Door) {

														new WallDoor (wallface.RelatedLine, location.x, SelectedItem.prefabItem.Size.z, SelectedItem.prefabItem.Size.y, tempObjectPlaceholder);
													} else if (SelectedItem.itemType == type.Wall) {

													}
												}
												//											else if (correctedLocation.HasValue) {

												//											}
											}
										}
									}
								}///
							}///

						}
					}
					break;
				case BuildingEditMode.WallFaceSelected:
					{
						//DeSelect Wall
						if (Input.GetMouseButtonDown(0) && getSelectedWallFace() != null)
						{

							selectedWallFace = null;
						}
					}
					break;
				case BuildingEditMode.WallFaceMoving:
					{
					}
					break;
				}
			
				if (Input.GetMouseButtonDown (0)) {
					if (selectedWallFace != null)
					{
						cameraTarget = (selectedWallFace.a + selectedWallFace.b) * 0.5f + Vector3.up * selectedWallFace.Height * 0.5f;
					}
					if (Time.time - lastClickTime < DoubleClickCatchTime)
					{
						gameCamera.TargetObject = cameraTarget;
					}
					lastClickTime = Time.time;
				}  
			}
			//else 
			{

				if ((Mode == BuildingEditMode.None || Mode == BuildingEditMode.Drawing) && SelectedItem == null)
				{
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					Collider coll = GetComponent<MeshCollider>();
					RaycastHit hit = new RaycastHit();
					snapObject.SetActive(false);
					bool flag = false;
					List<RaycastHit> hits = new List<RaycastHit>(Physics.RaycastAll(ray));
					hits.Sort(delegate (RaycastHit x, RaycastHit y) {
						return x.distance.CompareTo(y.distance);
					});
					if (hits.Count > 0)
					{
						Vector3 firstSet = hits[0].point;
						for (int i = 0; i < hits.Count; i++)
						{
							if ((hits[i].point - firstSet).sqrMagnitude < 0.00001f)
							{
								if (hits[i].collider == coll)
								{
									flag = true;
									hit = hits[i];
									break;
								}

							}
							else
								break;

						}
					}
					if (flag && !EventSystem.current.IsPointerOverGameObject())
					{

						//if (coll.Raycast (ray, out hit, float.MaxValue)) {
						if (snapEnabled)
						{
							hit.point = snapToGrid(hit.point);
							//                      Debug.Log (hit.point);
						}

						if (verticesSelected.Count != 0)
						{
							for (int i = 0; i < verticesSelected.Count; i++)
							{
								if (verticesSelected[i] % 2 == 0)
									lines[verticesSelected[i] / 2].a = hit.point;
								else
									lines[verticesSelected[i] / 2].b = hit.point;
							}
						}



						if (DraggedLine != null)
							DraggedLine.b = hit.point;

						snapObject.SetActive(true);
						snapObject.transform.position = hit.point;

						if ((Input.GetMouseButtonDown(0))&& verticesSelected.Count == 0 )
						{
							

							if (!pointASelected)
							{
								if (IsBasement) {
									if (lines.Count == 0) {
											DraggedLine.Enabled = true;
											pointA = hit.point;
											DraggedLine.a = hit.point;
											DraggedLine.b = hit.point;
											pointASelected = true;


									} else {
										if (Line.isPointOverPath (lines, hit.point)) {
											DraggedLine.Enabled = true;
											pointA = hit.point;
											DraggedLine.a = hit.point;
											DraggedLine.b = hit.point;
											pointASelected = true;

										}
									}


								} else {

									DraggedLine.Enabled = true;
									pointA = hit.point;
									DraggedLine.a = hit.point;
									DraggedLine.b = hit.point;
									pointASelected = true;
								} 



							}
							else
							{

								pointB = hit.point;

								lines = Line.Split(lines, pointA);
								lines = Line.Split(lines, pointB);
								int id1 = lineVertices.FindIndex(delegate (Vector3 obj) {
									return (obj - pointA).sqrMagnitude <= 0.0001f;
								});
								int id2 = lineVertices.FindIndex(delegate (Vector3 obj) {
									return (obj - pointB).sqrMagnitude <= 0.0001f;
								});
								if (id1 == -1)
								{
									id1 = lineVertices.Count;
									lineVertices.Add(pointA);
								}
								if (id2 == -1)
								{
									id2 = lineVertices.Count;
									lineVertices.Add(pointB);
								}
								//regeneratePath(true);

								// draw the basemenet if true else draw the floors
								if (IsBasement) {
									if (Line.Is45Degree0r0Degree (DraggedLine)) {
										
										lines.Add (new Line (lineVertices, id1, id2, 0.1f, 0.1f, LineMaterial, DefaultInnerWallMaterial, DefaultOuterWallMaterial, DefaultSideMaterial));
										lines [lines.Count - 1].Height = BasementHeight;
										lines [lines.Count - 1].Parent = this.transform;
										pointASelected = false;
										DraggedLine.Enabled = false;
										Debug.Log ("isbasement");
									}
									
								} 
								else {
									if (Line.Is45Degree0r0Degree (DraggedLine)) {
										
									lines.Add(new Line(lineVertices, id1, id2, 0.1f, 0.1f, LineMaterial, DefaultInnerWallMaterial, DefaultOuterWallMaterial, DefaultSideMaterial));
									lines[lines.Count - 1].Height = Height;
									lines[lines.Count - 1].Parent = this.transform;
									pointASelected = false;
									DraggedLine.Enabled = false;

									} 
								}
								for (int i = 0; i < lines.Count; i++)
								{
									lines[i].Enabled = false;
								}
								GameObject.Destroy (upperWallFace);
								upperWallFace = null;								
								/// ///// code updated on jun10 
								regeneratePath(true);
							}
						}
						else if (Input.GetMouseButtonDown(1) || (Input.GetMouseButton(0) && verticesSelected.Count != 0))
						{


							if (verticesSelected.Count == 0)
							{
								for (int i = 0; i < lines.Count; i++)
								{
									if (hit.point == lines[i].a)
									{
										verticesSelected.Add(i * 2);
									}
									if (hit.point == lines[i].b)
									{
										verticesSelected.Add(i * 2 + 1);
									}
								}
							}
							else
							{

								verticesSelected.Clear();
							}
							DraggedLine.Enabled = false;
							pointASelected = false;
						}

					}

					hit.ToString();
				}


			}
		}
//		Draggable.lines = lines;

	}

	void OnDisable()
	{
		if (!gameObject.activeSelf)
		{
			for (int i = 0; i < lines.Count; i++)
			{
				for (int j = 0; j < lines[i].Windows.Count; j++)
				{
					lines[i].Windows[j].Window.SetActive(false);
				}


				for (int j = 0; j < lines[i].Doors.Count; j++)
				{
					lines[i].Doors[j].Door.SetActive(false);
				}
			}
		}
	}

	void OnEnable()
	{
		if (gameObject.activeSelf)
		{
			for (int i = 0; i < lines.Count; i++)
			{
				for (int j = 0; j < lines[i].Windows.Count; j++)
				{
					lines[i].Windows[j].Window.SetActive(true);
				}

				for (int j = 0; j < lines[i].Doors.Count; j++)
				{
					lines[i].Doors[j].Door.SetActive(true);
				}
			}
		}
	}

	WallFace getSelectedWallFace()
	{
		float dst = float.MaxValue;
		WallFace selectedFace = null;
		for (int i = 0; i < wallFaces.Count; i++)
		{
			float det;
			if (wallFaces[i].IsMouseOver(out det))
			{
				if (det < dst)
				{
					dst = det;
					selectedFace = wallFaces[i];
				}
			}
		}

		return selectedFace;
	}

	public Mesh GetCeil()
	{
		//		if (IsBasement) {
		//			GameObject quad = GameObject.CreatePrimitive (PrimitiveType.Quad);
		//			quad.transform.
		//		} else {
		Mesh m = new Mesh();
		List<CombineInstance> meshes = new List<CombineInstance>();
		for (int i = 0; i < floors.Count; i++)
		{
			CombineInstance ci = new CombineInstance();
			ci.mesh = floors[i].GetComponent<MeshFilter>().mesh;
			ci.transform = Matrix4x4.identity;
			meshes.Add(ci);
		}
		m.CombineMeshes(meshes.ToArray());

		Vector3[] verts = new Vector3[m.vertices.Length];
		m.vertices.CopyTo(verts, 0);

		for (int i = 0; i < verts.Length; i++)
		{
			verts[i] += Vector3.up * lines[0].Height;
		}
		m.vertices = verts;

		return m;
		//		}
	}

	public Mesh GetOuterCeil()
	{
		List<Line> outer = new List<Line>();
		List<Vector3> verts = new List<Vector3>();
		for (int i = 0; i < wallFaces.Count; i++)
		{
			if (wallFaces[i].WallFaceType == WallFaceType.Outer)
			{

				verts.Add(wallFaces[i].a);
				verts.Add(wallFaces[i].b);
				outer.Add(new Line(verts, verts.Count - 2, verts.Count - 1, 0.05f, 0.05f, null, null, null, null));
			}
		}
		Line.WeldVertices(outer);
		Line.OptimizePath(ref outer);

		List<int> triangles;
		List<Vector2> uvs;
		List<Vector3> normals;
		Line.FillCap(outer, out triangles, out verts, out uvs, out normals);
		for (int i = 0; i < verts.Count; i++)
		{
			verts[i] += wallFaces[0].Height * Vector3.up;
		}
		Mesh MeshCap = new Mesh();
		MeshCap.vertices = verts.ToArray();
		MeshCap.uv = uvs.ToArray();
		MeshCap.normals = normals.ToArray();
		MeshCap.triangles = triangles.ToArray();
		return MeshCap;
	}


	//List<Vector3> vlines = new List<Vector3>();
	bool snap(Vector3 pos, float maxlength, out Vector3 nearest)
	{
		//snap to line vertices
		nearest = pos;
		float len = float.MaxValue;
		for (int i = 0; i < lines.Count; ++i)
		{
			{
				float det = Vector3.Distance(pos, lines[i].a);
				if (det < len)
				{
					len = det;
					nearest = lines[i].a;
				}
			}
			{
				float det = Vector3.Distance(pos, lines[i].b);
				if (det < len)
				{
					len = det;
					nearest = lines[i].b;
				}
			}
		}

		return len < maxlength;
	}

	Vector3 snapToGrid(Vector3 pos)
	{
		int divx = (int)((pos.x > 0 ? pos.x + 0.5f * snapGridDistance : pos.x - 0.5f * snapGridDistance) / snapGridDistance);
		divx *= snapGridDistance;//99999999
		int divy = (int)((pos.y > 0 ? pos.y + 0.5f * snapGridDistance : pos.y - 0.5f * snapGridDistance) / snapGridDistance);
		divy *= snapGridDistance;
		int divz = (int)((pos.z > 0 ? pos.z + 0.5f * snapGridDistance : pos.z - 0.5f * snapGridDistance) / snapGridDistance);
		divz *= snapGridDistance;

		pos.x = divx;
		//pos.y = divy;
		pos.z = divz;

		return pos;
	}
	/// <summary>
	/// Regenerates the path.
	/// </summary>
	/// <param name="optimize">If set to <c>true</c> optimize.</param>
	public void regeneratePath(bool optimize)
	{

		// we add the drawing code for th basement here 
		Vector3 _selectedWallFaceA = Vector3.zero, _selectedWallFaceB = Vector3.zero;
		if (_selectedWallFace != null)
		{
			_selectedWallFaceA = _selectedWallFace.a;
			_selectedWallFaceB = _selectedWallFace.b;
		}

		for (int i = 0; i < wallFaces.Count; i++)
		{
			wallFaces[i].Destroy();
		}
		wallFaces.Clear();
		GameObject.Destroy(upperWallFace);
		upperWallFace = null;

		List<WallFace> outerWall;
		List<WallFace> doorSides;
		List<WallFace> innerWall;


		List<Mesh> floors;


		if (optimize)
		{
			Line.OptimizePath(ref lines);
		}

		Line.Generate3DWallFacesFromLines(viewingMode == ViewingMode.Interior,lines, WallWireframeMaterial, WallSelectedMaterial, out outerWall, out doorSides, out innerWall, out upperWallFace, out floors);




		try
		{

			//		gggg (lines, WallWireframeMaterial, WallSelectedMaterial, out outerWall, out doorSides, out innerWall, out upperWallFace, out floors);


			for (int i = 0; i < this.floors.Count; i++)
			{
				GameObject.Destroy(this.floors[i]);
			}

			this.floors.Clear();
			this.floorColliders.Clear();

			for (int i = 0; i < floors.Count; i++)
			{
				GameObject floor = new GameObject("Room" + i.ToString() + "Floor");
				floor.transform.position += Vector3.up * 0.001f;
				floor.AddComponent<MeshFilter>().mesh = floors[i];
				floor.AddComponent<MeshRenderer>().material = DefaultFloorMaterial;
				floorColliders.Add(floor.AddComponent<MeshCollider>());
				floorColliders[i].enabled = false;
				floor.transform.parent = this.transform;
				this.floors.Add(floor);
			}


			if (Roof != null)
			{
				GameObject.Destroy(Roof);
				Roof = null;
			}

			if (roofEnabled)
			{
				Roof = new GameObject("roof");
				Roof.AddComponent<Roof>().CreateFromLines(lines, 0.4f, 0.4f);
				Roof.transform.parent = transform;
				Roof.GetComponent<MeshRenderer>().material = DefaultRoofMaterial;
				if (_viewingMode == ViewingMode.Interior)
					Roof.SetActive(false);
			}
		}
		catch
		{
		}

		for (int i = 0; i < outerWall.Count; i++)
		{
			if (isCeil)
				outerWall[i].WallMaterial = CeilMaterial;
			wallFaces.Add(outerWall[i]);
			wallFaces[wallFaces.Count - 1].Parent = transform;
			wallFaces[wallFaces.Count - 1].Wireframe = !wallFaces[wallFaces.Count - 1].IsFacingCamera;
		}
		// this for loop used to fill the innerwals 
		for (int i = 0; i < innerWall.Count; i++)
		{
			wallFaces.Add(innerWall[i]);
			wallFaces[wallFaces.Count - 1].Parent = transform;
			wallFaces[wallFaces.Count - 1].Wireframe = !wallFaces[wallFaces.Count - 1].IsFacingCamera;
		}

		for (int i = 0; i < doorSides.Count; i++)
		{
			wallFaces.Add(doorSides[i]);
			wallFaces[wallFaces.Count - 1].Parent = transform;
			wallFaces[wallFaces.Count - 1].Wireframe = !wallFaces[wallFaces.Count - 1].IsFacingCamera;
		}

		if (_selectedWallFace != null)
		{
			float dst = float.MaxValue;
			for (int i = 0; i < wallFaces.Count; i++)
			{
				float det1 = (wallFaces[i].a - _selectedWallFaceA).sqrMagnitude + (wallFaces[i].b - _selectedWallFaceB).sqrMagnitude;
				float det2 = (wallFaces[i].b - _selectedWallFaceA).sqrMagnitude + (wallFaces[i].a - _selectedWallFaceB).sqrMagnitude;
				if (det1 < dst)
				{
					_selectedWallFace = wallFaces[i];
					dst = det1;
				}
				if (det2 < dst)
				{
					_selectedWallFace = wallFaces[i];
					dst = det2;
				}
			}
			_selectedWallFace.Selected = true;
		}
		/// the drawing code of the basemenet 
		if (IsBasement) {
			Mesh m = null;
			try {
				m = GetOuterCeil ();

			} catch {
			}

			if (m != null) {

				GameObject.Destroy (upperWallFace);
				upperWallFace = null;
				upperWallFace = new GameObject("upper wall face");

				upperWallFace.AddComponent<MeshFilter>().mesh = m;
				upperWallFace.AddComponent<MeshRenderer>();
				upperWallFace.AddComponent<MeshCollider>();
				BasementButton.interactable = true;
			}
			else 
				BasementButton.interactable = false;



			for (int i = wallFaces.Count - 1; i >= 0; i--)
			{
				if (wallFaces[i].WallFaceType == WallFaceType.Inner)
				{
					wallFaces[i].Destroy();
					wallFaces.RemoveAt(i);
				}
			}
			for (int i = 0; i < floors.Count; i++)
			{
				GameObject.Destroy(floors[i]);
				GameObject.Destroy(floorColliders[i]);
			}
			floors.Clear();
			floorColliders.Clear();


			GameObject.Destroy(Roof);
			Roof = null;
		}
	}
	public void WireFrameWallViewInterior()
	{
		//haytham
		viewingMode = ViewingMode.Interior;

	}

	public void WireFrameWallViewExterior()
	{
		viewingMode = ViewingMode.Exterior;
	}

	public void SetRoofMaterial(Material Mat)
	{
		if (Mat != null)
			Roof.GetComponent<MeshRenderer>().material = Mat;
		DefaultRoofMaterial = Mat;
	}


	public void SetOuterWallMaterial(Material Mat)
	{
		if (Mat != null)
		{
			if (_selectedWallFace .WallFaceType == WallFaceType.Outer) {
				Debug.Log ("ChangeOutterWallMaterial ");
				SetSelectedWallFaceMaterials(DefaultInnerWallMaterial, Mat, DefaultSideMaterial);
			}
		}
	}

	public void SetInnerWallMaterial(Material Mat)
	{
		if (Mat != null)
		{
			if (_selectedWallFace .WallFaceType == WallFaceType.Outer) {
				Debug.Log ("ChangeInnerWallMaterial ");
				SetSelectedWallFaceMaterials(Mat,DefaultOuterWallMaterial, DefaultSideMaterial);
			}
		}
	}


	/* public void GetWallArea()
     {
         float wallSum = 0;
         wallSum += (selectedWallFace.RelatedLine.a - selectedWallFace.RelatedLine.b).magnitude;
         wallSum *= selectedWallFace.Height;
         Debug.Log("the area of the wall = " + wallSum + " M");
     }

     public void GetAllWindowArea()
     {
         float windowSum = 0;
         float h, w;
         for (int i = 0; i < selectedWallFace.RelatedLine.Windows.Count; i++)
         {
             h = selectedWallFace.RelatedLine.Windows[i].WindowHeight;
             w = selectedWallFace.RelatedLine.Windows[i].WindowWidth;
             windowSum += (h * w);
         }
         Debug.Log("The area of all windows at this wall =" + windowSum);
     }


     public void GetAllDoorArea()
     {
         float doorSum = 0;
         float h, w;
         for (int i = 0; i < selectedWallFace.RelatedLine.Doors.Count; i++)
         {
             h = selectedWallFace.RelatedLine.Doors[i].DoorHeight;
             w = selectedWallFace.RelatedLine.Doors[i].DoorWidth;
             doorSum += (h * w);
         }
         Debug.Log("The area of all doors at this wall =" + doorSum);
     }


     public void GetWallThickness()
     {
         float th = selectedWallFace.RelatedLine.Thickness;
         Debug.Log("the thickness of this wall =" + th);
     }
     public void ClearSelection()
     {
         selectedWallFace = null;
     }*/
	public float returnTotalWallsArea()
	{
		float sum = 0.0f;

		for (int i = 0; i < lines.Count; i++)
		{
			if (IsBasement)
			{
				continue;
			}
			sum += (lines[i].a - lines[i].b).magnitude * lines[i].Height;
		}
		return sum;
	}

	public void GetWallArea()//selected
	{
		float wallSum = 0;
		wallSum += (selectedWallFace.RelatedLine.a - selectedWallFace.RelatedLine.b).magnitude;
		wallSum *= selectedWallFace.Height;
		Debug.Log("the area of the wall = " + wallSum + " M");
	}

	public float returnWallArea()
	{
		float wallSum = 0;
		if (selectedWallFace != null) { 
			wallSum += (selectedWallFace.RelatedLine.a - selectedWallFace.RelatedLine.b).magnitude;
			wallSum *= selectedWallFace.Height;
		}
		return wallSum;
	}

	public void GetAllWindowArea()
	{
		float windowSum = 0;
		float h, w;
		for (int i = 0; i < selectedWallFace.RelatedLine.Windows.Count; i++)
		{
			h = selectedWallFace.RelatedLine.Windows[i].WindowHeight;
			w = selectedWallFace.RelatedLine.Windows[i].WindowWidth;
			windowSum += (h * w);
		}
		Debug.Log("The area of all windows at this wall =" + windowSum);
	}

	public float returnAllWindowArea()
	{
		float windowSum = 0;
		float h, w;
		for (int j = 0; j < lines.Count; j++)
		{
			for (int i = 0; i < lines[j].Windows.Count; i++)
			{
				h = lines[j].Windows[i].WindowHeight;
				w = lines[j].Windows[i].WindowWidth;
				windowSum += (h * w);
			}
		}

		return windowSum;
	}

	public void GetSelectedDoorArea()
	{
		float doorSum = 0;
		float h, w;
		for (int i = 0; i < selectedWallFace.RelatedLine.Doors.Count; i++)
		{
			h = selectedWallFace.RelatedLine.Doors[i].DoorHeight;
			w = selectedWallFace.RelatedLine.Doors[i].DoorWidth;
			doorSum += (h * w);
		}
		Debug.Log("The area of all doors at this wall =" + doorSum);
	}


	public float returnAllDoorArea()
	{
		float doorSum = 0;
		float h, w;
		for (int j = 0; j < lines.Count; j++)
		{
			for (int i = 0; i < lines[j].Doors.Count; i++)
			{
				h = lines[j].Doors[i].DoorHeight;
				w = lines[j].Doors[i].DoorWidth;
				doorSum += (h * w);
			}
		}
		return doorSum;
	}
	//problem !!!!!
	public double returnRoofArea()
	{
		double roofArea = 0;
		Vector3 a, b, c;//fistpoint, secpoint, thirpoint;
		Mesh mesh = Roof.GetComponent<MeshFilter>().mesh;
		//    Mesh mesh = GetCeil();
		Vector3[] vert = mesh.vertices;
		int[] ind = mesh.GetIndices(0);
		for (int i = 0; i < vert.Length; i += 3)
		{
			a = vert[ind[i]];
			b = vert[ind[i + 1]];
			c = vert[ind[i + 2]];
			roofArea += 0.5 * (Mathf.Abs((a.x - c.x) * (b.z - a.z) - (a.x - b.x) * (c.z - a.z)));
		}
		return roofArea;
	}

	public void setRoofDepth()
	{
		RoofDepth = float.Parse(roofDepth.GetComponent<InputField>().text);
	}

	public void GetRoofDepth()
	{
		roofDepth.text = RoofDepth.ToString();
	}


	public void setWallThickness()
	{
		// float th = PublicThick;//selectedWallFace.RelatedLine.Thickness;
		//  wallThick.text = th.ToString();
		//wallThicktxt.text = PublicThick.ToString();

	}


	/* public void getWallThickness()//selected
     {
         //selectedWallFace.RelatedLine.Thickness
             PublicThick = float.Parse(wallThick.GetComponent<InputField>().text);
         print("hello");

     }*/
	public void ClearSelection()
	{
		selectedWallFace = null;
	}

	//------------------------------------------------------------------------------------------------------------------
	//------------------------------------------------------------------------------------------------------------------
	//==================================================================================================================
	//The function below - SetInsThicknessFromInputField() - sets the thickness to the Insulation Layer inside the wall
	//==================================================================================================================
	public void SetInsThicknessFromInputField()
	{
		try
		{
			PublicFloatInsThick = float.Parse(valueIns.text);//Get the value of Insulation Thickness from InputField
			//Try and catch were used to deal with exceptions like: Unacceptable values as letters and so on.
			if (PublicFloatInsThick > 0.05f)//The thickness of insulation doesn't exceed specific value
			{
				PublicFloatInsThick = 0.01f;//if it exceeds specific value (it will be set automatically to 0.01)
			}
		}
		catch (System.Exception ex)
		{
			PublicFloatInsThick = 0.01f;//for Unacceptable vlaues set InsulationThickness to 0.01.
		}

		//Nothing of insulation Thickness Will be changed until modifying values of lines for Wall
		//Values are existed in array Lines
		//This array has the lines of all walls 
		//We need to modify The thickness for each line.
		for (int i = 0; i < lines.Count; i++)
		{
			lines[i].InsulationThickness = PublicFloatInsThick;
		}
		Debug.Log("Insulation Thickness is: " + PublicFloatInsThick);
		regeneratePath(true);//To repaint Walls To apply Changes.
	}
	//-----------------------------------------------------------------------------------------------------------------------------
	//-----------------------------------------------------------------------------------------------------------------------------
	public void SetWallThicknessFromInputField()
	{
		try
		{
			PublicFloatWallThick = float.Parse(valueWall.text);//Get the value of Wall Thickness from InputField
			//Try and catch were used to deal with exceptions like: Unacceptable values as letters and so on.
			if (PublicFloatWallThick > 0.5f)//The thickness of Wall doesn't exceed specific value
			{
				PublicFloatWallThick = 0.1f;//if it exceeds specific value (it will be set automatically to 0.1)
			}
		}
		catch (System.Exception ex)
		{
			Debug.Log("sorry");
			PublicFloatWallThick = 0.1f;//for Unacceptable vlaues set InsulationThickness to 0.01.
		}

		//Nothing of Wall Thickness Will be changed until modifying values of lines for Wall
		//Values are existed in array Lines
		//This array has the lines of all walls 
		//We need to modify The thickness for each line.
		for (int i = 0; i < lines.Count; i++)
		{
			lines[i].WallThickness = PublicFloatWallThick;
		}
		Debug.Log("Wall Thickness is: " + PublicFloatWallThick);
		regeneratePath(true);//To repaint Walls To apply Changes.
	}
	//-----------------------------------------------------------------------------------------------------------------------------
	//-----------------------------------------------------------------------------------------------------------------------------
}