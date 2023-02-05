using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Powergrid
{
	public class XORGate : PowerSlot
	{
		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public override int GetPower()
		{
			if (_powerFrom.Count == 1)
				return 1;
			return 0;
		}

		public override int ConvertPower(int inputPower)
		{
			if (_powerFrom.Count == 1)
				return 1;
			return 0;
		}
	}
}