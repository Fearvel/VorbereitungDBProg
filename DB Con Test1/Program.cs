using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Con_Test1
{
    class Program
    {
        static void Main(string[] args)
        {
            AdventureWorks2014Entities ctx =  new AdventureWorks2014Entities();
            NorthwindEntities ctxN = new NorthwindEntities();


            var con = (SqlConnection)ctx.Database.Connection;
            var conN = ctxN.Database.Connection;

            SqlCommand cmd = new SqlCommand("Select * from [AdventureWorks2014].[Person].[Address] where Addressid =1", con);
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            var dr = cmd.ExecuteReader();
            con.Close();
            var da = new SqlDataAdapter();
            da.SelectCommand =cmd;
           var ds = new DataSet();
            da.Fill(ds);
            var dt = ds.Tables[0];
            var c = dt.Rows.Count;
            var s = dt.Rows[0][1]; 
            var ad =  ctx.Address.ToList().Where(
                x => x.AddressID.Equals(1));
            var e = ad.GetEnumerator();
            var list =  new List<Address>();
            e.MoveNext();

            while (e.Current != null)
            {
                list.Add(e.Current);
                e.MoveNext();
            }

            List<Address> l = ctx.Address.ToList();
        }
    }
}
