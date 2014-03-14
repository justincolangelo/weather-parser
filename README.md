C# weather-parser
=================

Retrieve data from weather.gov. Parse out some of the data using XDocument and Linq for saving to a database. 

Create a new console application project in Visual Studio. Replace what parts of the Program.cs file in this repository that you would like to use. 
When you build the application, an exe file will be created. I needed the weather to be automated for saving to a database. 
This allowed for me to use a scheduled task to run this operation.

Go to http://www.weather.gov/ and enter your location. You will be sent to page where you can click an orange XML button. This will provide you with the url you need for the XML parser. 

You will need to set your connection string. I would recommend using this example to create a default connection string in the properties of the project.

http://www.c-sharpcorner.com/UploadFile/5089e0/how-to-create-single-connection-string-in-console-applicatio/

