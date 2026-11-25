using ComplaintManagement.Access.Database;
using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ComplaintManagement.Access.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public FeedbackRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        // =========================================
        // CREATE
        // =========================================

        public async Task<int> CreateAsync(
            ComplaintFeedback feedback)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Feedback_Create",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@ComplaintId",
                SqlDbType.Int
            ).Value = feedback.ComplaintId;


            command.Parameters.Add(
                "@UserId",
                SqlDbType.Int
            ).Value = feedback.UserId;


            command.Parameters.Add(
                "@Rating",
                SqlDbType.Int
            ).Value = feedback.Rating;


            command.Parameters.Add(
                "@Comment",
                SqlDbType.NVarChar,
                1000
            ).Value =
                (object?)feedback.Comment
                ?? DBNull.Value;


            await connection.OpenAsync();

            object? result =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }


        // =========================================
        // GET BY COMPLAINT
        // =========================================

        public async Task<IEnumerable<ComplaintFeedback?>>
            GetByComplaintAsync(int complaintId)
        {
            List<ComplaintFeedback> feedBacks = new();
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Feedback_GetByComplaint",
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
                feedBacks.Add(
                    new ComplaintFeedback
                    {
                        Id = Convert.ToInt32(
                    reader["Id"]
                ),

                        ComplaintId =
                    Convert.ToInt32(
                        reader["ComplaintId"]
                    ),

                        UserId =
                    Convert.ToInt32(
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

                        Rating =
                    Convert.ToInt32(
                        reader["Rating"]
                    ),

                        Comment =
                    reader["Comment"] == DBNull.Value
                        ? null
                        : reader["Comment"].ToString(),

                        CreatedAt =
                    Convert.ToDateTime(
                        reader["CreatedAt"]
                    )
                    });
           
            }

            return feedBacks;

        }


        // =========================================
        // GET BY ID
        // =========================================

        public async Task<ComplaintFeedback?>
            GetByIdAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Feedback_GetById",
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

            return MapFeedback(reader);
        }


        // =========================================
        // UPDATE
        // =========================================

        public async Task<bool> UpdateAsync(
            ComplaintFeedback feedback)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Feedback_Update",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = feedback.Id;


            command.Parameters.Add(
                "@Rating",
                SqlDbType.Int
            ).Value = feedback.Rating;


            command.Parameters.Add(
                "@Comment",
                SqlDbType.NVarChar,
                1000
            ).Value =
                (object?)feedback.Comment
                ?? DBNull.Value;


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
                    "sp_Feedback_Delete",
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
        // MAP FEEDBACK
        // =========================================

        private static ComplaintFeedback MapFeedback(
            SqlDataReader reader)
        {
            return new ComplaintFeedback
            {
                Id = Convert.ToInt32(
                    reader["Id"]
                ),

                ComplaintId =
                    Convert.ToInt32(
                        reader["ComplaintId"]
                    ),

                UserId =
                    Convert.ToInt32(
                        reader["UserId"]
                    ),

                UserName =
                    reader["UserName"] == DBNull.Value
                        ? null
                        : reader["UserName"].ToString(),

                Rating =
                    Convert.ToInt32(
                        reader["Rating"]
                    ),

                Comment =
                    reader["Comment"] == DBNull.Value
                        ? null
                        : reader["Comment"].ToString(),

                CreatedAt =
                    Convert.ToDateTime(
                        reader["CreatedAt"]
                    )
            };
        }
    }
}