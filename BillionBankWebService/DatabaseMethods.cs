/*
 * Programmer: Stefan Reesberg
 * Purpose: This class handles commonly requested methods for ease of use throughout the webservice application.
 * Date: 04/10/2022
 * 
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OleDb;


namespace BillionBankWebService
{
    public class DatabaseMethods
    {
        OleDbConnection connection;                                                                              //Create a variable to set up the connection to the database.
        
        public void ConnectToDatabase()                                                                          //This method handles connecting to the Web Service database.
        {
            string relativePath = HttpContext.Current.Server.MapPath("BillionBank.accdb");                       //Define the relative path the application will use to find the database from any location.
            connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + relativePath);  //Complete the connection setup once relative path is defined.
            
            connection.Open();                                                                                   //Open the connection to the database.
        }

        public void DisconnectDatabase()                                                                         //This method handles closing the connection to the database.
        {
            connection.Close();                                                                                  //Closes the connection to the database.
        }

        public bool ExecuteCommand(String query)                                                                 //This method executes non-queries directed at the database. (e.g INSERT)
        {
            try                                                                                                  //Insurance against possible exceptions caused when method is invoked.
            {
                OleDbCommand cmd = connection.CreateCommand();                                                   //Create a new OleDbCommand object, using the established connection.
                cmd.CommandText = query;                                                                         //Command text is set to the value of the query parameter.
                cmd.ExecuteNonQuery();                                                                           //Executes the non-query.
                return true;                                                                                     //return true if succeeded.
            }
            catch
            {
                return false;                                                                                    //return false if an exception occurred and was caught.
            }
        }
        public OleDbDataReader ExecuteQuery(String query)                                                        //This method executes queries directed at the database. (e.g SELECT)
        {
            try
            {
                OleDbCommand cmd = connection.CreateCommand();                                                   //Create a new OleDbCommand object, using the established connection.
                cmd.CommandText = query;                                                                         //Command text is set to the value of the query parameter.
                return cmd.ExecuteReader();                                                                      //Sends the CommandText to the Connection and builds a SqlDataReader. Returns SqlDataReader object.
            }
            catch
            {
                return null;                                                                                     //Return null if an exception is caught.
            }
        }
        public void CreateLog(string logFileName)                                                                //This method handles the creation of a log file when invoked.
        {
            string directory = HttpContext.Current.Server.MapPath("Transaction Log");                            //Maps the relative path to the Transaction Log folder regardless of application location.
            string logFile = directory + "/" + logFileName;                                                      //Points to save location for individual log files e.g "Transaction Log/stefan@mail.com.log"
            if (!File.Exists(logFile))                                                                           //Check if file does not exist.
            {
                var file = File.Create(logFile);                                                                 //Create the file.
                file.Close();                                                                                    //Close the file.
            }
        }
    }
}