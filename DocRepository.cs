using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.Common;
using System.Threading;
using System.IO;

namespace LockDemo {

  public class DocHeader {
    public string DocName;
    public int Total;
  }
  public class DocDetail {
    public string DocName;
    public string Name;
    public int Value;
  }


  public abstract class DocRepository {
    public ServerType ServerType; 
    public string SchemaPrefix;
    public string LogFile;
    public string SqlTemplateLoadHeaderWithoutLock = @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WHERE ""DocName""='{1}'";
    public string SqlTemplateLoadHeaderWithReadLock;
    public string SqlTemplateLoadHeaderWithWriteLock;
    public IsolationLevel ReadIsolationLevel = IsolationLevel.Unspecified;
    public IsolationLevel WriteIsolationLevel = IsolationLevel.Unspecified; 


    protected IDbConnection _conn;
    protected bool _useLocks;

    protected IDbTransaction _trans;
    protected bool _updating; 
    public IDbCommand LastCommand;

    protected DocRepository(ServerType serverType, IDbConnection conn, string schemaPrefix, bool useLocks, string logFile) {
      ServerType = serverType;
      _conn = conn;
      SchemaPrefix = schemaPrefix; 
      _useLocks = useLocks;
      LogFile = logFile;
      if(File.Exists(logFile))
        File.Delete(logFile);
      SqlTemplateLoadHeaderWithReadLock = SqlTemplateLoadHeaderWithoutLock;
    }

    public virtual void Open(bool forUpdate) {
      _updating = forUpdate;
      _conn.Open();
      //Start transaction if we are updating data or if we use locks
      if(_updating || _useLocks) {
        var isoLevel = _updating ? WriteIsolationLevel : ReadIsolationLevel;
        _trans = _conn.BeginTransaction(isoLevel);
        Log("\r\nBeginTransaction/{0}", isoLevel);
      }
    }

    public void Close(bool error = false) {
      if(_trans != null)
        if(error)
          try { _trans.Rollback(); Log("Rollback"); } catch { } 
        else 
        { _trans.Commit(); Log("Commit"); }
      _trans = null;
      _conn.Close();
    }

    public void Log(string msg, params object[] args) {
      if(args != null && args.Length > 0)
        msg = string.Format(msg, args);
      System.IO.File.AppendAllText(LogFile, msg + Environment.NewLine); 
    }


    public virtual DocHeader DocHeaderLoad(string docName) {
      string template;
      DocHeader doc; 
      if(_useLocks) {
        if(_updating)
          template = SqlTemplateLoadHeaderWithWriteLock;
        else
          template = SqlTemplateLoadHeaderWithReadLock;
      } else {
        template = SqlTemplateLoadHeaderWithoutLock;
      }
      using(var reader = ExecuteReader(template, SchemaPrefix, docName)) {
        if(!reader.Read())
          return null;
        doc = new DocHeader() { DocName = docName, Total = ToInt(reader["Total"]) };
      }
      return doc; 
    }

    public void DocHeaderInsert(string docName) {
      const string template =
@"INSERT INTO {0}""DocHeader"" 
   (""DocName"", ""Total"") VALUES ('{1}', {2})";
      ExecuteNonQuery(template, SchemaPrefix, docName, 0);
    }

    public void DocHeaderUpdate(string docName, int total) {
      const string template = @"UPDATE {0}""DocHeader"" SET ""Total"" = {2} WHERE ""DocName"" = '{1}'";
      ExecuteNonQuery(template, SchemaPrefix, docName, total);
    }

    public void DocHeaderDelete(string docName) {
      const string template = @"DELETE FROM {0}""DocHeader"" WHERE ""DocName"" = '{1}'";
      ExecuteNonQuery(template, SchemaPrefix, docName);
    }

    public void DocHeaderDeleteAll() {
      const string template = @"DELETE FROM {0}""DocHeader""";
      ExecuteNonQuery(template, SchemaPrefix);
    }

    public IList<DocDetail> DocDetailsLoadAll(string docName) {
      var template = @"SELECT ""DocName"", ""Name"", ""Value""
FROM {0}""DocDetail"" WHERE ""DocName""='{1}'";
      var list = new List<DocDetail>();
      using(var reader = ExecuteReader(template, SchemaPrefix, docName)) {
        while(reader.Read())
          list.Add(new DocDetail() {
            DocName = docName, Name = (string) reader["Name"], Value = ToInt(reader["Value"]) });
      }
      return list; 
    }

    public DocDetail DocDetailLoad(string docName, string name) {
      var template = @"SELECT ""DocName"", ""Name"", ""Value""
FROM {0}""DocDetail"" WHERE ""DocName""='{1}' AND ""Name"" = '{2}'";
      using(var reader = ExecuteReader(template, SchemaPrefix, docName, name)) {
        if(reader.Read())
          return new DocDetail() {
            DocName = docName, Name = (string)reader["Name"], Value = ToInt(reader["Value"])
          };
        else
          return null;
      }
    }

    public void DocDetailInsert(string docName, string name, int value) {
      const string template =
@"INSERT INTO {0}""DocDetail"" 
   (""DocName"", ""Name"", ""Value"") VALUES ('{1}', '{2}', {3})";
      ExecuteNonQuery(template, SchemaPrefix, docName, name, value);
    }

    public void DocDetailUpdate(string docName, string name, int value) {
      const string template = 
@"UPDATE {0}""DocDetail"" 
    SET ""Value"" = {3} WHERE ""DocName"" = '{1}' AND ""Name"" = '{2}'";
      ExecuteNonQuery(template, SchemaPrefix, docName, name, value);
    }

    public void DocDetailDelete(string docName, string name) {
      const string template = @"DELETE FROM {0}""DocDetail""WHERE ""DocName"" = '{1}' AND ""Name"" = '{2}'";
      ExecuteNonQuery(template, SchemaPrefix, docName, name);
    }
    public void DocDetailDeleteAll() {
      const string template = @"DELETE FROM {0}""DocDetail""";
      ExecuteNonQuery(template, SchemaPrefix);
    }

    private IDataReader ExecuteReader(string sqlTemplate, params object[] values) {
      //We mix thread switching into every operation
      Thread.Yield();
      var cmd = LastCommand = _conn.CreateCommand();
      cmd.Transaction = _trans;
      cmd.CommandText = PreviewSql(string.Format(sqlTemplate, values));
      Log(cmd.CommandText);
      var reader = cmd.ExecuteReader();
      return reader;
    }

    private void ExecuteNonQuery(string sqlTemplate, params object[] values) {
      //We mix thread switching into every operation
      Thread.Yield();
      var cmd = LastCommand = _conn.CreateCommand();
      cmd.Transaction = _trans;
      cmd.CommandText = PreviewSql(string.Format(sqlTemplate, values));
      Log(cmd.CommandText);
      cmd.ExecuteNonQuery();
    }

    //Oracle repo overrides these
    protected virtual string PreviewSql(string sql) {
      return sql;
    }
    protected virtual int ToInt(object value) {
      return (int)value;
    }

  }//class
}
