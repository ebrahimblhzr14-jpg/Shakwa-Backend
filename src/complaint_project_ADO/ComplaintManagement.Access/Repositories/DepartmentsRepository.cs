using ComplaintManagement.Access.Database;
using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ComplaintManagement.Access.Repositories
{
    public class DepartmentsRepository : IDepartmentsRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public DepartmentsRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        // =========================================
        // CREATE
        // =========================================

        public async Task<int> CreateAsync(
            Department department)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Departments_Create",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Name",
                SqlDbType.NVarChar,
                100
            ).Value = department.Name;

            command.Parameters.Add(
                "@Description",
                SqlDbType.NVarChar,
                500
            ).Value =
                (object?)department.Description
                ?? DBNull.Value;

            await connection.OpenAsync();

            object? result =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }


        // =========================================
        // GET ALL
        // =========================================

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            List<Department> departments = new();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Departments_GetAll",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                departments.Add(new Department
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
                });
            }

            return departments;
        }


        // =========================================
        // GET BY ID
        // =========================================

        public async Task<Department?> GetByIdAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Departments_GetById",
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

            return new Department
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
            Department department)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Departments_Update",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = department.Id;

            command.Parameters.Add(
                "@Name",
                SqlDbType.NVarChar,
                100
            ).Value = department.Name;

            command.Parameters.Add(
                "@Description",
                SqlDbType.NVarChar,
                500
            ).Value =
                (object?)department.Description
                ?? DBNull.Value;

            command.Parameters.Add(
                "@IsActive",
                SqlDbType.Bit
            ).Value = department.IsActive;

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
                    "sp_Departments_Delete",
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
    }
}