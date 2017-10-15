using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Item object of the Area Description File list.
/// </summary>
public class AreaDescriptionListElementDate : MonoBehaviour
{
	/// <summary>
	/// The toggle game object.
	/// </summary>
	public Toggle m_toggle;

	/// <summary>
	/// The name text view for displaying the Area Description's human readable name.
	/// </summary>
	public Text m_areaDescriptionName;

	/// <summary>
	/// The date text view for displaying the Area Description's creation date.
	/// </summary>
	public Text m_areaDescriptionDate;
}
