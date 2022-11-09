BillionBankWebService : The Web Service application that handles all website and database-related functionality.
The goal of these two applications in tandem is to simulate a banking application where the user can sign up for membership, log in, create accounts and perform transactions. Once the user is finished, they can log out. The ability to view their account details, edit user details and displaying recent transactions is also fully realized.
Starting with the “back-end”, BillionBankWebService features Web Methods that are responsible for:
•	Inserting the user details submitted through the interface into the database.
•	Verifying that the entered log-in details match with database records.
•	Querying the database to display the user’s secret question.
•	Verifying the secret answer entered by the user.
•	Updating the user’s password in the database.
•	Creating a bank account for the user.
•	Retrieving accounts linked to the user.
•	Populating list boxes with account details for display.
•	Determining the current balance of any account linked to the user.
•	Transferring currency from one account to another.
•	Displaying a selected account’s most recent transaction.
•	Updating the user’s details in the database.
•	Creating and writing to a Log file for each transaction made by the user.
The Web Service also includes a class called DatabaseMethods that is frequently instantiated due to the methods defined inside being commonly used throughout the application. These include:
•	Connecting to the database.
•	Disconnecting from the database.
•	Executing non-queries.
•	Executing queries.
•	Creating a log file.
The Web Service includes both the Transaction Log folder and the Database file.
