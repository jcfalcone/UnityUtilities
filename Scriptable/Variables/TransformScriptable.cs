using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Falcone.Variables
{
	[CreateAssetMenu(menuName="Variables/Transform")]
	public class TransformScriptable : ScriptableObject 
	{
		public Transform value;
	}
}