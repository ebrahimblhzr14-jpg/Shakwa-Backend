using ComplaintManagement.Access.Database;
using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ComplaintManagement.Access.Repositories
{
    public class StatusesRepository : IStatusesRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public StatusesRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        // =========================================
        // CREATE
        // =========================================

        public async Task<int> CreateAsync(
            ComplaintStatus status)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Statuses_Create",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Name",
                SqlDbType.NVarChar,
                50
            ).Value = status.Name;

            await connection.OpenAsync();

            object? result =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }


        // =========================================
        // GET ALL
        // =========================================

        public async Task<IEnumerable<ComplaintStatus>>
            GetAllAsync()
        {
            List<ComplaintStatus> statuses = new();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Statuses_GetAll",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                statuses.Add(
                    new ComplaintStatus
                    {
                        Id = Convert.ToInt32(
                            reader["Id"]
                        ),

                        Name = reader["Name"]
                            .ToString()!
                    });
            }

            return statuses;
        }


        // =========================================
        // GET BY ID
        // =========================================

        public async Task<ComplaintStatus?>
            GetByIdAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Statuses_GetById",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = id;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new ComplaintStatus
            {
                Id = Convert.ToInt32(
                    reader["Id"]
                ),

                Name = reader["Name"]
                    .ToString()!
            };
        }


        // =========================================
        // UPDATE
        // =========================================

        public async Task<bool> UpdateAsync(
            ComplaintStatus status)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Statuses_Update",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = status.Id;

            command.Parameters.Add(
                "@Name",
                SqlDbType.NVarChar,
                50
            ).Value = status.Name;

            await connection.OpenAsync();

            int rowsAffected =
                await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }


        // =========================================
        // DELETE
        // =========================================

        public async Task<bool> DeleteAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Statuses_Delete",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = id;

            await connection.OpenAsync();

            int rowsAffected =
                await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }



        public async Task<bool> ActivateAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Statuses_Activate",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Id",
                SqlDbType.Int).Value = id;

            await connection.OpenAsync();

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Statuses_Deactivate",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Id",
                SqlDbType.Int).Value = id;

            await connection.OpenAsync();

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }




    }
}