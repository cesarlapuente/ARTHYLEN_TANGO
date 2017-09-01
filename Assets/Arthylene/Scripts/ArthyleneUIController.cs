using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Tango;

public class ArthyleneUIController : MonoBehaviour, ITangoLifecycle, ITangoEvent, ITangoPose, ITangoDepth
{
	
	private const string AREA_DESCRIPTION_FILE_NAME = "ADF_arthylene_produce_department";


	/// <summary>
	/// The point cloud object in the scene.
	/// </summary>
	public TangoPointCloud m_pointCloud;


	/// <summary>
	/// Prefabs of different produces (fruit & vegetable).
	/// </summary>
	public GameObject[] m_producePrefabs;


	/// <summary>
	/// Main menu panel game object.
	/// 
	/// The panel will be disabled when any options starts.
	/// </summary>
	public GameObject m_panelMainMenu;

	public Button m_buttonPlace;
	public Button m_buttonSee;


	/// <summary>
	/// Scan menu panel game object.
	/// 
	/// The panel will be enabled when 'Scan produce department' starts.
	/// </summary>
	public GameObject m_panelScanMenu;

	public Button m_buttonSaveScan;
	public Text m_textButtonSaveScan;


	/// <summary>
	/// Place menu (side) panel game object.
	/// 
	/// The panel will be enabled when 'Place produce' starts.
	/// </summary>
	public GameObject m_panelPlaceMenuSide;


	/// <summary>
	/// PlaceMenuSide animator component.
	/// 
	/// The animator will start animations SlideIn and SlideOut.
	/// </summary>
	public Animator m_animPlaceMenuSide;
	private bool isPlaceMenuOpen;


	/// <summary>
	/// A reference to TangoApplication instance.
	/// </summary>
	private TangoApplication m_tangoApplication;


	/// <summary>
	/// The reference of the TangoPoseController object.
	/// 
	/// TangoPoseController listens to pose updates and applies the correct pose to itself and its built-in camera.
	/// </summary>
	public TangoPoseController m_poseController;


	/// <summary>
	/// The Area Description saved/loaded in the Tango Service.
	/// </summary>
	private AreaDescription m_areaDescription;


	/// <summary>
	/// If set, then the depth camera is on and we are waiting for the next depth update.
	/// </summary>
	private bool m_findPlaneWaitingForDepth;


	/// <summary>
	/// If the interaction is initialized.
	/// 
	/// Note that the initialization is triggered by the relocalization event. We don't want user to place object before
	/// the device is relocalized.
	/// </summary>
	private bool m_initialized = false;


	/// <summary>
	/// Reference to the newly placed produce.
	/// </summary>
	private GameObject newProduceObject = null;


	/// <summary>
	/// List of produces placed in the scene.
	/// </summary>
	private List<GameObject> m_produceList = new List<GameObject>();


	/// <summary>
	/// Current produce type.
	/// </summary>
	private int m_currentProduceType = 0;


	private string m_areaDescriptionUUID;

	private Thread m_saveThread;


	/// <summary>
	/// Unity Start function.
	/// 
	/// This function is responsible for connecting callbacks, set up TangoApplication and initialize the data list.
	/// </summary>
	void Start () 
	{
		// The PlaceMenu start closed.
		isPlaceMenuOpen = false;

		// Tango Initialization
		m_tangoApplication = FindObjectOfType<TangoApplication>();

		if (m_tangoApplication != null)
		{
			m_tangoApplication.Register(this);
			if (AndroidHelper.IsTangoCorePresent())
			{
				m_tangoApplication.RequestPermissions();
			}
		}
		else
		{
			Debug.Log("No Tango Manager found in scene.");
		}
	}

	
	// Update is called once per frame
	void Update () 
	{
		if (m_saveThread != null && m_saveThread.ThreadState != ThreadState.Running)
		{
			// After saving the scan, we reload the scene.
			#pragma warning disable 618
			Application.LoadLevel(Application.loadedLevel);
			#pragma warning restore 618
		}

		if (!m_initialized)
		{
			return;
		}

		if (Input.touchCount == 1)
		{
			Touch t = Input.GetTouch(0);
			Vector2 guiPosition = new Vector2(t.position.x, Screen.height - t.position.y);
			Camera cam = Camera.main;
			RaycastHit hitInfo;

			if (t.phase != TouchPhase.Began)
			{
				return;
			}

			StartCoroutine(_WaitForDepthAndFindPlane(t.position));
		}
	}


