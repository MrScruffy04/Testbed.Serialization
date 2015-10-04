namespace Testbed.Serialization.Models
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Serialization;

	[XmlType("errors")]
	public class ErrorCollectionModel
	{
		public ErrorCollectionModel()
		{
			Errors = new List<ErrorModel>();
		}

		public ErrorCollectionModel(IEnumerable<ErrorModel> errors)
		{
			Errors = errors.ToList();
		}

		/*
		 * We are intentionally omitting this property if it is null.
		 */
		[XmlElement("error")]
		public List<ErrorModel> Errors { get; set; }
	}
}
