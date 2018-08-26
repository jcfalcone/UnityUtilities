using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Falcone.Events
{
	[CreateAssetMenu(menuName = "Event")]
	public class GameEvent : ScriptableObject 
	{
		List<GameEventListener> listeners = new List<GameEventListener>();

		// Use this for initialization
		public void Register (GameEventListener _parent) 
		{
			this.listeners.Add(_parent);
		}
		
		// Update is called once per frame
		public void UnRegister (GameEventListener _parent) 
		{
			this.listeners.Remove(_parent);
		}

		public void Raise()
		{
			for(int count = 0; count < this.listeners.Count; count++)
			{
				this.listeners[count].Response();
			}
		}
	}
}
