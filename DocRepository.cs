using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.Common;
using System.Threading;

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


  public class DocRepository {
    public string LogFile;
    ServerType _serverType;
    string _schemaPrefix;
    string _loadHeaderWithoutLockTemplate = @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WHERE ""DocName""='{1}'";
    string _loadHeaderWithReadLockTemplate;
    string _loadHeaderWithWriteLockTemplate;
    protected bool _useLocks;
    protected bool _updating; 
    protected IDbConnection _conn;
    protected IDbTransaction _trans;
    public IDbCommand LastCommand;

    public static DocRepository Create(ServerType serverType, bool useLocks, string logFile) {
      switch(serverType) {
        case ServerType.MsSql:
          return new MsSqlDocRepository(useLocks, logFile);

        case ServerType.Postgres:
          return new PostgresDocRepository(useLocks, logFile);

        case ServerType.MySql:
          return new MySqlDocRepository(useLocks, logFile);

        case ServerType.Oracle:
          return new OracleDocRepository(useLocks, logFile);

        default:
          throw new NotImplementedException("Server " + serverType + " not implemented.");
      }//switch
    }//method


    protected DocRepository(bool useLocks, string logFile, ServerType serverType, IDbConnection conn, string schemaPrefix, string loadWithReadLockTemplate = null, string loadWithWriteLockTemplate = null) {
      _useLocks = useLocks;
      LogFile = logFile;
      _serverType = serverType;
      _conn = conn;
      _schemaPrefix = schemaPrefix;
      _loadHeaderWithReadLockTemplate = loadWithReadLockTemplate ?? _loadHeaderWithoutLockTemplate;
      _loadHeaderWithWriteLockTemplate = loadWithWriteLockTemplate ?? _loadHeaderWithoutLockTemplate;
    }

    public virtual void Open(bool forUpdate) {
      _updating = forUpdate;
      _conn.Open();
      //Start transaction if we are updating data or if we use locks
      if(_updating || _useLocks) {
        var isoLevel = GetTransactionIsolationLevel();
        _trans = _conn.BeginTransaction(isoLevel);
        Log("\r\nBeginTransaction/{0}", isoLevel);
      }
    }

    // Might be overridden, servers use specific isolation levels
    protected virtual IsolationLevel GetTransactionIsolationLevel() {
      return IsolationLevel.Unspecified; 
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
          template = _loadHeaderWithWriteLockTemplate;
        else
          template = _loadHeaderWithReadLockTemplate;
      } else {
        template = _loadHeaderWithoutLockTemplate;
      }
      using(var reader = ExecuteReader(template, _schemaPrefix, docName)) {
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
      ExecuteNonQuery(template, _schemaPrefix, docName, 0);
    }

    public void DocHeaderUpdate(string docName, int total) {
      const string template = @"UPDATE {0}""DocHeader"" SET ""Total"" = {2} WHERE ""DocName"" = '{1}'";
      ExecuteNonQuery(template, _schemaPrefix, docName, total);
    }

    public void DocHeaderDelete(string docName) {
      const string template = @"DELETE FROM {0}""DocHeader"" WHERE ""DocName"" = '{1}'";
      ExecuteNonQuery(template, _schemaPrefix, docName);
    }

    public void DocHeaderDeleteAll() {
      const string template = @"DELETE FROM {0}""DocHeader""";
      ExecuteNonQuery(template, _schemaPrefix);
    }

    public IList<DocDetail> DocDetailsLoadAll(string docName) {
      var template = @"SELECT ""DocName"", ""Name"", ""Value""
FROM {0}""DocDetail"" WHERE ""DocName""='{1}'";
      var list = new List<DocDetail>();
      using(var reader = ExecuteReader(template, _schemaPrefix, docName)) {
        while(reader.Read())
          list.Add(new DocDetail() {
            DocName = docName, Name = (string) reader["Name"], Value = ToInt(reader["Value"]) });
      }
      return list; 
    }

    public DocDetail DocDetailLoad(string docName, string name) {
      var template = @"SELECT ""DocName"", ""Name"", ""Value""
FROM {0}""DocDetail"" WHERE ""DocName""='{1}' AND ""Name"" = '{2}'";
      using(var reader = ExecuteReader(template, _schemaPrefix, docName, name)) {
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
      ExecuteNonQuery(template, _schemaPrefix, docName, name, value);
    }

    public void DocDetailUpdate(string docName, string name, int value) {
      const string template = 
@"UPDATE {0}""DocDetail"" 
    SET ""Value"" = {3} WHERE ""DocName"" = '{1}' AND ""Name"" = '{2}'";
      ExecuteNonQuery(template, _schemaPrefix, docName, name, value);
    }

    public void DocDetailDelete(string docName, string name) {
      const string template = @"DELETE FROM {0}""DocDetail""WHERE ""DocName"" = '{1}' AND ""Name"" = '{2}'";
      ExecuteNonQuery(template, _schemaPrefix, docName, name);
    }
    public void DocDetailDeleteAll() {
      const string template = @"DELETE FROM {0}""DocDetail""";
      ExecuteNonQuery(template, _schemaPrefix);
    }

    protected virtual IDataReader ExecuteReader(string sqlTemplate, params object[] values) {
      //We mix thread switching into every operation
      Thread.Yield();
      var cmd = LastCommand = _conn.CreateCommand();
      cmd.Transaction = _trans;
      cmd.CommandText = string.Format(sqlTemplate, values);
      Log(cmd.CommandText);
      var reader = cmd.ExecuteReader();
      return reader;
    }

    protected virtual void ExecuteNonQuery(string sqlTemplate, params object[] values) {
      Thread.Yield();
      var cmd = LastCommand = _conn.CreateCommand();
      cmd.Transaction = _trans;
      cmd.CommandText = string.Format(sqlTemplate, values);
      Log(cmd.CommandText);
      cmd.ExecuteNonQuery();
    }

    //Oracle repo overrides this
    protected virtual int ToInt(object value) {
      return (int)value;
    }

  }//class
}
