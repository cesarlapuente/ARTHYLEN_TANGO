using UnityEngine;
using System.Xml.Serialization;

/// <summary>
/// Data container for produce.
/// 
/// Used for serializing/deserializing produce to xml.
/// </summary>
[System.Serializable]
public class ProduceData
{
	/// <summary>
	/// Produce's type.
	/// (e.g. banana, apple, watermelon...).
	/// </summary>
	[XmlElement("type")]
	public int m_type;

	/// <summary>
	/// Position of the this produce, with respect to the origin of the game world (i.e. the produce department).
	/// </summary>
	[XmlElement("position")]
	public Vector3 m_position;

	/// <summary>
	/// Rotation of the this produce.
	/// </summary>
	[XmlElement("orientation")]
	public Quaternion m_orientation;
}