using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

  public class MsSqlDocRepository : DocRepository {
    public MsSqlDocRepository(bool useLocks, string logFile)
      : base(useLocks, logFile, ServerType.MsSql, 
          conn: new System.Data.SqlClient.SqlConnection(ConfigurationManager.AppSettings["MsSqlConnectionString"]), 
          schemaPrefix: "dbo.",
          loadWithReadLockTemplate: null, //same as load without lock, with snapshot isolation we don't any specific hints
          loadWithWriteLockTemplate: @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WITH(UpdLock) WHERE ""DocName""='{1}'"
          ) { }

    protected override IsolationLevel GetTransactionIsolationLevel() {
      return _updating ? IsolationLevel.ReadCommitted : IsolationLevel.Snapshot;
    }

  }//class

  public class PostgresDocRepository : DocRepository {
    public PostgresDocRepository(bool useLocks, string logFile)
      : base(useLocks, logFile, ServerType.Postgres,
          conn: new Npgsql.NpgsqlConnection(ConfigurationManager.AppSettings["PostgresConnectionString"]),
          schemaPrefix: "lck.",
          loadWithReadLockTemplate: @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WHERE ""DocName""='{1}' FOR SHARE", 
          loadWithWriteLockTemplate: @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WHERE ""DocName""='{1}' FOR UPDATE"
          ) { }

  }//class

  public class MySqlDocRepository : DocRepository {
    public MySqlDocRepository(bool useLocks, string logFile)
      : base(useLocks, logFile, ServerType.MySql,
          conn: new MySql.Data.MySqlClient.MySqlConnection(ConfigurationManager.AppSettings["MySqlConnectionString"]),
          schemaPrefix: "lck.",
          loadWithReadLockTemplate: @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WHERE ""DocName""='{1}' LOCK IN SHARE MODE",
          loadWithWriteLockTemplate: @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WHERE ""DocName""='{1}' FOR UPDATE"
          ) { }

  }//class

  public class OracleDocRepository : DocRepository {
    public OracleDocRepository(bool useLocks, string logFile)
      : base(useLocks, logFile, ServerType.Oracle,
          conn: new Oracle.ManagedDataAccess.Client.OracleConnection(ConfigurationManager.AppSettings["OracleConnectionString"]),
          schemaPrefix: string.Empty,
          loadWithReadLockTemplate: null, //same as load without lock
          loadWithWriteLockTemplate: @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WHERE ""DocName""='{1}' FOR UPDATE"
          ) { }

    protected override IsolationLevel GetTransactionIsolationLevel() {
      // for Oracle - read needs Serializable; with ReadCommitted we get inconsistent read errors
      return _updating ? IsolationLevel.ReadCommitted : IsolationLevel.Serializable;
    }

    //Oracle does not like quoted identifiers - it treats quotes as part of names
    protected override IDataReader ExecuteReader(string sqlTemplate, params object[] values) {
      return base.ExecuteReader(sqlTemplate.Replace("\"", string.Empty), values);
    }
    protected override void ExecuteNonQuery(string sqlTemplate, params object[] values) {
      base.ExecuteNonQuery(sqlTemplate.Replace("\"", string.Empty), values);
    }

    //Oracle uses/returns decimals in int columns
    protected override int ToInt(object value) {
      return (int)Convert.ChangeType(value, typeof(int));
    }
  }//class



}//ns
