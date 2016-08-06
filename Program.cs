using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockDemo {
  class Program {
    static void Main(string[] args) {
      // If useLocks=true, test should complete without errors.
      // Try running with 'useLocks:false' to see errors - deadlocks and inconsistent reads
      var serverType = ServerType.MsSql;
      var useLocks = true;
      var threadCount = 30;
      var repeatCount = 40;
      Console.WriteLine(string.Format("Running locking demo." +
          "\r\n Server type: {0}, thread count: {1}, repeat count: {2}, use locks: {3}", 
        serverType, threadCount, repeatCount, useLocks));
      var runner = new TestRunner(serverType, useLocks, repeatCount, threadCount);
      runner.RunParallelRandomOps();
      Console.WriteLine();
      Console.WriteLine("=========================================================================");
      Console.WriteLine(string.Format("Done. DB errors/deadlocks: {0}, Inconsistent reads: : {1}", 
        runner.DbErrorCount, runner.InconsistentReadCount));
      Console.WriteLine("Press any key.");
      Console.ReadKey(); 
    }
  }
}
