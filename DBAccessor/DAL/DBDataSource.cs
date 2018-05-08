using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DBAccessor
{

	[Serializable]
	public class DBInstance
	{
		public DatabaseType DatabaseType;
		public string ConnectionString;
		public string DBSoureName;
	}

	public enum DatabaseType
	{
		ORACLE = 0,
		SQLSERVER = 1,
		SQLITE = 2
	}

	public static class DataSoureNames
	{
		public static readonly string OA = "OA";
		public static readonly string HR = "HR";
		public static readonly string X3 = "X3";
		public static readonly string SAP = "SAP";
		public static readonly string PLM = "PLM";
		public static readonly string MIS = "MIS";
		public static readonly string MES = "MES";
		public static readonly string LocalDB = "LocalDB";
	}

	public static class DataSoure
	{
		public static DBInstance OA => DBInstancelist?.Find(a => a.DBSoureName == DataSoureNames.OA);
		public static DBInstance HR => DBInstancelist?.Find(a => a.DBSoureName == DataSoureNames.HR);
		public static DBInstance SAP => DBInstancelist?.Find(a => a.DBSoureName == DataSoureNames.SAP);
		public static DBInstance PLM => DBInstancelist?.Find(a => a.DBSoureName == DataSoureNames.PLM);
		public static DBInstance MIS => DBInstancelist?.Find(a => a.DBSoureName == DataSoureNames.MIS);
		public static DBInstance MES => DBInstancelist?.Find(a => a.DBSoureName == DataSoureNames.MES);
		public static DBInstance LocalDB => DBInstancelist?.Find(a => a.DBSoureName == DataSoureNames.LocalDB);

		static string GetConnectionString(String providerName)
		{
			var dbInstance = DBInstancelist?.Find(a => a.DBSoureName == providerName);
			return dbInstance == null ? String.Empty : dbInstance.ConnectionString;
		}

		static List<DBInstance> DBInstancelist;
		static DataSoure()
		{
			XmlSerializer xs = new XmlSerializer(typeof(List<DBInstance>));
			using (Stream stream = new FileStream("DbConfig.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				DBInstancelist = xs.Deserialize(stream) as List<DBInstance>;
			}
		}
	}
}
