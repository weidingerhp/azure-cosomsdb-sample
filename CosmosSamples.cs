using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDemo {
	class CosmosSamples {
		public async Task GetData() {
			var client = CosmosDBProvider.Client;

			// Open the container inside the database
			var container = client.GetContainer("CoderDojoDemo", "CoderDojoDemo");
			
			// issue select statement and get the FeedIterator (with the pages of data - default this would contain up
			// to 100 datasets)
			var iterator = container.GetItemQueryIterator<dynamic>("select * from c");
			
			// fetch the first page and iterate over all containing datasets
			var docs = await iterator.ReadNextAsync();
			foreach (var doc in docs) {
				await Console.Out.WriteLineAsync(JsonConvert.SerializeObject(doc, Formatting.Indented));
			}
		}


		// does the same as getData (with where clause if existent) and print it as table
		public async Task GetDataShort(string whereClause = null) {
			var client = CosmosDBProvider.Client;
			var container = client.GetContainer("CoderDojoDemo", "CoderDojoDemo");
			string selectStmt = "select * from c";
			if (whereClause != null) {
				selectStmt += " where " + whereClause;
			}
			
			var iterator = container.GetItemQueryIterator<dynamic>(selectStmt);
			await Console.Out.WriteLineAsync($"Data from {selectStmt}");
			
			while (iterator.HasMoreResults) {
				var docs = await iterator.ReadNextAsync();
				foreach (var doc in docs) {
					try {
						string[] names = ((JArray)doc.names).Select(p => p.ToString()).ToArray();
						await Console.Out.WriteLineAsync($"{doc.coder,3} {doc.dojo.date,12} {doc.dojo.location,10} {String.Join(",", names)}");
					}
					catch (Exception ex) {
						await Console.Out.WriteLineAsync(ex.Message);
					}
				}
			}
			
		}

		public async Task SelectData() {
			var client = CosmosDBProvider.Client;

			var container = client.GetContainer("CoderDojoDemo", "CoderDojoDemo");
			
			// Select all the items that match the WHERE-clause - in this case all datasets that contain "Joda" in the
			// names - array
			var iterator = container.GetItemQueryIterator<dynamic>("select * from c where ARRAY_CONTAINS(c.names, 'Joda')");
			var docs = await iterator.ReadNextAsync();
			foreach (var doc in docs) {
				try {
					string[] names = ((JArray)doc.names).Select(p => p.ToString()).ToArray();
					await Console.Out.WriteLineAsync($"{doc.coder,3} {doc.dojo.date,12} {doc.dojo.location,10} {String.Join(",", names)}");
				}
				catch (Exception ex) {
					await Console.Out.WriteLineAsync(ex.Message);
				}
			}
		}

		public async Task ModifyData() {
			var client = CosmosDBProvider.Client;

			var container = client.GetContainer("CoderDojoDemo", "CoderDojoDemo");
			
			// Select all the items that match the WHERE-clause
			var iterator = container.GetItemQueryIterator<dynamic>("select * from c where c.dojo.location = 'Steyr'");
			var docs = await iterator.ReadNextAsync();
			foreach (var doc in docs) {
				try {
					string[] names = ((JArray)doc.names).Select(p => p.ToString()).ToArray();
					await Console.Out.WriteLineAsync($"{doc.coder,3} {doc.dojo.date,12} {doc.dojo.location,10} {String.Join(",", names)}");
					
					// Replace the location with "Online"
					doc.dojo.location = "Online";
					
					// we need to use this here otherwise the compiler would get "JObjects" from the dynamic type
					// and it would not work
					string id = doc.id;
					string partKey = doc.dojo.date;
					
					// To replace the dataset you need the id and the "Partition Key" - See definition of Container
					// in our case dojo/date is the Partition-Key
					await container.ReplaceItemAsync(doc, id, new PartitionKey(partKey));
				}
				catch (Exception ex) {
					await Console.Out.WriteLineAsync(ex.Message);
				}
			}
		}

		public async Task WriteData() {
			var rnd = new Random(DateTime.Now.Millisecond);
			var client = CosmosDBProvider.Client;

			var codernames = new string[] {
				"klaus", "claudia", "fritz", "lea", "marvin", "ObiWan", "Joda", "Luke"
			};

			var container = client.GetContainer("CoderDojoDemo", "CoderDojoDemo");
			
			// create Entries for Linz, Wien and Steyr in the db with 10 datasets each
			foreach (var ort in new string[] { "Linz", "Wien", "Steyr" }) {
				DateTime t = new DateTime(2020, 6, 26);
				
				
				for (int i = 0; i < 10; i++) {
					// next dojo will be one week before
					t.AddDays(-7);
					int codersnum = rnd.Next(5, 50);

					List<string> coders = new List<string>();
					for (int j = 0; j < codersnum; j++) {
						coders.Add(codernames[rnd.Next(0, codernames.Length - 1)]);
					};

					dynamic data = new {
						id = Guid.NewGuid(),
						dojo = new {
							// dont use the DateTime directly since this is the partition key - and writing a date
							// in json has several variants.
							date = t.ToShortDateString(),
							location = ort
						},
						coder = rnd.Next(5, 50),
						names = coders
					};
					await container.CreateItemAsync<dynamic>(item: data);
				}
			}
		}

		public async Task DeleteData() {
			var client = CosmosDBProvider.Client;

			// iterate over all datasets in all pages and delete them
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
