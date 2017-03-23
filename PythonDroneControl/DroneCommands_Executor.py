#!/usr/bin/python

import pymongo
import rospy
import os
import time

from pymongo import MongoClient
from geometry_msgs.msg import PoseStamped
from geometry_msgs.msg import Twist
from geometry_msgs.msg import Vector3
from std_msgs.msg import Empty
from std_msgs.msg import String
from bson import ObjectId
from pprint import pprint

#Drone commands for takeoff and 
droneTakeoff = rospy.Publisher("/bebop/takeoff",Empty)
droneLanding = rospy.Publisher("/bebop/land",Empty)
droneControl = rospy.Publisher("/bebop/cmd_vel",Twist)

client = MongoClient('193.171.53.44', 27017)
db = client['DroneDB']
collection = db['DroneCommands']
droneStarted = False
velocity_x = 0.0
velocity_y = 0.0
velocity_z = 0.0

if __name__ == '__main__':
	try:
		rospy.init_node('talker', anonymous=True)
    		rate = rospy.Rate(10) # 10hz
		while True:
			cursor = list(collection.find())
			if cursor:
				currentDocument = cursor[0]
				command = currentDocument["Command"]
				try:
					if command == "takeoff":
        					print("Drone takes off...")
						droneStarted = True 
						droneTakeoff.publish(Empty())

   					if command == "land":
        					print("Drone is landing...")
						droneStarted = False 
						droneLanding.publish(Empty())

					if command == "control" and droneStarted == True:
						print("Drone is moving...")
						x = currentDocument["Velocity_x"]
						y = currentDocument["Velocity_y"]
						z = currentDocument["Velocity_z"]
						print("x= " + x + ", y= " + y + ", z= " + z)
						if float(x) > 0.0 and float(z) < 0.0:
							#print("links_oben")
							velocity_x = -0.3 * abs(float(x))
							velocity_z = -0.3 * abs(float(z))
						elif float(x) < 0.0 and float(z) < 0.0:
							#print("rechts_oben")
							velocity_x = 0.3 * abs(float(x))
							velocity_z = -0.3 * abs(float(z))
						elif float(x) > 0.0 and float(z) > 0.0:
							#print("rechts_unten")
							velocity_x = -0.3 * abs(float(x))
							velocity_z = 0.3 * abs(float(z))
						elif float(x) < 0.0 and float(z) > 0.0:
							#print("links_unten")
							velocity_x = 0.3 * abs(float(x))
							velocity_z = 0.3 * abs(float(z))

						#velocity_x = 0.1 * float(x)
						if float(y) > 1.0:
							y = 0.5
						velocity_y = 0.3 * float(y)
						#velocity_z = 0.1 * float(z)
						droneControl.publish(Twist(Vector3(velocity_z, velocity_x, velocity_y),Vector3(0,0,0)))
						
				except rospy.ROSInterruptException:
        				pass
				_id=currentDocument["_id"]
				collection.delete_one({'_id': ObjectId(_id)})
			else:
				print("No command available!")
			time.sleep(0.1)
	except KeyboardInterrupt:
		print("Closing programm!")

