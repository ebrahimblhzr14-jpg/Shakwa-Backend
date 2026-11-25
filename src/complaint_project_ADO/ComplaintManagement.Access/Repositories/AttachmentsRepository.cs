using ComplaintManagement.Access.Database;
using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ComplaintManagement.Access.Repositories
{
    public class AttachmentsRepository : IAttachmentsRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public AttachmentsRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        // =========================================
        // CREATE
        // =========================================

        public async Task<int> CreateAsync(
            ComplaintAttachment attachment)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Attachments_Create",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@ComplaintId",
                SqlDbType.Int
            ).Value = attachment.ComplaintId;


            command.Parameters.Add(
                "@UploadedByUserId",
                SqlDbType.Int
            ).Value = attachment.UploadedByUserId;


            command.Parameters.Add(
                "@FileName",
                SqlDbType.NVarChar,
                255
            ).Value = attachment.FileName;


            command.Parameters.Add(
                "@FilePath",
                SqlDbType.NVarChar,
                500
            ).Value = attachment.FilePath;


            command.Parameters.Add(
                "@FileType",
                SqlDbType.NVarChar,
                100
            ).Value =
                (object?)attachment.FileType
                ?? DBNull.Value;


            command.Parameters.Add(
                "@FileSize",
                SqlDbType.BigInt
            ).Value =
                (object?)attachment.FileSize
                ?? DBNull.Value;


            await connection.OpenAsync();

            object? result =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }


        // =========================================
        // GET BY COMPLAINT
        // =========================================

        public async Task<IEnumerable<ComplaintAttachment>>
            GetByComplaintAsync(int complaintId)
        {
            List<ComplaintAttachment> attachments = new();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Attachments_GetByComplaint",
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
                attachments.Add(
                    MapAttachment(reader)
                );
            }

            return attachments;
        }


        // =========================================
        // GET BY ID
        // =========================================

        public async Task<ComplaintAttachment?>
            GetByIdAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Attachments_GetById",
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

            return MapAttachment(reader);
        }


        // =========================================
        // UPDATE
        // =========================================

        public async Task<bool> UpdateAsync(
            ComplaintAttachment attachment)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Attachments_Update",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = attachment.Id;


            command.Parameters.Add(
                "@FileName",
                SqlDbType.NVarChar,
                255
            ).Value = attachment.FileName;


            command.Parameters.Add(
                "@FilePath",
                SqlDbType.NVarChar,
                500
            ).Value = attachment.FilePath;


            command.Parameters.Add(
                "@FileType",
                SqlDbType.NVarChar,
                100
            ).Value =
                (object?)attachment.FileType
                ?? DBNull.Value;


            command.Parameters.Add(
                "@FileSize",
                SqlDbType.BigInt
            ).Value =
                (object?)attachment.FileSize
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
                    "sp_Attachments_Delete",
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
        // MAP ATTACHMENT
        // =========================================

        private static ComplaintAttachment MapAttachment(
            SqlDataReader reader)
        {
            return new ComplaintAttachment
            {
                Id = Convert.ToInt32(
                    reader["Id"]
                ),

                ComplaintId = Convert.ToInt32(
                    reader["ComplaintId"]
                ),

                UploadedByUserId =
                    Convert.ToInt32(
                        reader["UploadedByUserId"]
                    ),

                UploadedByUserName =
                    reader["UploadedByUserName"] == DBNull.Value
                        ? null
                        : reader["UploadedByUserName"].ToString(),

                UploadedByUserImageUrl =
                    reader["UploadedByUserImageUrl"] == DBNull.Value
                        ? null
                        : reader["UploadedByUserImageUrl"].ToString(),

                FileName =
                    reader["FileName"].ToString()!,

                FilePath =
                    reader["FilePath"].ToString()!,

                FileType =
                    reader["FileType"] == DBNull.Value
                        ? null
                        : reader["FileType"].ToString(),

                FileSize =
                    reader["FileSize"] == DBNull.Value
                        ? null
                        : Convert.ToInt64(
                            reader["FileSize"]
                        ),

                UploadedAt =
                    Convert.ToDateTime(
                        reader["UploadedAt"]
                    )
            };
        }
    }
}