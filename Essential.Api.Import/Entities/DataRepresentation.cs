using System.ComponentModel.DataAnnotations;

namespace Essential.Api.Import.Entities
{
	public class DataRepresentation
	{
		[Display(Name = "Name")]
		public string Name { get; set; }
		[Display(Name = "Technical Name")]
		public string TechnicalName { get; set; }
		[Display(Name = "Description")]
		public string Description { get; set; }
		[Display(Name = "Information Representation")]
		public string InformationRepresentation { get; set; }
		[Display(Name = "Associated Application")]
		public string AssociatedApplication { get; set; }
	}
}
