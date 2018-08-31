using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Falcone.Variables;

namespace Falcone.Events
{
	public class BoolGameEvent : GameEventListener
	{
		[SerializeField]
		BoolScriptable targetBool;

		public UnityEvent trueResponse;
		public UnityEvent falseResponse;

		public override void Response()
		{
			if(this.targetBool.value)
			{
				this.trueResponse.Invoke();
			}
			else
			{
				this.falseResponse.Invoke();
			}
		}
	}
}
