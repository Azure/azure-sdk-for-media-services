using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.MediaServices.Client
{

	/// <summary>
	/// Cross site access policy.
	/// </summary>
	public class CrossSiteAccessPolicy
	{
		/// <summary>
		/// Policy.
		/// </summary>
		public string Policy { get; set; }

		/// <summary>
		/// Version.
		/// </summary>
		public string Version { get; set; }
	}
}
