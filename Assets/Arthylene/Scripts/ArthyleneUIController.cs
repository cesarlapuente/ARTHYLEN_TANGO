using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Tango;

public class ArthyleneUIController : MonoBehaviour, ITangoLifecycle, ITangoEvent
{
	
	private const string AREA_DESCRIPTION_FILE_NAME = "ADF_arthylene_produce_department";


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
	}


	// 
	public void StartPlace()
	{
		m_panelMainMenu.SetActive(false);
		m_panelPlaceMenuSide.SetActive(true);
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
}