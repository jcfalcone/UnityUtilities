using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Falcone.Variables
{
	[CreateAssetMenu(menuName="Variables/Transform List")]
	public class TransformList : ScriptableObject 
	{
		public List<Transform> value;
	}
}