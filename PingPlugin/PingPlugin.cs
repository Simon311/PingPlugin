using System;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;

namespace PingPlugin
{
	[ApiVersion(1, 23)]
	public class PingPlugin : TerrariaPlugin
	{
		public override Version Version
		{
			get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
		}

		public override string Name
		{
			get { return "Ping"; }
		}

		public override string Author
		{
			get { return "Simon311"; }
		}

		public override string Description
		{
			get { return "Measures ping to clients."; }
		}

		public PingPlugin(Main game) : base(game) 
		{
			Order = 0;
		}

		public const int DefaultOrder = 1;

		public readonly byte[] Packet = new byte[] { 5, 0, 39, 0, 0 };

		public static PingConfig Config;

		private PingData[] Players = new PingData[256];

		public override void Initialize()
		{
			ServerApi.Hooks.NetGetData.Register(this, GetData, DefaultOrder);
			ServerApi.Hooks.ServerLeave.Register(this, ServerLeave, DefaultOrder);
			Config = PingConfig.Read();
			Commands.ChatCommands.Add(new Command("ping.ping", Ping, Config.CommandName));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.NetGetData.Deregister(this, GetData);
				ServerApi.Hooks.ServerLeave.Deregister(this, ServerLeave);
			}
			base.Dispose(disposing);
		}

		private void GetData(GetDataEventArgs e)
		{
			if (e.MsgID == PacketTypes.ItemOwner && Players[e.Msg.whoAmI] != null)
			{
				var pingdata = Players[e.Msg.whoAmI];
				int ping = pingdata.Summarize();

				e.Msg.reader.BaseStream.Position = e.Index;
				short ItemID = e.Msg.reader.ReadInt16();
				byte ItemOwner = e.Msg.reader.ReadByte();

				if (ItemID == pingdata.ID && ItemOwner == 255)
				{
					Players[e.Msg.whoAmI] = null;
					NetMessage.SendData(22, -1, -1, "", ItemID, 0f, 0f, 0f, 0);
					bool Self = e.Msg.whoAmI == pingdata.ReportTo.Index;
					if (pingdata.ReportTo != null && (pingdata.ReportTo.Active || !pingdata.ReportTo.RealPlayer))
						pingdata.ReportTo.SendInfoMessage(string.Format("{1} ping is {0}ms.", ping, Self ? "Your" : (TShock.Players[e.Msg.whoAmI].Name + "'s")));
					return;
				}
				e.Msg.reader.BaseStream.Position = e.Index;
			}
		}

		private void ServerLeave(LeaveEventArgs e)
		{
			Players[e.Who] = null;
		}

		private short GetID(int Target)
		{
			Item c;
			for (short i = 0; i < 400; i++)
			{
				c = Main.item[i];
				if (c != null && c.active && c.owner != Target)
					return i;
			}

			return -1;
		}

		private void Ping(CommandArgs e)
		{
			bool Self = true;
			TSPlayer target = e.Player;

			if (e.Parameters.Count > 0 && e.Player.Group.HasPermission("ping.others"))
			{
				Self = false;
				string t = string.Join(" ", e.Parameters);
				var f = TShock.Utils.FindPlayer(t);
				switch (f.Count)
				{
					case 0:
						e.Player.SendErrorMessage("No players matched!");
						return;
					case 1:
						target = f[0];
						break;
					default:
						e.Player.SendErrorMessage("Multiple players matched: " + string.Join(", ", f));
						return;
				}
			}

			if (Self && !e.Player.RealPlayer)
			{
				e.Player.SendErrorMessage("Sorry, you can't ping yourself from there.");
				return;
			}

			if (Players[target.Index] != null)
			{
				e.Player.SendInfoMessage(string.Format("{0} already being pinged, please wait....", Self ? "You're" : (target.Name + " is")));
				return;
			}

			short id = GetID(target.Index);
			if (id == -1)
			{
				e.Player.SendErrorMessage("Unable to ping!");
				return;
			}
			byte[] Id = BitConverter.GetBytes(id);
			byte[] Data = (byte[])Packet.Clone();
			Data[3] = Id[0];
			Data[4] = Id[1];
			target.SendRawData(Data);
			Players[target.Index] = new PingData(id, e.Player);
		}
	}
}
