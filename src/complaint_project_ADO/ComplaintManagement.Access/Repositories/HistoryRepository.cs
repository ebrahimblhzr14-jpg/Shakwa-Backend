using ComplaintManagement.Access.Database;
using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ComplaintManagement.Access.Repositories
{
    public class ComplaintHistoryRepository
        : IComplaintHistoryRepository
    {
        private readonly SqlConnectionFactory
            _connectionFactory;

        public ComplaintHistoryRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        // =====================================================
        // Get By Complaint
        // =====================================================

        public async Task<IEnumerable<ComplaintHistory>>
            GetByComplaintIdAsync(int complaintId)
        {
            var historyList =
                new List<ComplaintHistory>();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_ComplaintHistory_GetByComplaintId",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@ComplaintId",
                SqlDbType.Int).Value =
                complaintId;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                historyList.Add(
                    MapHistory(reader));
            }

            return historyList;
        }


        // =====================================================
        // Get All Global (SuperAdmin Audit)
        // =====================================================

        public async Task<IEnumerable<ComplaintHistory>>
            GetAllGlobalAsync()
        {
            var historyList =
                new List<ComplaintHistory>();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT 
                    h.Id,
                    h.ComplaintId,
                    h.UserId,
                    u.Name AS UserName,
                    u.ImageUrl AS UserImageUrl,
                    h.Action,
                    h.OldValue,
                    h.NewValue,
                    h.Description,
                    h.CreatedAt
                FROM ComplaintHistory h
                LEFT JOIN Users u ON h.UserId = u.Id
                ORDER BY h.CreatedAt DESC;";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.CommandType = CommandType.Text;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                historyList.Add(
                    MapHistory(reader));
            }

            return historyList;
        }


        // =====================================================
        // Get By ID
        // =====================================================

        public async Task<ComplaintHistory?>
            GetByIdAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_ComplaintHistory_GetById",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@Id",
                SqlDbType.Int).Value = id;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return MapHistory(reader);
        }


        // =====================================================
        // Create
        // =====================================================

        public async Task<int>
            CreateAsync(ComplaintHistory history)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_ComplaintHistory_Create",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@ComplaintId",
                SqlDbType.Int).Value =
                history.ComplaintId;

            command.Parameters.Add(
                "@UserId",
                SqlDbType.Int).Value =
                (object?)history.UserId
                ?? DBNull.Value;

            command.Parameters.Add(
                "@Action",
                SqlDbType.NVarChar,
                100).Value =
                history.Action;

            command.Parameters.Add(
                "@OldValue",
                SqlDbType.NVarChar,
                500).Value =
                (object?)history.OldValue
                ?? DBNull.Value;

            command.Parameters.Add(
                "@NewValue",
                SqlDbType.NVarChar,
                500).Value =
                (object?)history.NewValue
                ?? DBNull.Value;

            command.Parameters.Add(
                "@Description",
                SqlDbType.NVarChar,
                1000).Value =
                (object?)history.Description
                ?? DBNull.Value;

            await connection.OpenAsync();

            return Convert.ToInt32(
                await command.ExecuteScalarAsync());
        }


        // =====================================================
        // Mapper
        // =====================================================

        private static ComplaintHistory MapHistory(
            SqlDataReader reader)
        {
            return new ComplaintHistory
            {
                Id =
                    Convert.ToInt32(
                        reader["Id"]),

                ComplaintId =
                    Convert.ToInt32(
                        reader["ComplaintId"]),

                UserId =
                    reader["UserId"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(
                            reader["UserId"]),

                UserName =
                    reader["UserName"] == DBNull.Value
                        ? null
                        : reader["UserName"].ToString(),

                UserImageUrl =
                    reader["UserImageUrl"] == DBNull.Value
                        ? null
                        : reader["UserImageUrl"].ToString(),

                Action =
                    reader["Action"]
                        .ToString() ?? string.Empty,

                OldValue =
                    reader["OldValue"] == DBNull.Value
                        ? null
                        : reader["OldValue"]
                            .ToString(),

                NewValue =
                    reader["NewValue"] == DBNull.Value
                        ? null
                        : reader["NewValue"]
                            .ToString(),

                Description =
                    reader["Description"] == DBNull.Value
                        ? null
                        : reader["Description"]
                            .ToString(),

                CreatedAt =
                    Convert.ToDateTime(
                        reader["CreatedAt"])
            };
        }
    }
}