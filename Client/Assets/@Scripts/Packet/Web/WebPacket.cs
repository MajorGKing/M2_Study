using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WebPacket
{
	using System;

	[Serializable]
	public class JwtToken
	{
		public long sub;
		public long iat;
		public long exp;
	}
}