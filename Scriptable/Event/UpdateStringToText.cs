using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Falcone.Events
{
	public class UpdateStringToText : MonoBehaviour 
	{
		[SerializeField]
		Falcone.Variables.StringScriptable text;

		[SerializeField]
		TextMeshProUGUI targetlbl;	
		// Update is called once per frame
		public void UpdateString () 
		{
			this.targetlbl.text = this.text.value;
		}
	}
}
