using ComplaintManagement.Access.Database;
using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ComplaintManagement.Access.Repositories
{
    public class CommentsRepository : ICommentsRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public CommentsRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        // =========================================
        // CREATE
        // =========================================

        public async Task<int> CreateAsync(
            ComplaintComment comment)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Comments_Create",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@ComplaintId",
                SqlDbType.Int
            ).Value = comment.ComplaintId;


            command.Parameters.Add(
                "@UserId",
                SqlDbType.Int
            ).Value = comment.UserId;


            command.Parameters.Add(
                "@Comment",
                SqlDbType.NVarChar
            ).Value = comment.Comment;


            await connection.OpenAsync();

            object? result =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }


        // =========================================
        // GET BY COMPLAINT
        // =========================================

        public async Task<IEnumerable<ComplaintComment>>
            GetByComplaintAsync(int complaintId)
        {
            List<ComplaintComment> comments = new();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Comments_GetByComplaint",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@ComplaintId",
                SqlDbType.Int
            ).Value = complaintId;


            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                comments.Add(
                    MapComment(reader)
                );
            }

            return comments;
        }


        // =========================================
        // GET BY ID
        // =========================================

        public async Task<ComplaintComment?>
            GetByIdAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Comments_GetById",
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

            return MapComment(reader);
        }


        // =========================================
        // UPDATE
        // =========================================

        public async Task<bool> UpdateAsync(
            ComplaintComment comment)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Comments_Update",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = comment.Id;


            command.Parameters.Add(
                "@Comment",
                SqlDbType.NVarChar
            ).Value = comment.Comment;


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
                    "sp_Comments_Delete",
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


        // =========================================
        // MAP COMMENT
        // =========================================

        private static ComplaintComment MapComment(
            SqlDataReader reader)
        {
            return new ComplaintComment
            {
                Id = Convert.ToInt32(
                    reader["Id"]
                ),

                ComplaintId = Convert.ToInt32(
                    reader["ComplaintId"]
                ),

                UserId = Convert.ToInt32(
                    reader["UserId"]
                ),

                UserName =
                    reader["UserName"] == DBNull.Value
                        ? null
                        : reader["UserName"].ToString(),

                UserImageUrl =
                    reader["UserImageUrl"] == DBNull.Value
                        ? null
                        : reader["UserImageUrl"].ToString(),

                Comment = reader["Comment"]
                    .ToString()!,

                CreatedAt =
                    Convert.ToDateTime(
                        reader["CreatedAt"]
                    )
            };
        }
    }
}