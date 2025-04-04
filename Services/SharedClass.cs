using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Services
{
    public class SharedClass
    {
        IConfiguration _configuration;
        public SharedClass(IConfiguration configuration)
        {

            _configuration = configuration;

        }

        public async Task<DataTable> GetTableAsync(string query, bool isStoredProcedure, object? model = null, string? dbConn = null)
        {
            DataTable dt = new DataTable();
            try
            {
                string? _connectionString = _configuration.GetValue<string>("ConnectionStrings:CoverMate");

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync(); // Asynchronous connection open

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandType = isStoredProcedure ? CommandType.StoredProcedure : CommandType.Text;

                        // Handle parameters if the model is provided
                        if (model != null)
                        {
                            foreach (PropertyInfo prop in model.GetType().GetProperties())
                            {
                                string propName = prop.Name;
                                object value = prop.GetValue(model);
                                cmd.Parameters.AddWithValue("@" + propName, value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader rdr = await cmd.ExecuteReaderAsync())
                        {
                            dt.Load(rdr); // Asynchronous DataTable load
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                // Services.ExecJS(String.Format("console.error('[{0}] {1}: {2}');", Environment.MachineName, ex.GetType().ToString(), ex.Message.Replace("'", "\"")));
            }

            return dt;
        }

        /// <summary>
        /// Shared class global function used to get data from database. This can also be used to INSERT and UPDATE query with returned values.
        /// </summary>
        /// <returns>Returns Datatable type</returns>
        public async Task ExecuteQueryAsync(string query, bool isStoredProcedure, object? model = null, string? dbConn = null)
        {
            try
            {
                string? _connectionString = _configuration.GetValue<string>("ConnectionStrings:CoverMate");


                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        if (isStoredProcedure)
                            cmd.CommandType = CommandType.StoredProcedure;

                        if (model != null)
                        {
                            foreach (PropertyInfo prop in model.GetType().GetProperties())
                            {
                                string propName = prop.Name;
                                object value = prop.GetValue(model);
                                cmd.Parameters.AddWithValue("@" + propName, value);
                            }
                        }

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
            }
        }


    }
}
