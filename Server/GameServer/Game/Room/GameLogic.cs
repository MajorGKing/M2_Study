using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Server.Game
{
	// GameLogic
	// - GameRoom
	// -- Zone
	public class GameLogic : JobSerializer
	{
		public static GameLogic Instance { get; } = new GameLogic();

		public void Update()
		{
			Flush();
		}
	}
}
