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

	/// <summary>
	/// PlaceMenuSide animator component.
	/// 
	/// The animator will start animations SlideIn and SlideOut.
	/// </summary>
	public Animator m_animPlaceMenuSide;
	private bool isPlaceMenuOpen;


	// Use this for initialization
	void Start () 
	{
		// The PlaceMenu start closed.
		isPlaceMenuOpen = false;
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
		m_animPlaceMenuSide.enabled = true;
		m_animPlaceMenuSide.Play("PlaceMenuSlideIn");
	}


	private void PlaceMenuSlideOut()
	{
		m_animPlaceMenuSide.Play("PlaceMenuSlideOut");
	}
}