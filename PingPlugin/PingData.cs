using System;
using TShockAPI;

namespace PingPlugin
{
	public class PingData
	{
		public DateTime Start { get; private set; }
		public int ID { get; private set; }
		public TSPlayer ReportTo { get; private set; }

		public PingData(int ItemID, TSPlayer Caller)
		{
			ID = ItemID;
			Start = DateTime.Now;
			ReportTo = Caller;
		}

		public int Summarize()
		{
			return (int)(DateTime.Now - Start).TotalMilliseconds;
		}
	}
}
