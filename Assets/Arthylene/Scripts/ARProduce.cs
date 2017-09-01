using System.Collections;
using UnityEngine;


public class ARProduce : MonoBehaviour
{
	/// <summary>
	/// The type of the produce.
	/// 
	/// This field is used in the Area Learning example for identify the produce type.
	/// </summary>
	public int m_type = 0;

	/// <summary>
	/// The Tango time stamp when this object is created
	/// 
	/// This field is used in the Area Learning example, the timestamp is save for the position adjustment when the
	/// loop closure happens.
	/// </summary>
	public float m_timestamp = -1.0f;

	/// <summary>
	/// The produce's transformation with respect to the device frame.
	/// </summary>
	public Matrix4x4 m_deviceTProduce = new Matrix4x4();

	/// <summary>
	/// The animation playing.
	/// </summary>
	private Animation m_anim;

	/// <summary>
	/// Awake this instance.
	/// </summary>
	private void Awake()
	{
		// The animation should be started in Awake and not Start so that it plays on its first frame.
		m_anim = GetComponent<Animation>();
		m_anim.Play("Show", PlayMode.StopAll);
	}

	/// <summary>
	/// Plays an animation, then destroys.
	/// </summary>
	private void Hide()
	{
		m_anim.Play("Hide", PlayMode.StopAll);
	}

	/// <summary>
	/// Callback for the animation system.
	/// </summary>
	private void HideDone()
	{
		Destroy(gameObject);
	}
}