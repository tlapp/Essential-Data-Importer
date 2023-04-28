// See https://aka.ms/new-console-template for more information
using Essential.Api.Import;
using Essential.Api.Import.Entities;
using Newtonsoft.Json;
using Npoi.Mapper;
using System.Diagnostics;

Console.WriteLine("Starting Essential Import Procedure.....", ConsoleColor.Green);
Stopwatch stopwatch = Stopwatch.StartNew();

//Load the values from the Excel sheet. 
var mapper = new Mapper("essential_upload.xlsx");
var informationRepresentations = mapper.Take<InformationRepresentation>("Information Representation");
var dataRepresentations = mapper.Take<DataRepresentation>("Data Representation");
var dataRepresentationAttributes = mapper.Take<DataRepresentationAttribute>("Data Representation Attribute");

Console.WriteLine($"Found {informationRepresentations.Count()} information representation objects.", ConsoleColor.Blue);
Console.WriteLine($"Found {dataRepresentations.Count()} data representation objects.", ConsoleColor.Blue);
Console.WriteLine($"Found {dataRepresentationAttributes.Count()} data representation attribute objects.", ConsoleColor.Blue);

//Set up API connections for app.
//TODO: Move this to config file.
string baseUrl = "https://apm.valard.com/";
string apiKey = "624XEsMZQDmNNSCCCMrKqW:06jvOhk1Ey5pZxHxcc4efi";
string repositoryId = "a60bf46c5322de08058e"; //PROD: "ea0dfcfb81c2fa9ebfe8";
string className = string.Empty;

using ApiClient apiClient = new(baseUrl);
apiClient.AddHeader("x-api-key", apiKey);

#region Get Authorization token.
var authorizationToken = await GetAuthToken(apiClient);
#endregion Get Authorization token.

