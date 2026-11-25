using ComplaintManagement.Access.Database;
using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ComplaintManagement.Access.Repositories
{
    public class AssignmentsRepository : IAssignmentsRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public AssignmentsRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        // =========================================
        // CREATE
        // =========================================

        public async Task<int> CreateAsync(
            ComplaintAssignment assignment)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Assignments_Create",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@ComplaintId",
                SqlDbType.Int
            ).Value = assignment.ComplaintId;


            command.Parameters.Add(
                "@AssignedToUserId",
                SqlDbType.Int
            ).Value = assignment.AssignedToUserId;


            command.Parameters.Add(
                "@AssignedByUserId",
                SqlDbType.Int
            ).Value = assignment.AssignedByUserId;


            command.Parameters.Add(
                "@Notes",
                SqlDbType.NVarChar,
                500
            ).Value =
                (object?)assignment.Notes
                ?? DBNull.Value;


            await connection.OpenAsync();

            object? result =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }


        // =========================================
        // GET ALL
        // =========================================

        public async Task<IEnumerable<ComplaintAssignment>>
            GetAllAsync()
        {
            List<ComplaintAssignment> assignments = new();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Assignments_GetAll",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                assignments.Add(
                    MapAssignment(reader)
                );
            }

            return assignments;
        }


        // =========================================
        // GET BY ID
        // =========================================

        public async Task<ComplaintAssignment?>
            GetByIdAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Assignments_GetById",
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

            return MapAssignment(reader);
        }


        // =========================================
        // UPDATE
        // =========================================

        public async Task<bool> UpdateAsync(
            ComplaintAssignment assignment)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Assignments_Update",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = assignment.Id;


            command.Parameters.Add(
                "@AssignedToUserId",
                SqlDbType.Int
            ).Value = assignment.AssignedToUserId;


            command.Parameters.Add(
                "@UnassignedAt",
                SqlDbType.DateTime2
            ).Value =
                (object?)assignment.UnassignedAt
                ?? DBNull.Value;


            command.Parameters.Add(
                "@Notes",
                SqlDbType.NVarChar,
                500
            ).Value =
                (object?)assignment.Notes
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
                    "sp_Assignments_Delete",
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
        // HAS ACTIVE ASSIGNMENT
        // =========================================

        public async Task<bool> HasActiveAssignmentAsync(int complaintId, int adminId)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM ComplaintAssignments
                    WHERE ComplaintId = @ComplaintId
                      AND AssignedToUserId = @AdminId
                      AND UnassignedAt IS NULL
                ) THEN 1 ELSE 0 END;";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.CommandType = CommandType.Text;

            command.Parameters.Add(
                "@ComplaintId",
                SqlDbType.Int
            ).Value = complaintId;

            command.Parameters.Add(
                "@AdminId",
                SqlDbType.Int
            ).Value = adminId;

            await connection.OpenAsync();

            object? result =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(result) == 1;
        }


        // =========================================
        // MAP ASSIGNMENT
        // =========================================

        private static ComplaintAssignment MapAssignment(
            SqlDataReader reader)
        {
            return new ComplaintAssignment
            {
                Id = Convert.ToInt32(
                    reader["Id"]
                ),

                ComplaintId = Convert.ToInt32(
                    reader["ComplaintId"]
                ),

                AssignedToUserId =
                    Convert.ToInt32(
                        reader["AssignedToUserId"]
                    ),

                AssignedToUserName =
                    reader["AssignedToUserName"] == DBNull.Value
                        ? null
                        : reader["AssignedToUserName"].ToString(),

                AssignedToUserImageUrl =
                    reader["AssignedToUserImageUrl"] == DBNull.Value
                        ? null
                        : reader["AssignedToUserImageUrl"].ToString(),

                AssignedByUserId =
                    Convert.ToInt32(
                        reader["AssignedByUserId"]
                    ),

                AssignedByUserName =
                    reader["AssignedByUserName"] == DBNull.Value
                        ? null
                        : reader["AssignedByUserName"].ToString(),

                AssignedByUserImageUrl =
                    reader["AssignedByUserImageUrl"] == DBNull.Value
                        ? null
                        : reader["AssignedByUserImageUrl"].ToString(),

                AssignedAt =
                    Convert.ToDateTime(
                        reader["AssignedAt"]
                    ),

                UnassignedAt =
                    reader["UnassignedAt"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(
                            reader["UnassignedAt"]
                        ),

                Notes =
                    reader["Notes"] == DBNull.Value
                        ? null
                        : reader["Notes"].ToString()
            };
        }
    }
}