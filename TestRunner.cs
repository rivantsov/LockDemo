using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace LockDemo {
  public class TestRunner {
    public ServerType ServerType; 
    public bool UseLocks;
    public int RepeatCount;
    public int ThreadCount; 
    public int InconsistentReadCount = 0;
    public int DbErrorCount = 0;


    public TestRunner(ServerType serverType, bool useLocks, int repeatCount = 100, int threadCount = 20) {
      ServerType = serverType;
      UseLocks = useLocks;
      RepeatCount = repeatCount;
      ThreadCount = threadCount; 
    }

    public void RunParallelRandomOps() {
      if(!InitData())
        return; 
      //Create 
      var tasks = new List<Task>();
      for(int i=0; i < ThreadCount; i++) {
        var logFile = "sqllog_" + i + ".log";
        if(File.Exists(logFile))
          File.Delete(logFile); 
        var repo = DocRepository.Create(ServerType, UseLocks, logFile);
        var task = new Task(RunRandomOps, repo);
        tasks.Add(task);
        task.Start(); 
      }
      Task.WaitAll(tasks.ToArray());
    }//method

    private bool InitData() {
      // create document headers and details
      var repo = DocRepository.Create(ServerType, useLocks: false, logFile: "init.log");
      try {
        repo.Open(forUpdate: true);
        repo.DocDetailDeleteAll();
        repo.DocHeaderDeleteAll();
        for(int i = 0; i < 5; i++) {
          var docName = "D" + i;
          repo.DocHeaderInsert(docName);
          for(int j = 0; j < 5; j++)
            repo.DocDetailInsert(docName, "V" + j, 0);
        }
        repo.Close();
        return true; 
      } catch(Exception ex) {
        Console.WriteLine("InitData error: " + ex.ToString());
        Console.WriteLine("--- SQL: " + repo.LastCommand.CommandText);
        return false;
      }
    }

    private void RunRandomOps(object objRepo) {
      var repo = (DocRepository)objRepo;
      var rand = new Random();
      DocHeader doc;
      int total; 
      for(int i = 0; i < RepeatCount; i++) {
        if(i % 5 == 0)
          Console.Write("."); //show progress
        Thread.Yield();
        var op = rand.Next(2);
        var docName = "D" + rand.Next(5);
        try {
          switch(op) {
            case 0: //update several random detail rows
              repo.Open(forUpdate: true);
              if(UseLocks)
                doc = repo.DocHeaderLoad(docName);
              repo.DocDetailUpdate(docName, "V" + rand.Next(5), rand.Next(10));
              repo.DocDetailUpdate(docName, "V" + rand.Next(5), rand.Next(10));
              repo.DocDetailUpdate(docName, "V" + rand.Next(5), rand.Next(10));
              // Recalc total and update header
              total = repo.DocDetailsLoadAll(docName).Sum(d => d.Value);
              repo.DocHeaderUpdate(docName, total);
              repo.Close();
              break;

            case 1: //load doc, verify total
              repo.Open(forUpdate: false);
              doc = repo.DocHeaderLoad(docName);
              total = repo.DocDetailsLoadAll(docName).Sum(d => d.Value);
              if(total != doc.Total) {
                Interlocked.Increment(ref InconsistentReadCount);
                var msg = "\r\n--- Inconsistent read; doc.Total: " + doc.Total + ", sum(det): " + total;
                Console.WriteLine(msg); 
                repo.Log(msg);
              }
              repo.Close();
              break;
          }//switch
        } catch(Exception ex) {
          //database error, most often deadlock
          Interlocked.Increment(ref DbErrorCount);
          Console.WriteLine("\r\n--- DB error: " + ex.Message);
          repo.Log("Db error ------------------------------------------------------- \r\n" + ex.ToString() + "\r\n");
          repo.Close(error: true);
        }
      }//for i

    }//method

  }
}
