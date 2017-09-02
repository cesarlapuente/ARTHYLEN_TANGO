using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;


public class FirebaseUtils {


	/// <summary>
	/// Write produce list to an xml file stored in application storage.
	/// Later on, it should be handled with Firebase.
	/// </summary>
	public static void saveProduceToDisk(string fileName, List<GameObject> produceList)
	{
		// Compose a XML data list.
		List<ProduceData> xmlDataList = new List<ProduceData>();
		foreach (GameObject obj in produceList)
		{
			// Add produce data to the list, we intentionally didn't add the timestamp, because the timestamp will not be
			// useful when the next time Tango Service is connected. The timestamp is only used for loop closure pose
			// correction in current Tango connection.
			ProduceData temp = new ProduceData();
			temp.m_type = obj.GetComponent<ARProduce>().m_type;
			temp.m_position = obj.transform.position;
			temp.m_orientation = obj.transform.rotation;
			xmlDataList.Add(temp);
		}

		string path = Application.persistentDataPath + "/" + fileName + ".xml";
		var serializer = new XmlSerializer(typeof(List<ProduceData>));
		using (var stream = new FileStream(path, FileMode.Create))
		{
			serializer.Serialize(stream, xmlDataList);
		}
	}
}
