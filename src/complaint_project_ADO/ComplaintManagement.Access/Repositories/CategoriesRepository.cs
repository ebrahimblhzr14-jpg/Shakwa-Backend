using ComplaintManagement.Access.Database;
using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ComplaintManagement.Access.Repositories
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public CategoriesRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        // =========================================
        // CREATE
        // =========================================

        public async Task<int> CreateAsync(
            ComplaintCategory category)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Categories_Create",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Name",
                SqlDbType.NVarChar,
                100
            ).Value = category.Name;

            command.Parameters.Add(
                "@Description",
                SqlDbType.NVarChar,
                500
            ).Value =
                (object?)category.Description
                ?? DBNull.Value;

            await connection.OpenAsync();

            object? result =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }


        // =========================================
        // GET ALL
        // =========================================

        public async Task<IEnumerable<ComplaintCategory>>
            GetAllAsync()
        {
            List<ComplaintCategory> categories = new();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Categories_GetAll",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                categories.Add(
                    new ComplaintCategory
                    {
                        Id = Convert.ToInt32(
                            reader["Id"]
                        ),

                        Name = reader["Name"]
                            .ToString()!,

                        Description =
                            reader["Description"] == DBNull.Value
                                ? null
                                : reader["Description"]
                                    .ToString(),

                        IsActive = Convert.ToBoolean(
                            reader["IsActive"]
                        )
                    });
            }

            return categories;
        }


        // =========================================
        // GET BY ID
        // =========================================

        public async Task<ComplaintCategory?>
            GetByIdAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Categories_GetById",
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

            return new ComplaintCategory
            {
                Id = Convert.ToInt32(
                    reader["Id"]
                ),

                Name = reader["Name"]
                    .ToString()!,

                Description =
                    reader["Description"] == DBNull.Value
                        ? null
                        : reader["Description"].ToString(),

                IsActive = Convert.ToBoolean(
                    reader["IsActive"]
                )
            };
        }


        // =========================================
        // UPDATE
        // =========================================

        public async Task<bool> UpdateAsync(
            ComplaintCategory category)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Categories_Update",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = category.Id;

            command.Parameters.Add(
                "@Name",
                SqlDbType.NVarChar,
                100
            ).Value = category.Name;

            command.Parameters.Add(
                "@Description",
                SqlDbType.NVarChar,
                500
            ).Value =
                (object?)category.Description
                ?? DBNull.Value;

            command.Parameters.Add(
                "@IsActive",
                SqlDbType.Bit
            ).Value = category.IsActive;

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
                    "sp_Categories_Delete",
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
                    "sp_Categories_Activate",
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
                    "sp_Categories_Deactivate",
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