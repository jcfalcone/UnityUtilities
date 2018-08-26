using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Falcone.Events
{
	public class RaiseEvent : MonoBehaviour 
	{
		[SerializeField]
		GameEvent gameEvent;
		
		public void Raise () 
		{
			this.gameEvent.Raise();
		}
	}
}