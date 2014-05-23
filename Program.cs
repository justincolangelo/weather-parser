using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace WeatherUpdater
{
    class Program
    {

        static void Main(string[] args)
        {

            try
            {
                // Get an open connection
                var conn = _GetOpenSqlConnection();

                Console.WriteLine("Opened SQL connection.");

                Console.WriteLine("Retrieving XML data from Map Click...");

                // Retrieve the xml data
                var xml = _GetMapClickXml();

                Console.WriteLine("Retrieved XML data from Map Click.");

                // Get organized data for updating the database
                Dictionary<string, string> fieldsToUpdate = ParseXmlIntoDictionary(xml);

                Console.WriteLine("Organized data in dictionary.");

                if (conn != null && fieldsToUpdate != null)
                {
                    UpdateDatabase(conn, fieldsToUpdate);
                }
                else
                {
                    Console.WriteLine("Either connection or data dictionary is null. Operation did not complete.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error.");
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }

        /*
         * Return the open sql connection
         * 
         * Example of using properties in console application to set default connection string
         * http://www.c-sharpcorner.com/UploadFile/5089e0/how-to-create-single-connection-string-in-console-applicatio/
         */
        static private SqlConnection _GetOpenSqlConnection()
        {
            // Setup connection string
            string _connectionString = Properties.Settings.Default.connectionStr;

            // Create the connection
            var _conn = new SqlConnection(_connectionString);

            // Open database connection
            Console.WriteLine("Opening SQL connection...");
            _conn.Open();

            return _conn;
        }

        /*
         * Close sql connection
         */
        static private void _CloseSqlConnection(SqlConnection conn)
        {

            // Close database connection
            Console.WriteLine("Closing SQL connection...");
            conn.Close();

        }

        /*
         * Gather JSON data from MapClick
         */
        static private XDocument _GetMapClickXml()
        {

            // Setup URL to get data
            var url =
                "http://forecast.weather.gov/MapClick.php?lat=<your_latitude>&lon=<your_longitude>&unit=0&lg=english&FcstType=dwml";

            // Create xml document to parse
            var xml = new XmlTextReader(url);
            XDocument result = XDocument.Load(xml);

            return result;
        }

        /*
         * Organize the data for updating the database
         */
        static private Dictionary<string, string> ParseXmlIntoDictionary(XDocument result)
        {
            var data = new Dictionary<string, string>{};

            var observations = (from r in result.Descendants("data").Elements("parameters")
                                where r.Parent.Attribute("type").Value == "current observations"
                                select r).ToList();

            // Current observation weather
            var weather = (from status in observations.Descendants("weather-conditions")
                           where status.Attribute("weather-summary") != null
                           select status.Attribute("weather-summary")).ToList();

            data["weatherIcon"] = weather[0].Value;

            // Current observation temperature
            var curTempList = (from temp in observations.Descendants("temperature")
                               where temp.Attribute("type").Value == "apparent"
                               select temp).ToList();

            data["villageTemp"] = curTempList[0].Value;

            // Current observation wind
            var curWindList = (from wind in observations.Descendants("direction")
                               where wind.Attribute("type").Value == "wind"
                               select wind).ToList();

            data["villageWind"] = curWindList[0].Value;

            // Current observation visibility
            var curVisibility = (from visi in observations.Descendants("weather-conditions")
                                 select visi).ToList();

            data["villageVisibility"] = curVisibility[1].Value;

            // Select conditions from the document
            var conditions = result.Descendants("weather").Elements("weather-conditions").ToList();

            // Assign weather icons
            data["todayWeatherIcon"] = conditions[0].Attribute("weather-summary").Value;
            data["tomorrowWeatherIcon"] = conditions[2].Attribute("weather-summary").Value;
            data["nextDayWeatherIcon"] = conditions[4].Attribute("weather-summary").Value;

            // Select the temperatures from the document
            var temps = result.Descendants("temperature").ToList();
            var highTemps = (from ht in temps where ht.Attribute("type").Value == "maximum" select ht).Elements("value").ToList();
            var lowTemps = (from lt in temps where lt.Attribute("type").Value == "minimum" select lt).Elements("value").ToList();

            // Assign vars for SQL Update statement
            data["todayHigh"] = highTemps[0].Value;
            data["todayLow"] = lowTemps[0].Value;
            data["tomorrowHigh"] = highTemps[1].Value;
            data["tomorrowLow"] = lowTemps[1].Value;
            data["nextDayHigh"] = highTemps[2].Value;
            data["nextDayLow"] = lowTemps[2].Value;

            // Select the text forecast for right now
            var forecastNow = (from fc in result.Descendants("data").Elements("parameters")
                                where fc.Parent.Attribute("type").Value == "forecast"
                                select fc).ToList();

            var wordedForecast = (from wf in forecastNow.Descendants("wordedForecast").Elements("text")
                                  select wf).ToList();

            data["todaysForecastComment"] = wordedForecast[0].Value;

            return data;
        }
 
        /*
         * Update the database with weather info
         */
        static private void UpdateSnowReportDatabase(SqlConnection conn, Dictionary<string, string> data )
        {
            // Sql update command string
            Console.WriteLine("Building SQL insert statement...");
            var sqlString = string.Format("UPDATE [your_database].[your_database_owner].[table] SET <some_property1> = '{0}', " +
                                          "<some_property2> = '{1}', " +
                                          "<some_property3> = '{2}', " +
                                          "<some_property4> = '{3}', " +
                                          "<some_property5> = '{4}', " +
                                          "<some_property6> = '{5}', " +
                                          "<some_property7> = '{6}', " +
                                          "<some_property8> = '{7}', " +
                                          "<some_property9> = '{8}', " +
                                          "<some_property10> = '{9}', " +
                                          "<some_property11> = '{10}', " +
                                          "<some_property12> = '{11}', " +
                                          "<some_property13> = '{12}', " +
                                          "<some_property14> = '{13}', " +
                                          "<some_property15> = '{13}'", 
                                          data["weatherIcon"].Trim(),
                                          data["todayWeatherIcon"].Trim(),
                                          data["todayHigh"].Trim(),
                                          data["todayLow"].Trim(),
                                          data["tomorrowWeatherIcon"].Trim(),
                                          data["tomorrowHigh"].Trim(),
                                          data["tomorrowLow"].Trim(),
                                          data["nextDayWeatherIcon"].Trim(),
                                          data["nextDayHigh"].Trim(),
                                          data["nextDayLow"].Trim(),
                                          data["villageTemp"].Trim(),
                                          data["villageWind"].Trim(),
                                          data["villageVisibility"].Trim(),
                                          data["todaysForecastComment"].Trim()
            );

            // Create sql
            var sql = new SqlCommand(sqlString, conn);

            // Write the data
            Console.WriteLine("Executing update query...");
            sql.ExecuteNonQuery();

            // Shut it down
            _CloseSqlConnection(conn);

        }

    }
}
