using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Falcone.Variables
{
	[CreateAssetMenu(menuName="Variables/Bool")]
	public class BoolScriptable : ScriptableObject
	{
		public bool value;
	}
}