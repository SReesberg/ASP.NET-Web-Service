/*
 * Programmer: Stefan Reesberg
 * Purpose: This application is responsible for all database-related and additional functionality concerning the connected Interface application.
 * Date: 04/10/2022
 * 
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.OleDb;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections;

namespace BillionBankWebService
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://BillionBanksWS.org")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        
        [WebMethod]                                                             
        public void AddUserDetails(string name, string surname, string ID,    //This WebMethod receives all the user-entered data from the client and inserts it into the relevant database table.                      
            string email, string contactNum, string secQ,                     //Also creates a log file for every new user that registers.
            string secA, string physAdd, string pw)
        {
            try                                                               //Insurance against unexpected exceptions.
            {
                DatabaseMethods dm = new DatabaseMethods();                   //Create an object of the DatabaseMethods class.

                dm.ConnectToDatabase();                                       //Connect to the database.

                dm.CreateLog(email + ".log");                                 //Create the log file.

                //Executes the non-query, inserts all entered data into the database.
                dm.ExecuteCommand("INSERT INTO [User](userID, userName, userSurname, userEmail, userContactNumber, userSecretQuestion, userSecretAnswer,userPhysicalAddress,userPassword) "
                    + "Values('" + ID + "','" + name + "', '" + surname + "', '" + email + "', '" + contactNum + "', '" + secQ + "', '" + secA + "', '" + physAdd + "', '" + pw + "');");

                dm.DisconnectDatabase();                                      //Disconnects the database after command is executed.
            }
            catch(OleDbException e)
            {
               Console.WriteLine(e.Message);                                  //Display exception description.
            }
             
        }

        [WebMethod(EnableSession =true)]
        public string LogIn(string email, string password)                    //This WebMethod handles the login process by checking if the e-mail and password entered matches the records in the database.
        {
            try                                                               //Insurance against unexpected exceptions.
            {
                DatabaseMethods dm = new DatabaseMethods();                   //Creation of class object.

                dm.ConnectToDatabase();                                       //Connect to the database.

                //Assign the SQL query to a variable. This query selects all records from the User table where the email and password matches up.
                string queryString = "SELECT * FROM [User] WHERE userEmail= '" + email + "' AND userPassword= '" + password + "';";

                OleDbDataReader dbReader = dm.ExecuteQuery(queryString);      //Create a new OleDbDataReader object that executes the query saved to the variable.

                

                if (dbReader != null && dbReader.HasRows)                     //Checks if the reader does not return null and that it contains rows - meaning the query was successful.
                {

                    dm.DisconnectDatabase();                                  //Disconnect the database.
                    return email;                                             //Return the e-mail parameter untouched to the client for use in the Session variable creation.

                }
                else                                                          //Otherwise set the e-mail to "0" and return it. This will cause the client to recognize that no matches were found.
                {
                    email = "0";
                    dm.DisconnectDatabase();                                 
                    return email;
                }
            }
            catch
            {
               
                return "ERROR";                                              //Returns this string in order to trigger a response client-side.
            }

            
        }


        [WebMethod]
        public List<string> GetQuestion(string email)                         //This WebMethod retrieves the user's secret question for display in the client interface, provided the e-mail matches.
        {
            try                                                               //Insures against unexpected exceptions.
            {
                DatabaseMethods dm = new DatabaseMethods();                   //Creation of new class object.

                dm.ConnectToDatabase();                                       //Connect to database.

                //Assign the SQL query to a variable.
                string queryDisplayQuestion = "SELECT userSecretQuestion FROM [User] WHERE userID= (SELECT userID FROM [User] WHERE userEmail= '" + email + "');";

                List<string> question = new List<string>();                   //Create a new List of strings to save the results of the query.

                //Create a OleDbDataReader that executes the query.
                using (OleDbDataReader reader = dm.ExecuteQuery(queryDisplayQuestion))
                {
                    while (reader.Read())                                     //While the reader Reads, add the result to the question List.
                    {
                        question.Add((string)reader["userSecretQuestion"]);

                    }
                }

                dm.DisconnectDatabase();                                      //Disconnect the database.

                return question;                                              //Return the question List.
            }
            catch(OleDbException e)
            {
                Console.WriteLine(e.Message);                                 //Display exception description.
                return null;                                                  //Return a null value to the client interface.
            }
        }


        [WebMethod]

        public string CheckPassword(string answer, string email)             //This WebMethod checks if the entered secret answer and the e-mail address corresponds with the database records.
        {
            try                                                              //Insures against unexpected exceptions.
            {
                DatabaseMethods dm = new DatabaseMethods();                  //Creation of new class object.

                dm.ConnectToDatabase();                                      //Connect to database.

                //Query is saved to a variable.
                string queryString = "SELECT userSecretAnswer FROM [User] WHERE userSecretAnswer= '" + answer + "' AND userID= (SELECT userID FROM [User] WHERE userEmail= '" + email + "');";

                OleDbDataReader dbReader = dm.ExecuteQuery(queryString);     //Create a OleDbDataReader that executes the query.

                

                if (dbReader != null && dbReader.HasRows)                    //Checks if the reader does not return null and that it contains rows - meaning the query was successful.
                {
                    answer = email;                                          //Change answer parameter to e-mail and return it.
                    dm.DisconnectDatabase();                                 //Disconnect the database.
                    return answer;
                }
                else
                {
                    answer = "MISMATCH";
                    dm.DisconnectDatabase();                                 
                    return answer;                                           //Leave answer untouched and return it, causing the check to fail client-side.
                }

            }
            catch(OleDbException e)                                          
            {
                Console.WriteLine(e.Message);                                //Display exception description.
                return answer;                                               //return untouched answer, ensuring check fails.
            }
        }


        [WebMethod]
        public void SetPassword(string sessionvar, string password)          //This WebMethod updates the user's password in the database.
        {
            try                                                              //Insures against unexpected exceptions.
            {
                DatabaseMethods dm = new DatabaseMethods();                  //Creation of new class object.

                dm.ConnectToDatabase();                                      //Connect to database.

                //Executes the command, updating the password.
                dm.ExecuteCommand("UPDATE [User] SET userPassword= '" + password + "' WHERE userID= (SELECT userID FROM [User] WHERE userEmail= '" + sessionvar + "');");

                dm.DisconnectDatabase();                                     //Disconnect the database.
            }
            catch(OleDbException e)
            {
                Console.WriteLine(e.Message);                                //Display caught exception description.
            }
        }

        [WebMethod]

        public void CreateAccount(string sessionvar,string accountType)      //This WebMethod handles the creation of user accounts, the first account per user receives R100, each subsequent account receives nothing.
        {
            try                                                              //Insures against unexpected exceptions.
            {
                DatabaseMethods dm = new DatabaseMethods();                  //Creation of new class object.

                dm.ConnectToDatabase();                                      //Connect to the database by invoking the method.

                DateTime creation = DateTime.Now;                            //Create a DateTime variable that stores the current time, expressed in local time.

                //Save the query in a string variable for later use. This query selects all records from the Account table where the userID is equal to the userID linked to the unique email of the user.
                string queryString = "SELECT * FROM [Account] WHERE userID= (SELECT userID FROM [User] WHERE userEmail= '" + sessionvar + "');";

                OleDbDataReader dbReader = dm.ExecuteQuery(queryString);     //Create a OleDbDataReader that executes the first query.

                //Create another query selecting the userID linked to the user E-mail passed as the session variable.
                string queryID = "SELECT userID FROM [User] WHERE userEmail= '" + sessionvar + "';";

                string id = "";                                              //Declare a string variable that holds an empty string.

                using (OleDbDataReader reader = dm.ExecuteQuery(queryID))    //Execute the second query using a reader.
                {
                    while (reader.Read())                                    //While reader Reads, equate the result to the previously declared string variable. This works because the result of the query can only be one record.
                    {
                        id = reader["userID"].ToString();
                    }
                }

                decimal balance;                                            //Declare an empty decimal variable.

                if (dbReader != null && dbReader.HasRows)                   //If the results of the first reader are not null and the reader has rows:
                {

                    balance = 0;                                            //No money is added because the user has a previously created account.
                }
                else                                                        //If the reader has no entries:
                {
                    balance = 100;                                          //Add a 100 currency to the created account, because it is the first.

                }

                if (id != "")                                               //As long as the id variable is not empty - meaning that a userID is present to be linked with the account in either case - execute the command.
                {
                    //Inserts the new account details into the Account table of the database.
                    dm.ExecuteCommand("INSERT INTO [Account](userID, creationDate, currentBalance, accountType) VALUES('" + id + "', '" + creation.ToString() + "', " + balance + ", '" + accountType + "');");
                }
                dm.DisconnectDatabase();                                    //Disconnect from the database.
            }
            catch(OleDbException e)
            {
                Console.WriteLine(e.Message);                               //Display the description of the exception once it's caught.
            }
        }

        [WebMethod]

        public List<int> GetAccounts(string sessionvar)                     //This WebMethod handles the retrieval of Account Numbers linked to the user by using the session variable to query the database.
        {
            try                                                             //Pre-empts an exception.  
            {
                DatabaseMethods dm = new DatabaseMethods();                 //Creation of a new class object.

                dm.ConnectToDatabase();                                     //Connect to the database.

                //Save SQL query to the string variable. This query selects accountNumbers from the Account table where the user's email matches the session variable.
                string query = "SELECT accountNumber FROM Account WHERE userID= (SELECT userID FROM [User] WHERE userEmail= '" + sessionvar + "');";

                List<int> accounts = new List<int>();                       //Create a List of integers that will store the returned Account Numbers.

                using (OleDbDataReader reader = dm.ExecuteQuery(query))     //Create a reader that executes the query.
                {
                    while (reader.Read())                                   //While the reader Reads.
                    {
                        accounts.Add((int)reader["accountNumber"]);         //Add each result to the accounts List.
                    }
                }

                dm.DisconnectDatabase();                                    //Disconnect the database.

                return accounts;                                            //Return the accounts List.
                
            }
            catch
            {
                return null;                                                //Return null if an exception is caught.
            }
        }
        
        [WebMethod]
        public List<string> PopulateList(string sessionvar,                 //This WebMethod populates the Account ListBox by querying the database for Account Numbers matching the unique e-mail of the user.
            int accountNumber)
        {
            try
            {
                DatabaseMethods dm = new DatabaseMethods();                  //Creation of a new class object.

                dm.ConnectToDatabase();                                      //Connect to the database.

                //Declare a string that is used to store the SQL statement. Selects all records from Account table where userID matches userID linked with the email stored in the session variable and the account number.
                string query = "SELECT * FROM [Account] WHERE userID= (SELECT userID FROM [User] WHERE userEmail= '" + sessionvar + "') AND accountNumber= " + accountNumber + ";";

                List<string> accountFields = new List<string>();             //Create a List of strings that can hold the returned query's fields.

                using (OleDbDataReader reader = dm.ExecuteQuery(query))      //Create a reader that executes the query and reads the result.
                {
                    while (reader.Read())                                    //While the reader reads, add each result to the accountFields List, then return the List.
                    {
                        accountFields.Add(reader["accountNumber"].ToString());
                        accountFields.Add(reader["creationDate"].ToString());
                        accountFields.Add(reader["currentBalance"].ToString());
                        accountFields.Add((string)reader["userID"]);
                        accountFields.Add((string)reader["accountType"]);
                    }
                }
                dm.DisconnectDatabase();                                    //Disconnects the database.

                return accountFields;
            }
            catch
            {
                return null;                                                  //Return null when an exception is caught.
            }
        }

        [WebMethod]        
        public List<decimal> GetCurrentBalance(string sessionvar              //This WebMethod returns the selected account's current balance.
            ,int selectedAccount) 
        {
            try                                                               //Start of try block for exception catching.
            {
                DatabaseMethods dm = new DatabaseMethods();                   //Creation of new class object.

                dm.ConnectToDatabase();                                       //Connect to the database.

                //Store query in string variable. This query selects the current balance from the specified account.
                string query = "SELECT currentBalance FROM [Account] WHERE userID= (SELECT userID FROM [User] WHERE userEmail= '" + sessionvar + "') AND accountNumber= " + selectedAccount + ";";

                List<decimal> currentBalance = new List<decimal>();           //Declare a List of decimals to store the current balance.

                using (OleDbDataReader reader = dm.ExecuteQuery(query))       //Create a reader that reads the query results.
                {
                    while (reader.Read())
                    {
                        currentBalance.Add((decimal)reader["currentBalance"]);//While the reader Reads, add the result to the currentBalance List.
                    }
                }

                dm.DisconnectDatabase();                                      //Disconnect from database.

                return currentBalance;                                        //Return the List.
            }
            catch
            {
                return null;                                                  //Return null if exception is caught.
            }
        }

        [WebMethod]
        
        public void TransferMoney(int destinationAcc, int sourceAccount,     //This WebMethod transfers currency from one account to another.
            decimal amountToTransfer, decimal currentBalanceSource, decimal currentBalanceDest)
        {
            try                                                              //Start of try block to catch exceptions.
            {
                DatabaseMethods dm = new DatabaseMethods();                  //Creation of new class object.

                dm.ConnectToDatabase();                                      //Connect to the database.

                //Execute a non-query. This command will insert the relevant transaction details into the Transaction table of the database.
                dm.ExecuteCommand("INSERT INTO[Transactions] (sourceAccount, destinationAccount, transferAmount, accountNumber) VALUES ('" + sourceAccount.ToString() + "', '" + destinationAcc.ToString() + "', " + amountToTransfer + " , " + sourceAccount + ")");


                decimal resultSub = currentBalanceSource - amountToTransfer;//Subtract the transfer amount from the source account's balance.

                decimal resultAdd = currentBalanceDest + amountToTransfer;  //Add the transferred amount to the destination account's balance.

                //This query updates the relevant account's current balance post subtraction.
                string querySubtract = "UPDATE [Account] SET currentBalance= " + resultSub + " WHERE accountNumber= " + sourceAccount + ";";

                //This query updates the relevant account's current balance post addition.
                string queryAdd = "UPDATE [Account] SET currentBalance= " + resultAdd + " WHERE accountNumber= " + destinationAcc + ";";

                dm.ExecuteQuery(querySubtract);                             //Execute subtraction query.
                dm.ExecuteQuery(queryAdd);                                  //Execute addition query.

                dm.DisconnectDatabase();                                    //Disconnects the database.
            }
            catch(OleDbException e)
            {
                Console.WriteLine(e.Message);                               //If an exception is caught, display exception description.
            }
        }

        [WebMethod]

        public List<string> GetRecentTransaction(int accountNr)            //This WebMethod retrieves the relevant account's most recent transaction.
        {
            try                                                            //Start of try block for catching exceptions.
            {
                DatabaseMethods dm = new DatabaseMethods();                //Creation of new class object.

                dm.ConnectToDatabase();                                    //Connect to database.

                List<string> transDetails = new List<string>();            //Create List of strings to store transaction details for display.

                //Store SQL query in string variable. Selects the singular top record from Transactions table ordered by descending where the accountNumber matches.
                string query = "SELECT TOP 1 * FROM [Transactions] WHERE accountNumber= " + accountNr + " ORDER BY transactionID DESC;";

                using (OleDbDataReader reader = dm.ExecuteQuery(query))    //Creates a reader that executes and reads the results of the query.
                {
                    while (reader.Read())                                  //While the reader Reads, add each result to the List.
                    {
                        transDetails.Add(reader["transactionID"].ToString());
                        transDetails.Add(reader["sourceAccount"].ToString());
                        transDetails.Add(reader["destinationAccount"].ToString());
                        transDetails.Add(reader["transferAmount"].ToString());
                        transDetails.Add(reader["accountNumber"].ToString());

                    }
                }
                dm.DisconnectDatabase();                                   //Disconnect database.

                return transDetails;                                       //Return the List.
            }
            catch
            {
                return null;                                               //Return null if an exception is caught.
            }
        }

        [WebMethod]
        public void UpdateDetails(string sessionvar, string name,         //This WebMethod updates the user's details in the database.
            string surname, string email, string contactNum, string secretQ, string secretA, string physAdd)
        {
            try                                                           //Start of try block for catching exceptions.
            {
                DatabaseMethods dm = new DatabaseMethods();               //Creation of new class object.

                dm.ConnectToDatabase();                                   //Connect to database.

                //Store SQL query in string variable. Query updates each specified column in the relevant record with the changes.
                string query = "UPDATE [User] SET userName= '" + name + "', userSurname= '" + surname + "', userEmail= '" + email + "', userContactNumber= '" + contactNum + "', userSecretQuestion= '" + secretQ + "', userSecretAnswer= '" + secretA + "', userPhysicalAddress= '" + physAdd + "' WHERE userID= (SELECT userID FROM [User] WHERE userEmail= '" + sessionvar + "'); ";
                
                dm.ExecuteCommand(query);                                 //Execute the query.

                dm.DisconnectDatabase();                                  //Disconnect the database.
            }
            catch(OleDbException e)
            {
                Console.WriteLine(e.Message);                             //If an exception is caught, display description of exception.
            }
        }

        [WebMethod]
        public void WriteToLog(int destinationAcc, int sourceAccount,    //This WebMethod writes each transaction to a unique log file tied to the user's e-mail address.
            decimal amountToTransfer, string logFileName)
        {
            try                                                          //Start of try block for exception catching.
            {
                DatabaseMethods dm = new DatabaseMethods();              //Creation of new class object.

                string directory = HttpContext.Current.Server.MapPath("Transaction Log");//Maps the relative path to the Transaction Log folder regardless of app location.

                string logFile = directory + "/" + logFileName;          //Specify Log File's location.

                if (!File.Exists(logFile))                               //If the file does not exist, create it.    
                    dm.CreateLog(logFileName);

                StreamWriter sw = new StreamWriter(logFile, true);       //Create a streamwriter that appends, not replaces.

                //Write the specified details to the log file.
                sw.WriteLine("[Transaction Details]\n\nFrom Account Number: {0}  \nTo Account Number: {1}  \nAmount Transferred: {2}",
                    sourceAccount.ToString(), destinationAcc.ToString(), amountToTransfer.ToString());

                sw.WriteLine();                                         //Adds two clear lines between each log.
                sw.WriteLine();

                sw.Dispose();                                           //Release all resources used by StreamWriter.
                sw.Close();                                             //Close the stream.
            }
            catch(OleDbException e)
            {
                Console.WriteLine(e.Message);                           //Display exception description if caught.
            }

        }






    }
}
