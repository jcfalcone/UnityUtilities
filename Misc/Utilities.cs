using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
	static Camera mainCamera;

	public static bool IsInFrontOfCamera(Vector3 _position)
	{
		if(mainCamera == null)
		{
			mainCamera = Camera.main;
		}

		if(mainCamera != null)
		{
			Vector3 position = mainCamera.WorldToScreenPoint(_position);

			if(position.x > 0 && position.x < 1 &&
			   position.y > 0 && position.y < 1 && 
			   position.z > 0)
			{
				return true;
			}
		}

		return false;
	}
}
