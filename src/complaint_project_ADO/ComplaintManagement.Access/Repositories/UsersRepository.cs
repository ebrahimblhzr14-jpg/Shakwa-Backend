using ComplaintManagement.Access.Database;
using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ComplaintManagement.Access.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public UsersRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // =========================================
        // CREATE
        // =========================================

        public async Task<int> CreateAsync(User user)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand("sp_Users_Create", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Name",
                SqlDbType.NVarChar,
                100
            ).Value = user.Name;

            command.Parameters.Add(
                "@Email",
                SqlDbType.NVarChar,
                255
            ).Value = user.Email;

            command.Parameters.Add(
                "@PasswordHash",
                SqlDbType.NVarChar,
                500
            ).Value = user.PasswordHash;

            command.Parameters.Add(
                "@Phone",
                SqlDbType.NVarChar,
                30
            ).Value = (object?)user.Phone ?? DBNull.Value;

            command.Parameters.Add(
                "@RoleId",
                SqlDbType.Int
            ).Value = user.RoleId;

            command.Parameters.Add(
                "@ImageUrl",
                SqlDbType.NVarChar,
                2048
            ).Value = (object?)user.ImageUrl ?? DBNull.Value;

            await connection.OpenAsync();

            object? result =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }


        // =========================================
        // GET ALL
        // =========================================

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            List<User> users = new();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand("sp_Users_GetAll", connection);

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = Convert.ToInt32(
                        reader["Id"]
                    ),

                    Name = reader["Name"]
                        .ToString()!,

                    Email = reader["Email"]
                        .ToString()!,

                    Phone = reader["Phone"] == DBNull.Value
                        ? null
                        : reader["Phone"].ToString(),

                    ImageUrl = reader["ImageUrl"] == DBNull.Value
                        ? null
                        : reader["ImageUrl"].ToString(),

                    RoleId = Convert.ToInt32(
                        reader["RoleId"]
                    ),

                    RoleName = reader["RoleName"] == DBNull.Value
                        ? null
                        : reader["RoleName"].ToString(),

                    IsActive = Convert.ToBoolean(
                        reader["IsActive"]
                    ),

                    CreatedAt = Convert.ToDateTime(
                        reader["CreatedAt"]
                    )
                });
            }

            return users;
        }


        // =========================================
        // GET BY ID
        // =========================================

        public async Task<User?> GetByIdAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand("sp_Users_GetById", connection);

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

            return new User
            {
                Id = Convert.ToInt32(
                    reader["Id"]
                ),

                Name = reader["Name"]
                    .ToString()!,

                Email = reader["Email"]
                    .ToString()!,

                Phone = reader["Phone"] == DBNull.Value
                    ? null
                    : reader["Phone"].ToString(),

                ImageUrl = reader["ImageUrl"] == DBNull.Value
                    ? null
                    : reader["ImageUrl"].ToString(),

                RoleId = Convert.ToInt32(
                    reader["RoleId"]
                ),

                RoleName = reader["RoleName"] == DBNull.Value
                    ? null
                    : reader["RoleName"].ToString(),

                IsActive = Convert.ToBoolean(
                    reader["IsActive"]
                ),

                CreatedAt = Convert.ToDateTime(
                    reader["CreatedAt"]
                )
            };
        }


        // =========================================
        // GET BY EMAIL
        // =========================================

        public async Task<User?> GetByEmailAsync(string email)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Users_GetByEmail",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Email",
                SqlDbType.NVarChar,
                255
            ).Value = email;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new User
            {
                Id = Convert.ToInt32(
                    reader["Id"]
                ),

                Name = reader["Name"]
                    .ToString()!,

                Email = reader["Email"]
                    .ToString()!,

                PasswordHash = reader["PasswordHash"]
                    .ToString()!,

                Phone = reader["Phone"] == DBNull.Value
                    ? null
                    : reader["Phone"].ToString(),

                ImageUrl = reader["ImageUrl"] == DBNull.Value
                    ? null
                    : reader["ImageUrl"].ToString(),

                RoleId = Convert.ToInt32(
                    reader["RoleId"]
                ),

                RoleName = reader["RoleName"] == DBNull.Value
                    ? null
                    : reader["RoleName"].ToString(),

                IsActive = Convert.ToBoolean(
                    reader["IsActive"]
                ),

                CreatedAt = Convert.ToDateTime(
                    reader["CreatedAt"]
                )
            };
        }


        // =========================================
        // UPDATE
        // =========================================

        public async Task<bool> UpdateAsync(User user)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Users_Update",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = user.Id;

            command.Parameters.Add(
                "@Name",
                SqlDbType.NVarChar,
                100
            ).Value = user.Name;

            command.Parameters.Add(
                "@Email",
                SqlDbType.NVarChar,
                255
            ).Value = user.Email;

            command.Parameters.Add(
                "@Phone",
                SqlDbType.NVarChar,
                30
            ).Value =
                (object?)user.Phone ?? DBNull.Value;

            command.Parameters.Add(
                "@RoleId",
                SqlDbType.Int
            ).Value = user.RoleId;

            command.Parameters.Add(
                "@IsActive",
                SqlDbType.Bit
            ).Value = user.IsActive;

            command.Parameters.Add(
                "@ImageUrl",
                SqlDbType.NVarChar,
                2048
            ).Value = (object?)user.ImageUrl ?? DBNull.Value;

            await connection.OpenAsync();

            int rowsAffected =
                await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }


        // =========================================
        // UPDATE PASSWORD
        // =========================================

        public async Task<bool> UpdatePasswordAsync(int id,string passwordHash)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Users_UpdatePassword",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = id;

            command.Parameters.Add(
                "@PasswordHash",
                SqlDbType.NVarChar,
                500
            ).Value = passwordHash;

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
                    "sp_Users_Delete",
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


        public async Task<bool> DeactivateAsync(
    int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Users_Deactivate",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = id;

            await connection.OpenAsync();

            object? result =
                await command.ExecuteScalarAsync();

            int rowsAffected =
                Convert.ToInt32(result);

            return rowsAffected > 0;
        }



        public async Task<bool> ActivateAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Users_Activate",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = id;

            await connection.OpenAsync();

            object? result =
                await command.ExecuteScalarAsync();

            int rowsAffected =
                Convert.ToInt32(result);

            return rowsAffected > 0;
        }

        public async Task<bool> PutDepartmentAsync(int userID,int DepID)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Users_PutDepartment",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = userID;
            command.Parameters.Add(
                "@DepID",
                SqlDbType.Int
            ).Value = DepID;

            await connection.OpenAsync();

            object? result =
                await command.ExecuteNonQueryAsync();

            int rowsAffected =
                Convert.ToInt32(result);

            return rowsAffected > 0;
        }
    }
}