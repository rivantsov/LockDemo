using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockDemo {

  public enum ServerType {
    MsSql,
    MySql,
    Postgres,
    Oracle
  }

  class Program {
    public static ServerType ServerType;
    public static string ConnectionString;
    public static bool UseLocks;
    public static int ThreadCount;
    public static int RepeatCount;

    static void Main(string[] args) {
      try {
        // If useLocks=true, test should complete without errors.
        // Try running with 'useLocks:false' to see errors - deadlocks and inconsistent reads
        LoadConfig(); 
        Console.WriteLine(string.Format("Running locking demo." +
            "\r\n Server type: {0}, thread count: {1}, repeat count: {2}, use locks: {3}",
          ServerType, ThreadCount, RepeatCount, UseLocks));
        var runner = new TestRunner();
        runner.RunParallelRandomOps();
        Console.WriteLine();
        Console.WriteLine("=========================================================================");
        Console.WriteLine(string.Format("Done. DB errors/deadlocks: {0}, Inconsistent reads: : {1}",
          runner.DbErrorCount, runner.InconsistentReadCount));
      } catch (Exception ex) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: " + ex.ToString());
        Console.ForegroundColor = ConsoleColor.White;
      }
      Console.WriteLine("Press any key.");
      Console.ReadKey(); 
    }

    private static void LoadConfig() {
      var strServerType = ConfigurationManager.AppSettings["ServerType"];
      if(!Enum.TryParse(strServerType, out ServerType))
        throw new Exception("Config error, invalid server type: " + strServerType);
      ConnectionString = ConfigurationManager.AppSettings[ServerType + "ConnectionString"];
      UseLocks = ConfigurationManager.AppSettings["UseLocks"] == "true";
      ThreadCount = int.Parse(ConfigurationManager.AppSettings["ThreadCount"]);
      RepeatCount = int.Parse(ConfigurationManager.AppSettings["RepeatCount"]);
    }


  }
}
