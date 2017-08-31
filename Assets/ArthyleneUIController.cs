using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArthyleneUIController : MonoBehaviour 
{
	/// <summary>
	/// Main menu panel game object.
	/// 
	/// The panel will be disabled when any options starts.
	/// </summary>
	public GameObject m_panelMainMenu;

	/// <summary>
	/// Place menu (side) panel game object.
	/// 
	/// The panel will be enabled when 'Place produce' starts.
	/// </summary>
	public GameObject m_panelPlaceMenuSide;

	private Animator m_animPlaceMenuSlide;
	private bool isPlaceMenuOpen;


	// Use this for initialization
	void Start () 
	{
		isPlaceMenuOpen = false;

		// Get the animator component from the PanelPlaceMenuSide
		m_animPlaceMenuSlide = m_panelPlaceMenuSide.GetComponent<Animator>();
	}

	
	// Update is called once per frame
	void Update () 
	{
		
	}


	// 
	public void StartPlace()
	{
		m_panelMainMenu.SetActive(false);
		m_panelPlaceMenuSide.SetActive(true);
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
		m_animPlaceMenuSlide.enabled = true;
		m_animPlaceMenuSlide.Play("PlaceMenuSlideIn");
	}


	private void PlaceMenuSlideOut()
	{
		m_animPlaceMenuSlide.Play("PlaceMenuSlideOut");
	}
}