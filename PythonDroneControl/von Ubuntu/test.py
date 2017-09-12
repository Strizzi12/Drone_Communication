import pymongo

from pymongo import MongoClient
client = MongoClient('193.171.53.35', 27017)

db = client['DroneDB']

collection = db['DroneCommands']

cursor = collection.find()

for document in cursor:
	print(document)


