using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Falcone.Events
{
	public class GameDelayCall : MonoBehaviour 
	{
		public UnityEvent response;

		public void DelayCall(float _time)
		{
			StartCoroutine(this.LateRaise(_time));
		}

		IEnumerator LateRaise(float _time)
		{
			yield return new WaitForSeconds(_time);
			this.response.Invoke();
		}
	}
}
