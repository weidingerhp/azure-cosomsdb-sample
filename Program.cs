using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;

namespace CosmosDemo {
	class CosmosDBProvider {
		public static CosmosClient Client { get; }

		static CosmosDBProvider() {
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
			Client = new CosmosClient(config["CosmosEndpoint"], config["CosmosKey"]);
		}
	}

	class Program {
		static void Main(string[] args) {
			var samples = new CosmosSamples();
			Console.Out.WriteLine("Reading Data from Database");
			samples.GetData().Wait();

			Console.Out.WriteLine("Write Some Data into Database");
			samples.WriteData().Wait();

			Console.Out.WriteLine("Reading Data from Database");
			samples.GetData().Wait();

			Console.Out.WriteLine("Selecting Data from Database");
			samples.SelectData().Wait();

			Console.Out.WriteLine("Delete all data");
			samples.DeleteData().Wait();
		}
	}
}