	// 
	public void StartPlace()
	{
		m_panelMainMenu.SetActive(false);
		m_panelPlaceMenuSide.SetActive(true);

		// Check that Area Description has been found
		if (!string.IsNullOrEmpty(m_areaDescriptionUUID))
		{
			m_areaDescription = AreaDescription.ForUUID(m_areaDescriptionUUID);
			m_tangoApplication.m_areaDescriptionLearningMode = false;

			m_tangoApplication.Startup(m_areaDescription);
			m_poseController.gameObject.SetActive(true);
		}
	}


	public void StartScan()
	{
		m_panelMainMenu.SetActive(false);
		m_panelScanMenu.SetActive(true);


		// If there is already one ADF we delete it.
		if (!string.IsNullOrEmpty(m_areaDescriptionUUID))
		{
			// Load up an existing Area Description.
			AreaDescription areaDescription = AreaDescription.ForUUID(m_areaDescriptionUUID);
			areaDescription.Delete();
		}
		m_areaDescriptionUUID = null;

		m_tangoApplication.m_areaDescriptionLearningMode = true;
		m_tangoApplication.Startup(null);

		m_poseController.gameObject.SetActive(true);
	}


	/// <summary>
	/// Save the Area Description.
	/// </summary>
	public void SaveScan()
	{
		// Disable interaction before saving by removing PanelScanMenu.
		m_buttonSaveScan.interactable = false;
		m_initialized = false;

		// Check if Area Description Learning mode was ON (it should be)
		if (m_tangoApplication.m_areaDescriptionLearningMode)
		{
			m_saveThread = new Thread(delegate()
				{
					// Start saving process in another thread.
					m_areaDescription = AreaDescription.SaveCurrent();
					AreaDescription.Metadata metadata = m_areaDescription.GetMetadata();
					metadata.m_name = AREA_DESCRIPTION_FILE_NAME;
					m_areaDescription.SaveMetadata(metadata);
				});
			m_saveThread.Start();
		}
	}


	public void TogglePlaceMenu()
	{
		// if the PlaceMenu is already open then we close it
		if (isPlaceMenuOpen) 
		{
			PlaceMenuSlideOut();
		}
		// otherwise we open it.
		else
		{
			PlaceMenuSlideIn();
		}

		// change the state of the PlaceMenu
		isPlaceMenuOpen = !isPlaceMenuOpen;
	}


	private void PlaceMenuSlideIn()
	{
		// We have to enabled it, because it is at first disabled to avoid auto start
		m_animPlaceMenuSide.enabled = true;
		m_animPlaceMenuSide.Play("PlaceMenuSlideIn");
	}


	private void PlaceMenuSlideOut()
	{
		m_animPlaceMenuSide.Play("PlaceMenuSlideOut");
	}


	/// <summary>
	/// Wait for the next depth update, then find the plane at the touch position.
	/// </summary>
	/// <returns>Coroutine IEnumerator.</returns>
	/// <param name="touchPosition">Touch position to find a plane at.</param>
	private IEnumerator _WaitForDepthAndFindPlane(Vector2 touchPosition)
	{
		m_findPlaneWaitingForDepth = true;

		// Turn on the camera and wait for a single depth update.
		m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);
		while (m_findPlaneWaitingForDepth)
		{
			yield return null;
		}

