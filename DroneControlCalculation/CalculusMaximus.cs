using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Diagnostics;
using System.Globalization;

namespace DroneControlCalculation
{
	public class CalculusMaximus
	{

		/// <summary>
		/// Backgroundworker DoWork-Method
		/// </summary>
		/// <param name="o"></param>
		/// <param name="args"></param>
		public static void startCalculation(object o, DoWorkEventArgs args)
		{
			//stores the DBInterface to get access to the query data
			DBInterface dbInterface = (DBInterface)args.Argument;

			while(true)
			{
				if(dbInterface.queryData == null)
				{
					continue;
				}

				if(dbInterface.queryData.Count != 0)
				{
					QueryData tmp = dbInterface.queryData.First();
					QueryData firstTuple = new QueryData(tmp);
					dbInterface.queryData.Remove(tmp);

					Thread tmpThread = new Thread(delegate () { calcUpDownCommand(dbInterface, firstTuple); });
					tmpThread.Start();
				}
			}
		}

		private static void calcUpDownCommand(DBInterface dbInterface, QueryData firstTuple)
		{
			if(dbInterface == null || firstTuple == null)
			{
				return;
			}

			// ### IS position
			float tmpX = 0.0f;
			float isX = 0.0f;
			float tmpY = 0.0f;
			float isY = 0.0f;
			float tmpZ = 0.0f;
			float isZ = 0.0f;

			if(firstTuple.isData == null)
			{
				return;
			}

			foreach(var item in firstTuple.isData)
			{
				float.TryParse(item[Statics.X].AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out tmpX);
				float.TryParse(item[Statics.Y].AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out tmpY);
				float.TryParse(item[Statics.Z].AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out tmpZ);

				isX += tmpX;
				isY += tmpY;
				isZ += tmpZ;

			}

			isX = isX / firstTuple.isData.Count;
			isY = isY / firstTuple.isData.Count;
			isZ = isZ / firstTuple.isData.Count;

			// ### SHOULD position
			tmpX = 0.0f;
			float shouldX = 0.0f;
			tmpY = 0.0f;
			float shouldY = 0.0f;
			tmpZ = 0.0f;
			float shouldZ = 0.0f;

			if(firstTuple.shouldData == null)
			{
				return;
			}

			foreach(var item in firstTuple.shouldData)
			{
				float.TryParse(item[Statics.X].AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out tmpX);
				float.TryParse(item[Statics.Y].AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out tmpY);
				float.TryParse(item[Statics.Z].AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out tmpZ);

				shouldX += tmpX;
				shouldY += tmpY;
				shouldZ += tmpZ;
			}

			shouldX = shouldX / firstTuple.shouldData.Count;
			shouldY = shouldY / firstTuple.shouldData.Count;
			shouldZ = shouldZ / firstTuple.shouldData.Count;

			if(float.IsNaN(shouldZ) || float.IsNaN(shouldX) || float.IsNaN(shouldY) || float.IsNaN(isX) || float.IsNaN(isY) || float.IsNaN(isZ))
			{
				//Debug.Print("1");
				return;
			}

			//comparison only Y value
			float diffY = shouldY - isY;
			float diffX = shouldX - isX;
			float diffZ = shouldZ - isZ;

			//Debug.Print(diffY.ToString());
			if(float.IsNaN(diffY) || float.IsNaN(diffZ) || float.IsNaN(diffX))
			{
				//Debug.Print("2");
				return;
			}

			var document = new BsonDocument
							{
								 { "Command", "control" },
								 { "Velocity_x", diffX.ToString().Replace(',', '.') },
								 { "Velocity_y", diffY.ToString().Replace(',', '.') },
								 { "Velocity_z", diffZ.ToString().Replace(',', '.') },
								 { "Timestamp", DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString()}
							};
			dbInterface.sendCommand(document);


		}

	}
}
