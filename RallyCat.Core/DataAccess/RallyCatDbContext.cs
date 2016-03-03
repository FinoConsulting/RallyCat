using System;
using System.Data;
using FluentData;


namespace RallyCat.Core.DataAccess
{
    public class RallyCatDbContext
    {
        private static String _ConnectionStringName;

        public static void SetConnectionString(String connectionStringName)
        {
            _ConnectionStringName = connectionStringName;
        }

        public static IDbContext QueryDb()
        {
            if (_ConnectionStringName == null) { throw new DataException("Please set ConnectionStringName first"); }
            return new DbContext().ConnectionStringName(_ConnectionStringName, new SqlServerProvider());
        }
    }
}