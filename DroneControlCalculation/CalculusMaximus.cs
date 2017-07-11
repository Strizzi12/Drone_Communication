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
		private static float velocityFactor025ms = 0.1f;

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

					calcControlCommand(dbInterface, firstTuple);
					//Thread tmpThread = new Thread(delegate () { calcControlCommand(dbInterface, firstTuple); });
					//tmpThread.Start();
					//Thread.Sleep(250);
				}
			}
		}

		private static void calcControlCommand(DBInterface dbInterface, QueryData firstTuple)
		{
			float velocityFactor1ms = 1.0f;

			if(dbInterface == null || firstTuple == null)
			{
				return;
			}

			// ### IS position
			float isX = 0.0f;
			float isY = 0.0f;
			float isZ = 0.0f;

			if(firstTuple.isData == null)
			{
				return;
			}

			float.TryParse(firstTuple.isData[Statics.X].AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out isX);
			float.TryParse(firstTuple.isData[Statics.Y].AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out isY);
			float.TryParse(firstTuple.isData[Statics.Z].AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out isZ);


			// ### SHOULD position
			float shouldX = 0.0f;
			float shouldY = 0.0f;
			float shouldZ = 0.0f;

			if(firstTuple.shouldData == null)
			{
				return;
			}

			float.TryParse(firstTuple.shouldData[Statics.X].AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out shouldX);
			float.TryParse(firstTuple.shouldData[Statics.Y].AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out shouldY);
			float.TryParse(firstTuple.shouldData[Statics.Z].AsString, NumberStyles.Any, CultureInfo.InvariantCulture, out shouldZ);

			if(float.IsNaN(shouldZ) || float.IsNaN(shouldX) || float.IsNaN(shouldY) || float.IsNaN(isX) || float.IsNaN(isY) || float.IsNaN(isZ))
			{
				return;
			}

			//comparison only Y value
			float diffY = shouldY - isY;
			float diffX = shouldX - isX;
			float diffZ = shouldZ - isZ;

			if(float.IsNaN(diffY) || float.IsNaN(diffZ) || float.IsNaN(diffX))
			{
				return;
			}

			if(myAbs(diffX) < 0.3f || myAbs(diffZ) < 0.3f)
			{
				velocityFactor1ms = velocityFactor1ms * velocityFactor025ms;
			}

			if(diffX > 0f && diffZ < 0f)
			{
				diffX = -velocityFactor1ms * myAbs(diffX);
				diffZ = -velocityFactor1ms * myAbs(diffZ);

			}
			else if(diffX < 0f && diffZ < 0f)
			{
				diffX = velocityFactor1ms * myAbs(diffX);
				diffZ = -velocityFactor1ms * myAbs(diffZ);
			}
			else if(diffX > 0f && diffZ > 0f)
			{
				diffX = -velocityFactor1ms * myAbs(diffX);
				diffZ = velocityFactor1ms * myAbs(diffZ);
			}
			else if(diffX < 0f && diffZ > 0f)
			{
				diffX = velocityFactor1ms * myAbs(diffX);
				diffZ = velocityFactor1ms * myAbs(diffZ);
			}

			if(diffY > 1.5f)
			{
				diffY = 0.5f;
			}
			diffY = velocityFactor1ms * diffY;
			Debug.WriteLine("X=" + diffX + " Y=" + diffY + " Z=" + diffZ);

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

		static float myAbs(float value)
		{
			if(value < 0)
			{
				return (value * -1f);
			}
			else
			{
				return value;
			}
		}
	}
}
