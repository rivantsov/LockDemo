using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockDemo {

  public class MsSqlDocRepository : DocRepository {
    public MsSqlDocRepository(string connString, bool useLocks, string logFile)
                  : base(ServerType.MsSql, new System.Data.SqlClient.SqlConnection(connString), "dbo.", useLocks,logFile) {
      base.SqlTemplateLoadHeaderWithWriteLock =
           @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WITH(UpdLock) WHERE ""DocName""='{1}'";
      base.ReadIsolationLevel = IsolationLevel.Snapshot;
      base.WriteIsolationLevel = IsolationLevel.ReadCommitted; 
    }
  }//class

  public class PostgresDocRepository : DocRepository {
    public PostgresDocRepository(string connString, bool useLocks, string logFile)
                  : base(ServerType.Postgres, new Npgsql.NpgsqlConnection(connString), "lck.", useLocks, logFile) {
      base.SqlTemplateLoadHeaderWithReadLock = @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WHERE ""DocName""='{1}' FOR SHARE";
      base.SqlTemplateLoadHeaderWithWriteLock = @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WHERE ""DocName""='{1}' FOR UPDATE";
    }
  }//class

  public class MySqlDocRepository : DocRepository {
    public MySqlDocRepository(string connString, bool useLocks, string logFile)
          : base(ServerType.MySql, new MySql.Data.MySqlClient.MySqlConnection(connString), "lck.", useLocks, logFile) {
      base.SqlTemplateLoadHeaderWithReadLock = @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WHERE ""DocName""='{1}' LOCK IN SHARE MODE";
      base.SqlTemplateLoadHeaderWithWriteLock = @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WHERE ""DocName""='{1}' FOR UPDATE";
    }
  }//class

  public class OracleDocRepository : DocRepository {
    public OracleDocRepository(string connString, bool useLocks, string logFile)
      : base(ServerType.Oracle, new Oracle.ManagedDataAccess.Client.OracleConnection(connString), "", useLocks, logFile
          ) {
      base.SqlTemplateLoadHeaderWithWriteLock = @"SELECT ""DocName"", ""Total"" FROM {0}""DocHeader"" WHERE ""DocName""='{1}' FOR UPDATE";
      base.ReadIsolationLevel = IsolationLevel.Serializable;
      base.WriteIsolationLevel = IsolationLevel.ReadCommitted; 
    }

    //Oracle does not like quoted identifiers - it treats quotes as part of names
    protected override string PreviewSql(string sql) {
      return sql.Replace("\"", string.Empty);
    }
    //Oracle uses/returns decimals in int columns
    protected override int ToInt(object value) {
      return (int)Convert.ChangeType(value, typeof(int));
    }
  }//class



}//ns
