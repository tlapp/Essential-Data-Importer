namespace Essential.Api.Import.Entities
{

	public class InstanceCollection
	{
		public Instance[] instances { get; set; }
	}

	public class Instance
	{
		public string id { get; set; }
		public Externalid[] externalIds { get; set; }
		public string className { get; set; }
		public string name { get; set; }
		public Slot[] slots { get; set; }
		public string description { get; set; }
	}

	public class Externalid
	{
		public string sourceName { get; set; }
		public string id { get; set; }
	}

	public class Slot
	{
		public string slotName { get; set; }
		public object[] slotValue { get; set; }
	}

}
