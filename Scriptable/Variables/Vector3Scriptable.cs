using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Falcone.Variables
{
	[CreateAssetMenu(menuName="Variables/Vector3")]
	public class Vector3Scriptable : ScriptableObject
	{
		public Vector3 value;
	}
}