using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

string _connectionString = "Data Source =.; Integrated Security = True; Initial Catalog = RepeatableReadData; Encrypt = false";

Console.WriteLine("write number 1:\n");
int numberToInsert1 = int.Parse(Console.ReadLine()!);
Console.WriteLine("write number 2:\n");
int numberToInsert2 = int.Parse(Console.ReadLine()!);

Thread t1 = new Thread(() => RunTransaction(numberToInsert1, _connectionString));
Thread t2 = new Thread(() => RunTransaction(numberToInsert2, _connectionString));
t1.Name = "t1";
t2.Name = "t2";
t1.Start();
t2.Start();

Console.WriteLine("press anything to stop...");
Console.ReadLine();

void RunTransaction(int number, string connectionString)
{
    using IDbConnection dbConnection = new SqlConnection(connectionString);
    dbConnection.Open();
    using IDbTransaction trans = dbConnection.BeginTransaction(IsolationLevel.Serializable);

    try
    {
        int resultId = -1;
        if (!NumberExists(number, trans))
        {
            Console.WriteLine("Im in! - " + Thread.CurrentThread.Name); // for testing
            string query = "INSERT INTO Numbers (number) OUTPUT INSERTED.id values (@number);";
            resultId = dbConnection.QuerySingle<int>(query, new { number }, trans);
            trans.Commit();
        }
        Console.WriteLine(resultId + " - " + Thread.CurrentThread.Name);
    }
    catch (Exception e)
    {
        trans.Rollback();
        Console.WriteLine($"Error creating a new number in the database - {Thread.CurrentThread.Name}. Error is: '{e.Message}'");
    }
    dbConnection.Close();
}

bool NumberExists(int numberToCheck, IDbTransaction transaction)
{
    string query = "select count(number) from Numbers where number=@number";
    return transaction.Connection.Execute(query, new {number = numberToCheck}, transaction) > 0;
}