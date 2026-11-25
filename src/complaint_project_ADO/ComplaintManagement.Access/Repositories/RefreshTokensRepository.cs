using ComplaintManagement.Access.Database;
using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ComplaintManagement.Access.Repositories
{
    public class RefreshTokensRepository
        : IRefreshTokensRepository
    {
        private readonly SqlConnectionFactory
            _connectionFactory;

        public RefreshTokensRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory;
        }


        // =========================================
        // CREATE
        // =========================================

        public async Task<int> CreateAsync(
            RefreshToken refreshToken)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_RefreshTokens_Create",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@UserId",
                SqlDbType.Int
            ).Value =
                refreshToken.UserId;


            command.Parameters.Add(
                "@Token",
                SqlDbType.NVarChar,
                500
            ).Value =
                refreshToken.Token;


            command.Parameters.Add(
                "@ExpiresAt",
                SqlDbType.DateTime2
            ).Value =
                refreshToken.ExpiresAt;


            await connection.OpenAsync();

            object? result =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }


        // =========================================
        // GET BY TOKEN
        // =========================================

        public async Task<RefreshToken?>
            GetByTokenAsync(string token)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_RefreshTokens_GetByToken",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@Token",
                SqlDbType.NVarChar,
                500
            ).Value = token;


            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;


            return MapRefreshToken(reader);
        }


        // =========================================
        // REVOKE
        // =========================================

        public async Task<bool> RevokeAsync(
            int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_RefreshTokens_Revoke",
                    connection);

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


        // =========================================
        // MAPPING
        // =========================================

        private static RefreshToken
            MapRefreshToken(
                SqlDataReader reader)
        {
            return new RefreshToken
            {
                Id =
                    Convert.ToInt32(
                        reader["Id"]),

                UserId =
                    Convert.ToInt32(
                        reader["UserId"]),

                Token =
                    reader["Token"]
                        .ToString()!,

                ExpiresAt =
                    Convert.ToDateTime(
                        reader["ExpiresAt"]),

                RevokedAt =
                    reader["RevokedAt"]
                    == DBNull.Value
                        ? null
                        : Convert.ToDateTime(
                            reader["RevokedAt"]),

                CreatedAt =
                    Convert.ToDateTime(
                        reader["CreatedAt"])
            };
        }
    }
}