using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIClient.Common.Enums
{
	public enum ReportFinishedEnum
	{
		[Description("All other statuses")]
		Others = 0,
		[Description("Draft or Pending Signature")]
		DraftOrPendingSignature = 1,
		[Description("Final")]
		Final = 2,
		[Description("Discarded")]
		Discarded = 3
	}
}
