using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;
using Smod2.Attributes;

namespace SCP_372
{
	[PluginDetails(
		author = "ShingekiNoRex",
		name = "SCP-372",
		description = "Player is invisible while standing still and not shooting.",
		id = "shingekinorex.scp372",
		version = "4.0.0",
		SmodMajor = 3,
		SmodMinor = 10,
		SmodRevision = 6
	)]
	class SCP372Plugin : Plugin
	{
		public override void OnDisable()
		{
		}

		public override void OnEnable()
		{

		}

		public override void Register()
		{
			AddEventHandlers(new EventHandler(this), Priority.NORMAL);
		}
	}

	class EventHandler : IEventHandlerSetRole, IEventHandlerShoot, IEventHandlerFixedUpdate, IEventHandlerRoundStart
    {
		private Plugin plugin;

		public EventHandler(Plugin plugin)
		{
			this.plugin = plugin;
		}

		public class ShowGhost
		{
			public int playerId;

			public float remainingTime;
		}

		public List<ShowGhost> ghostList = new List<ShowGhost>();

		public void OnRoundStart(RoundStartEvent ev)
		{
			List<Player> PlayerList = new List<Player>();
			foreach (Player player in ev.Server.GetPlayers())
			{
				if (player.PlayerRole.Team == TeamType.D_CLASS)
				{
					PlayerList.Add(player);
				}
			}

			if (PlayerList.Count > 2)
			{
				PlayerList[new System.Random().Next(PlayerList.Count)].ChangeRole(RoleType.TUTORIAL);
			}
		}

		public void OnSetRole(PlayerSetRoleEvent ev)
        {
			if (ev.RoleType == RoleType.TUTORIAL)
			{
				ev.Player.SetGhostMode(true);
			}
			else if (ev.Player.GetGhostMode())
			{
				ev.Player.SetGhostMode(false);
			}
        }

		public void OnShoot(PlayerShootEvent ev)
		{
			//plugin.Logger.Warn("SCP-372", "ONSHOOT " + ev.Weapon.ItemType + " " + ev.HitType);
			if (ev.Player.PlayerRole.Team == TeamType.TUTORIAL)
			{
				foreach(ShowGhost ghost in ghostList)
				{
					if (ghost.playerId == ev.Player.PlayerID)
					{
						ghost.remainingTime = 3f;
						ev.Player.SetGhostMode(false);
						return;
					}
				}

				ghostList.Add(new ShowGhost { playerId = ev.Player.PlayerID, remainingTime = 3f });
				ev.Player.SetGhostMode(false);
			}
		}

		public void OnFixedUpdate(FixedUpdateEvent ev)
		{
			for (int i = 0; i < ghostList.Count; i++)
			{
				ghostList[i].remainingTime -= 0.02f;
				if (ghostList[i].remainingTime <= 0)
				{
					foreach(Player player in plugin.PluginManager.Server.GetPlayers())
					{
						if (player.PlayerID == ghostList[i].playerId && player.PlayerRole.Team == TeamType.TUTORIAL)
						{
							player.SetGhostMode(true);
						}
					}
					ghostList.RemoveAt(i);
					return;
				}
			}
		}
	}
}