		m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);

		// Find the plane.
		Camera cam = Camera.main;
		Vector3 planeCenter;
		Plane plane;
		if (!m_pointCloud.FindPlane(cam, touchPosition, out planeCenter, out plane))
		{
			yield break;
		}

		// Ensure the location is always facing the camera.  This is like a LookRotation, but for the Y axis.
		Vector3 up = plane.normal;
		Vector3 forward;
		if (Vector3.Angle(plane.normal, cam.transform.forward) < 175)
		{
			Vector3 right = Vector3.Cross(up, cam.transform.forward).normalized;
			forward = Vector3.Cross(right, up).normalized;
		}
		else
		{
			// Normal is nearly parallel to camera look direction, the cross product would have too much
			// floating point error in it.
			forward = Vector3.Cross(up, cam.transform.right);
		}


		// Instantiate produce object.
		newProduceObject = Instantiate(m_producePrefabs[m_currentProduceType],
			planeCenter,
			Quaternion.LookRotation(forward, up)) as GameObject;

		ARProduce produceScript = newProduceObject.GetComponent<ARProduce>();

		produceScript.m_type = m_currentProduceType;
		produceScript.m_timestamp = (float)m_poseController.LastPoseTimestamp;

		Matrix4x4 uwTDevice = Matrix4x4.TRS(m_poseController.transform.position,
			m_poseController.transform.rotation,
			Vector3.one);
		Matrix4x4 uwTProduce = Matrix4x4.TRS(newProduceObject.transform.position,
			newProduceObject.transform.rotation,
			Vector3.one);
		produceScript.m_deviceTProduce = Matrix4x4.Inverse(uwTDevice) * uwTProduce;

		m_produceList.Add(newProduceObject);
	}


	/*
	 * Implements
	 */


	/// <summary>
	/// ITangoLifecycle - Internal callback when a permissions event happens.
	/// </summary>
	/// <param name="permissionsGranted">If set to <c>true</c> permissions granted.</param>
	public void OnTangoPermissions(bool permissionsGranted)
	{
		if (permissionsGranted)
		{
			m_areaDescriptionUUID = TangoUtils.GetAreaDescriptionUUIDbyName(AREA_DESCRIPTION_FILE_NAME);
			if (string.IsNullOrEmpty(m_areaDescriptionUUID))
			{
				// m_buttonPlace.interactable = false;
				m_buttonSee.interactable = false;
			}
		}
		else
		{
			AndroidHelper.ShowAndroidToastMessage("Motion Tracking and Area Learning Permissions Needed");

			// This is a fix for a lifecycle issue where calling
			// Application.Quit() here, and restarting the application
			// immediately results in a deadlocked app.
			AndroidHelper.AndroidQuit();
		}
	}

	/// <summary>
	/// ITangoLifecycle - This is called when successfully connected to the Tango service.
	/// </summary>
	public void OnTangoServiceConnected()
	{
	}

	/// <summary>
	/// ITangoLifecycle - This is called when disconnected from the Tango service.
	/// </summary>
	public void OnTangoServiceDisconnected()
	{
	}


	/// <summary>
	/// ITangoEvent - This is called each time a Tango event happens.
	/// </summary>
	/// <param name="tangoEvent">Tango event.</param>
	public void OnTangoEventAvailableEventHandler(Tango.TangoEvent tangoEvent)
	{
		// We will not have the saving progress when the learning mode is off.
		if (!m_tangoApplication.m_areaDescriptionLearningMode)
		{
			return;
		}

		if (tangoEvent.type == TangoEnums.TangoEventType.TANGO_EVENT_AREA_LEARNING
			&& tangoEvent.event_key == "AreaDescriptionSaveProgress")
		{
			m_textButtonSaveScan.text = "Saving. " + (float.Parse(tangoEvent.event_value) * 100) + "%";
		}
	}


	/// <summary>
	/// ITangoPose - OnTangoPoseAvailable event from Tango.
	/// 
	/// In this function, we only listen to the Start-Of-Service with respect to Area-Description frame pair. This pair
	/// indicates a relocalization or loop closure event happened, base on that, we either start the initialize the
	/// interaction or do a bundle adjustment for all produce position.
	/// </summary>
	/// <param name="poseData">Returned pose data from TangoService.</param>
	public void OnTangoPoseAvailable(Tango.TangoPoseData poseData)
	{
		// This frame pair's callback indicates that a loop closure or relocalization has happened. 
		//
		// When learning mode is on, this callback indicates the loop closure event. Loop closure will happen when the
		// system recognizes a pre-visited area, the loop closure operation will correct the previously saved pose 
		// to achieve more accurate result. (pose can be queried through GetPoseAtTime based on previously saved
		// timestamp).
		// Loop closure definition: https://en.wikipedia.org/wiki/Simultaneous_localization_and_mapping#Loop_closure
		//
		// When learning mode is off, and an Area Description is loaded, this callback indicates a
		// relocalization event. Relocalization is when the device finds out where it is with respect to the loaded
		// Area Description. In our case, when the device is relocalized, the produces will be loaded because we
		// know the relative device location to the produce.
		if (poseData.framePair.baseFrame == 
			TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION &&
			poseData.framePair.targetFrame ==
			TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE &&
			poseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID)
		{
			// When we get the first loop closure/relocalization event, we initialized all the in-game interactions.
			if (!m_initialized)
			{
				m_initialized = true;
				if (m_areaDescription == null)
				{
					Debug.Log("AndroidInGameController.OnTangoPoseAvailable(): m_areaDescription is null");
					return;
				}
			}
		}
	}

	/// <summary>
	/// ITangoDepth - This is called each time new depth data is available.
	/// 
	/// On the Tango tablet, the depth callback occurs at 5 Hz.
	/// </summary>
	/// <param name="tangoDepth">Tango depth.</param>
	public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
	{
		// Don't handle depth here because the PointCloud may not have been updated yet. Just
		// tell the coroutine it can continue.
		m_findPlaneWaitingForDepth = false;
	}
}