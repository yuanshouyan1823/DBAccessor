using System;
using System.Data.SqlClient;
using System.Data;
using System.Data.OracleClient;
using System.Data.SQLite;
using System.Collections;
using System.Collections.Generic;

namespace DBAccessor
{
	public class DataOperator : IDisposable
	{
		#region 构造和析构
		public DataOperator(DBInstance dbInstance)
		{
			this.dbInstance = dbInstance;
			DbConnection.Open();
		}

		private bool disposed;
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (DbConnection.State != ConnectionState.Closed)
						DbConnection.Close();
				}
				disposed = true;
			}
		}

		~DataOperator()
		{
			Dispose(false);
		}
		#endregion

		private IDbConnection dbConnection;

		private DBInstance dbInstance;

		public String DataSoureName => dbInstance.DBSoureName;

		public String ConnectionString => dbInstance.ConnectionString;

		public IDbConnection DbConnection
		{
			get
			{
				if (dbConnection == null)
				{
					switch (dbInstance.DatabaseType)
					{
						case DatabaseType.ORACLE:
							dbConnection = new OracleConnection(dbInstance.ConnectionString);
							break;
						case DatabaseType.SQLSERVER:
							dbConnection = new SqlConnection(dbInstance.ConnectionString);
							break;
						case DatabaseType.SQLITE:
							dbConnection = new SQLiteConnection(dbInstance.ConnectionString);
							break;
						default:
							throw new Exception("Unsupported DB Type!");
					}
				}
				return dbConnection;
			}
		}

		private void PrepareCommand(IDbCommand command, CommandType commandType, string sql, IDbDataParameter[] parms)
		{
			if (command == null)
			{
				throw new ArgumentNullException("IDbCommand", "空参数异常!");
			}
			command.CommandText = sql;
			command.CommandType = commandType;
			if (parms != null && parms.Length > 0)
			{
				foreach (var parameter in parms)
				{
					if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && (parameter.Value == null))
					{
						parameter.Value = DBNull.Value;
					}
					command.Parameters.Add(parameter);
				}
			}
		}

		public DataTable ExecuteQuery(string sql, CommandType commandType = CommandType.Text, params IDbDataParameter[] parameter)
		{
			using (IDbCommand cmd = DbConnection.CreateCommand())
			{
				PrepareCommand(cmd, commandType, sql, parameter);
				DataTable tab = new DataTable();
				using (IDataReader reader = cmd.ExecuteReader())
				{
					tab.Load(reader);
					return tab;
				}
			}
		}

		public int ExecuteNonQuery(string sql, CommandType commandType = CommandType.Text, params IDbDataParameter[] parameter)
		{
			using (IDbCommand cmd = DbConnection.CreateCommand())
			{
				PrepareCommand(cmd, commandType, sql, parameter);
				return cmd.ExecuteNonQuery();
			}
		}

		public object ExecuteScalar(string sql, CommandType commandType = CommandType.Text, params IDbDataParameter[] parameter)
		{
			using (IDbCommand cmd = DbConnection.CreateCommand())
			{
				PrepareCommand(cmd, commandType, sql, parameter);
				return cmd.ExecuteScalar();
			}
		}

		public IDataReader ExecuteReader(string sql, CommandType commandType = CommandType.Text, params IDbDataParameter[] parameter)
		{
			using (IDbCommand cmd = DbConnection.CreateCommand())
			{
				PrepareCommand(cmd, commandType, sql, parameter);
				return cmd.ExecuteReader();//CommandBehavior.CloseConnection
			}
		}

		public bool ExecuteTransaction(Hashtable sqlList)
		{
			using (IDbTransaction trans = DbConnection.BeginTransaction())
			{
				try
				{
					using (IDbCommand cmd = DbConnection.CreateCommand())
					{
						foreach (DictionaryEntry item in sqlList)
						{
							cmd.Parameters.Clear();
							cmd.CommandText = item.Key.ToString();
							if (item.Value != null)
							{
								var parameter = item.Value as IDbDataParameter[];
								for (int i = 0; i < parameter.Length && parameter != null; i++)
								{
									cmd.Parameters.Add(parameter[i]);
								}
							}

							cmd.Transaction = trans;
							cmd.Connection = DbConnection;
							cmd.ExecuteNonQuery();
						}
						trans.Commit();
						return true;
					}
				}
				catch (Exception ex)
				{
					trans.Rollback();
					throw ex;
				}
			}
		}

		public bool ExecuteTransaction(IEnumerable<Tuple<string, CommandType, IDbDataParameter[]>> sqlList)
		{
			using (IDbTransaction trans = DbConnection.BeginTransaction())
			{
				try
				{
					using (IDbCommand cmd = DbConnection.CreateCommand())
					{
						foreach (var item in sqlList)
						{
							cmd.Parameters.Clear();
							cmd.CommandText = item.Item1.ToString();
							cmd.CommandType = item.Item2;
							if (item.Item3 != null)
							{
								var parameter = item.Item3;
								for (int i = 0; i < parameter.Length && parameter != null; i++)
								{
									cmd.Parameters.Add(parameter[i]);
								}
							}

							cmd.Transaction = trans;
							cmd.Connection = DbConnection;
							cmd.ExecuteNonQuery();
						}
						trans.Commit();
						return true;
					}
				}
				catch (Exception ex)
				{
					trans.Rollback();
					throw ex;
				}
			}
		}

	}
}