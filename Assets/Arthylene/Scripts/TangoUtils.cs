using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tango;

public class TangoUtils {


	/// <summary>
	/// Return UUID of the first Area Description named by the parameter, if none then return null.
	/// This function will query from the Tango API for the Tango space Area Description. 
	/// </summary>
	public static string GetAreaDescriptionUUIDbyName(string name)
	{
		// Get Tango space Area Description list.
		AreaDescription[] areaDescriptionList = AreaDescription.GetList();

		if (areaDescriptionList == null)
		{
			return null;
		}

		// Look for the specific Area Description in the list
		foreach (AreaDescription areaDescription in areaDescriptionList)
		{
			if (areaDescription.GetMetadata().m_name.Equals(name))
			{
				return areaDescription.m_uuid;
			}
		}

		return null;
	}
}
