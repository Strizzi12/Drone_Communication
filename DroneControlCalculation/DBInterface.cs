using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Diagnostics;
using System.Collections;

namespace DroneControlCalculation
{
	public class DBInterface
	{
		public IMongoClient Client = null;
		public IMongoDatabase Database = null;
		public List<QueryData> queryData = null;

		/// <summary>
		/// Constructor incl. init
		/// </summary>
		public DBInterface()
		{
			Client = new MongoClient();
			Database = Client.GetDatabase(Statics.DRONEDBNAME);
			queryData = new List<DroneControlCalculation.QueryData>();
		}

		/// <summary>
		/// Manual init of DB connection
		/// </summary>
		public void InitDatabase()
		{
			Client = new MongoClient(/*"193.171.53.68"*/);

			Database = Client.GetDatabase(Statics.DRONEDBNAME);
		}

		/// <summar>
		/// Drops all tables from DroneDB
		/// </summary>
		public void ResetDatabase()
		{
			if(Database != null)
			{
				Database.DropCollection(Statics.ISCOORDINATES);
				Database.DropCollection(Statics.SHOULDCOORDINATES);
				Database.DropCollection(Statics.DRONECOMMANDS);
			}
			Client = null;
			Database = null;
		}

		public int QueryData()
		{
			if(Database == null)
			{
				return -1;
			}

			IMongoCollection<BsonDocument> shouldCollection = null;
			IMongoCollection<BsonDocument> isCollection = null;
			BsonDocument shouldTuple = null;
			BsonDocument isTuple = null;

			shouldCollection = Database.GetCollection<BsonDocument>(Statics.SHOULDCOORDINATES);
			isCollection = Database.GetCollection<BsonDocument>(Statics.ISCOORDINATES);

			if(shouldCollection == null || isCollection == null)
			{
				return -1;
			}

			//shouldTuples = shouldCollection.Find(<BsonDocument>.Empty)?.ToList();
			//shouldTuples = shouldCollectio.n


			//SortByBuilder sbb = new SortByBuilder();
			//sbb.Descending("_id");
			//var allDocs = collection.FindAllAs<BsonDocument>().SetSortOrder(sbb).SetLimit(N);


			try
			{
				var test = shouldCollection.Find(FilterDefinition<BsonDocument>.Empty);
				if(test == null || test.Count() == 0)
				{
					return 0;
				}

				shouldTuple = test.Sort("{Timestamp: -1}").Limit(1).First();

				test = isCollection.Find(FilterDefinition<BsonDocument>.Empty);
				if(test == null || test.Count() == 0)
				{
					return 0;
				}

				isTuple = test.Sort("{Timestamp: -1}")?.Limit(1)?.First();
			}
			catch(Exception ex)
			{
				Debug.Print(ex.Message);
				return 0;
			}

			if(shouldTuple == null || isTuple == null)
			{
				return 0;
			}

			queryData.Add(new QueryData(shouldTuple, isTuple));

			shouldCollection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
			isCollection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
			return 0;
		}

		/// <summary>
		/// Sends given command to drone; 
		/// (Stores it into DB)
		/// </summary>
		/// <param name="_document"></param>
		public async void sendCommand(BsonDocument _document)
		{
			var collection = Database.GetCollection<BsonDocument>(Statics.DRONECOMMANDS);
			await collection.InsertOneAsync(_document);
		}

	}
}
