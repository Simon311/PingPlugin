using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingPlugin
{
	public class PingData
	{
		public DateTime Start { get; private set; }
		public int ID { get; private set; }
		public int ReportTo { get; private set; }

		public PingData(int ItemID, int Caller)
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
