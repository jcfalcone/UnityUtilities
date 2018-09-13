using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Falcone.Events
{
	public class UpdateNumberToText : MonoBehaviour 
	{
		[SerializeField]
		Falcone.Variables.IntScriptable text;

		[SerializeField]
		TextMeshProUGUI targetlbl;	
		// Update is called once per frame
		public void UpdateString () 
		{
			this.targetlbl.text = this.text.value.ToString();
		}
	}
}
