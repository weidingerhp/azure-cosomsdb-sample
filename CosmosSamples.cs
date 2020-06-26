using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDemo {
	class CosmosSamples {
		public async Task GetData() {
			var client = CosmosDBProvider.Client;

			var container = client.GetContainer("CoderDojoDemo", "CoderDojoDemo");
			var iterator = container.GetItemQueryIterator<dynamic>("select * from c");
			var docs = await iterator.ReadNextAsync();
			foreach(var doc in docs) {
				await Console.Out.WriteLineAsync(JsonConvert.SerializeObject(doc, Formatting.Indented));
			}
			
		}

		public async Task SelectData() {
			var client = CosmosDBProvider.Client;

			var container = client.GetContainer("CoderDojoDemo", "CoderDojoDemo");
			var iterator = container.GetItemQueryIterator<dynamic>("select * from c where c.coder > 25");
			var docs = await iterator.ReadNextAsync();
			foreach (var doc in docs) {
				await Console.Out.WriteLineAsync($"{doc.coder,3} {doc.dojo.date,12} {doc.dojo.location}");
			}

		}

		public async Task WriteData() {
			var rnd = new Random(DateTime.Now.Millisecond);
			var client = CosmosDBProvider.Client;

			var container = client.GetContainer("CoderDojoDemo", "CoderDojoDemo");
			foreach (var ort in new string[] { "Linz", "Wien", "Steyr" }) {
				DateTime t = new DateTime(2020, 6, 26);
				
				
				for (int i = 0; i < 10; i++) {
					t.AddDays(-7);
					dynamic data = new {
						id = Guid.NewGuid(),
						dojo = new {
							date = t.ToShortDateString(),
							location = ort
						},
						coder = rnd.Next(5, 50)
					};
					await container.CreateItemAsync<dynamic>(item: data);
				}
			}
		}

		public async Task DeleteData() {
			var client = CosmosDBProvider.Client;

			var container = client.GetContainer("CoderDojoDemo", "CoderDojoDemo");
			var iterator = container.GetItemQueryIterator<dynamic>("select * from c");
			while(iterator.HasMoreResults) {
				var docs = await iterator.ReadNextAsync();
				foreach(var doc in docs) {
					string id = doc.id;
					string pk = doc.dojo.date;
					await container.DeleteItemAsync<dynamic>(id, new PartitionKey(pk));
				}
			}
		}
	}
}
