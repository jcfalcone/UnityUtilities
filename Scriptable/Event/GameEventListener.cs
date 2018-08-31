using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Falcone.Events
{
	public class GameEventListener : MonoBehaviour 
	{

		public GameEvent gameEvent;
		public UnityEvent response;

		// Use this for initialization
		void OnEnable () 
		{
			this.gameEvent.Register(this);
		}
		
		// Update is called once per frame
		void OnDisable () 
		{
			this.gameEvent.UnRegister(this);
		}

		public virtual void Response()
		{
			this.response.Invoke();
		}
	}
}