if (authorizationToken != null)
{
	apiClient.AddHeader("Authorization", $"Bearer {authorizationToken.bearerToken}");


	#region Data Representation Attributes
	//Begin processing Data Attributes
	className = "Data_Representation_Attribute";
	string dataRepresentationAttributePath = $"api/essential-utility/v2/repositories/{repositoryId}/classes/{className}/instances";
	Console.WriteLine($"Begin processing {className}.");
	//Query to get all attributes from Essential
	var existingDataAttributes = JsonConvert.DeserializeObject<InstanceCollection>(
		await apiClient.Get(dataRepresentationAttributePath));
	Console.WriteLine($"Found {existingDataAttributes.instances.Count()} {className} objects in Essential.", ConsoleColor.Blue);

	InstanceCollection instanceCollection = new();
	List<Instance> instancesToProcess = new();
	foreach (var dataAttr in dataRepresentationAttributes.ToList())
	{
		Instance instanceToProcess = new();
		List<Slot> instanceSlots = new();

		if (existingDataAttributes != null)
		{
			var existingInstance = existingDataAttributes.instances.SingleOrDefault(w => w.name == dataAttr.Value.Name);
			if (existingInstance != null)
			{
				instanceToProcess.id = existingInstance.id;
			}
		}

		instanceToProcess.name = dataAttr.Value.Name;
		instanceToProcess.className = className;
		instanceToProcess.description = dataAttr.Value.Description;

		//Add a slot for the technical name.
		if (!string.IsNullOrEmpty(dataAttr.Value.TechnicalName))
		{
			
			List<string> slotValues = new()
				{
					dataAttr.Value.TechnicalName
				};

			instanceSlots.Add(new Slot()
			{
				slotName = "dra_technical_name",
				slotValue = slotValues.ToArray()
			});
		}
		instanceToProcess.slots = instanceSlots.ToArray();
		instancesToProcess.Add(instanceToProcess);
	}

	instanceCollection.instances = instancesToProcess.ToArray();

	//await SubmitToEssential(apiClient, dataRepresentationAttributePath, instanceCollection);
	instanceCollection = new InstanceCollection();
	Console.WriteLine($"Saved {instancesToProcess.Count} items.");

	existingDataAttributes = null;
	
	Console.WriteLine($"Finished processing {className}.");
	Console.WriteLine(string.Empty);

	#endregion Data Representation Attributes


	#region Data Representations
	className = "Data_Representation";
	string dataRepresentationPath = $"api/essential-utility/v2/repositories/{repositoryId}/classes/{className}/instances";
	//Query to get all attributes from Essential
	var existingDataRepresentations = JsonConvert.DeserializeObject<InstanceCollection>(
		await apiClient.Get(dataRepresentationPath));
	Console.WriteLine($"Found {existingDataRepresentations.instances.Count()} {className} objects in Essential.", ConsoleColor.Blue);
	existingDataAttributes = JsonConvert.DeserializeObject<InstanceCollection>(
		await apiClient.Get(dataRepresentationAttributePath));
	Console.WriteLine($"Found {existingDataAttributes.instances.Count()} data representation attribute objects in Essential.", ConsoleColor.Blue);

	instancesToProcess.Clear();
	foreach (var dataRep in dataRepresentations.ToList())
	{
		Instance instanceToProcess = new();
		List<Slot> instanceSlots = new();

		string instanceName = $"{dataRep.Value.AssociatedApplication} {dataRep.Value.Name}";
		if (existingDataRepresentations != null)
		{
			var existingInstance = existingDataRepresentations.instances.SingleOrDefault(w => w.name == instanceName);
			if (existingInstance != null)
			{
				instanceToProcess.id = existingInstance.id;
			}
		}

		instanceToProcess.name = instanceName;
		instanceToProcess.className = className;
		instanceToProcess.description = dataRep.Value.Description;

		//Add a slot for the technical name.
		if (!string.IsNullOrEmpty(dataRep.Value.TechnicalName))
		{

			List<string> slotValues = new()
				{
					dataRep.Value.TechnicalName
				};

			instanceSlots.Add(new Slot()
			{
				slotName = "dr_technical_name",
				slotValue = slotValues.ToArray()
			});
		}

		var attributesToAdd = dataRepresentationAttributes.Where(w => w.Value.DataRepresentation == dataRep.Value.Name &&
			w.Value.AssociatedApplication == dataRep.Value.AssociatedApplication)
			.ToList();

		if (attributesToAdd.Count > 0)
		{
			List<string> slotValues = new();
			foreach (var attribute in attributesToAdd)
			{
				string attributeName = $"{attribute.Value.AssociatedApplication} {attribute.Value.Name}";
				slotValues.AddRange(existingDataAttributes.instances
					.Where(w => w.name == attribute.Value.Name)
					.Select(s => s.id)
					.ToList());
			}
			instanceSlots.Add(new Slot()
			{
				slotName = "contained_data_representation_attributes",
				slotValue = slotValues.ToArray()
			});
		}

		instanceToProcess.slots = instanceSlots.ToArray();
		instancesToProcess.Add(instanceToProcess);
	}

	instanceCollection.instances = instancesToProcess.ToArray();

	await SubmitToEssential(apiClient, dataRepresentationPath, instanceCollection);
	instanceCollection = new InstanceCollection();
	Console.WriteLine($"Saved {instancesToProcess.Count} items.");

	existingDataRepresentations = null;
	existingDataAttributes = null;

	Console.WriteLine($"Finished processing {className}.");
	Console.WriteLine(string.Empty);
	#endregion Data Representations

	#region Data Representations
	className = "Information_Representation";
	string infoRepresentationPath = $"api/essential-utility/v2/repositories/{repositoryId}/classes/{className}/instances";
	//Query to get all attributes from Essential
	var existingInformationRepresentations = JsonConvert.DeserializeObject<InstanceCollection>(
		await apiClient.Get(infoRepresentationPath));
	Console.WriteLine($"Found {existingInformationRepresentations.instances.Count()} {className} objects in Essential.", ConsoleColor.Blue);
	existingDataRepresentations = JsonConvert.DeserializeObject<InstanceCollection>(
		await apiClient.Get(dataRepresentationPath));
	Console.WriteLine($"Found {existingDataRepresentations.instances.Count()} data representation objects in Essential.", ConsoleColor.Blue);

	instancesToProcess.Clear();
	foreach (var infoRep in informationRepresentations.ToList())
	{
		Instance instanceToProcess = new();
		List<Slot> instanceSlots = new();

		string instanceName = $"{infoRep.Value.AssociatedApplication} {infoRep.Value.Name}";
		if (existingInformationRepresentations != null)
		{
			var existingInstance = existingInformationRepresentations.instances.SingleOrDefault(w => w.name == instanceName);
			if (existingInstance != null)
			{
				instanceToProcess.id = existingInstance.id;
			}
		}

		instanceToProcess.name = instanceName;
		instanceToProcess.className = className;
		instanceToProcess.description = infoRep.Value.Description;

		var attributesToAdd = dataRepresentations.Where(w => w.Value.InformationRepresentation == infoRep.Value.Name &&
			w.Value.AssociatedApplication == infoRep.Value.AssociatedApplication)
			.ToList();

		if (attributesToAdd.Count() > 0)
		{
			List<string> slotValues = new();
			foreach (var attribute in attributesToAdd)
			{
				slotValues.AddRange(existingDataRepresentations.instances
					.Where(w => w.name == $"{attribute.Value.AssociatedApplication} {attribute.Value.Name}")
					.Select(s => s.id)
					.ToList());
			}
			instanceSlots.Add(new Slot()
			{
				slotName = "supporting_data_representations",
				slotValue = slotValues.ToArray()
			});
		}

		instanceToProcess.slots = instanceSlots.ToArray();
		instancesToProcess.Add(instanceToProcess);
	}

	instanceCollection.instances = instancesToProcess.ToArray();
	
	await SubmitToEssential(apiClient, infoRepresentationPath, instanceCollection);
	instanceCollection = new InstanceCollection();
	Console.WriteLine($"Saved {instancesToProcess.Count} items.");

	existingInformationRepresentations = null;
	existingDataRepresentations = null;
	existingDataAttributes = null;

	Console.WriteLine($"Finished processing {className}.");
	Console.WriteLine(string.Empty);
	#endregion Data Representations

}

stopwatch.Stop();
Console.WriteLine($"Processing time elapsed: {stopwatch.Elapsed}.", ConsoleColor.Green);
Console.WriteLine("Completed Essential Import Procedure..... Click any key to end.", ConsoleColor.Green);
Console.ReadLine();


static async Task<AuthorizationToken> GetAuthToken(ApiClient apiClient)
{
	string authorizationPath = $"api/oauth/token";

	var result = await apiClient.Post(
		authorizationPath,
		new AuthenticationRequest());
	var authorizationToken = JsonConvert.DeserializeObject<AuthorizationToken>(result);
	return authorizationToken;
}

static async Task SubmitToEssential(ApiClient apiClient, string path, object content)
{
	try
	{
		//retry the post request to save/update the object.
		await apiClient.Post(path, content);
	}
	catch (Exception ex)
	{
		if (ex.Message.StartsWith("Error: 403"))
		{
			AuthorizationToken authorizationToken = await GetAuthToken(apiClient);
			apiClient.RemoveHeader("Authorization");
			apiClient.AddHeader("Authorization", $"Bearer {authorizationToken.bearerToken}");
			//retry the post request to save/update the object.
			await apiClient.Post(path, content);
		}
	}
}


