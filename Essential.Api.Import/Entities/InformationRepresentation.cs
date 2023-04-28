using System.ComponentModel.DataAnnotations;

namespace Essential.Api.Import.Entities
{
	public class InformationRepresentation
	{
		[Display(Name = "Name")]
		public string Name { get; set; }
		[Display(Name = "Description")]
		public string Description { get; set; }
		public string Id { get; set; }
		public string AssociatedApplication { get; set; }
	}
}
