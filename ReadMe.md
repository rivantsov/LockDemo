# Database Locking Demo application
This small application is a sample code for the (upcoming) article about using database locks to ensure consistent loading and update of complex documents in relational databases. The app works several servers: MS SQL Server, MySql, Postgres, Oracle. 
Before you run the application, you need to prepare the database:
* Choose a server type and create LockTest database on (local) target server. (For MySql and Oracle - skip this, choose the target server installation)
* Open SQL Browser app (SQL Management Studio, SQL browser) and run DDL script from DDLscripts folder to create database tables
* Adjust connection string for target server in the app.config file. 
* Modify serverType variable variable in Program.cs
Run the app. Turn on/off 'useLocks' variable and see the effects (errors) on the app execution. 

# What the app is doing
The app works with 'documents' consisting of a header record (DocHeader table) and multiple DocDetail records. Each detail row contains 'Value' (int) column. DocHeader has a 'Total' column that is a sum of all values in related detail rows - this is a 'consistency' requirement.
The app initially creates several documents with multiple detail records each. The test starts 20+ threads, each repeating multple times a random operation over a randomly chosen document. An operation might be one the following: 
* Update - choose 3 random child detail rows, update them with random value in 'Value' column. Calculate new total and update the header row. 
* Read and check the total - load header and all detail rows for a random document, verify that total matches the sum of detail rows. If not - record it as a consistency check failure. 
When the tests are executed without locks, we get multiple deadlock and consistency load errors. If we use locks, the tests should run without any errors. 

**The purpose of the app is to demonstrate the correct locking implementation for consistent document read and update operations.** 
