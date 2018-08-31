using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Falcone.Variables;

namespace Falcone.Events
{
	public class SetTransform : MonoBehaviour 
	{
		public TransformScriptable transformVal;

		public void OnEnable()
		{
			this.transformVal.value = transform;
			Destroy(this);
		}
	}
}