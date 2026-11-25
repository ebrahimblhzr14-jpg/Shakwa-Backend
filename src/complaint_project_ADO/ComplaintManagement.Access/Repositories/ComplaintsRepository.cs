using ComplaintManagement.Access.Database;
using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ComplaintManagement.Access.Repositories
{
    public class ComplaintsRepository : IComplaintsRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public ComplaintsRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        // =========================================
        // CREATE
        // =========================================

        public async Task<int> CreateAsync(
            Complaint complaint)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Complaints_Create",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@UserId",
                SqlDbType.Int
            ).Value = complaint.UserId;


            command.Parameters.Add(
                "@CategoryId",
                SqlDbType.Int
            ).Value = complaint.CategoryId;


            command.Parameters.Add(
                "@DepartmentId",
                SqlDbType.Int
            ).Value =
                (object?)complaint.DepartmentId
                ?? DBNull.Value;


            command.Parameters.Add(
                "@StatusId",
                SqlDbType.Int
            ).Value = complaint.StatusId;


            command.Parameters.Add(
                "@PriorityId",
                SqlDbType.Int
            ).Value = complaint.PriorityId;


            command.Parameters.Add(
                "@Title",
                SqlDbType.NVarChar,
                200
            ).Value = complaint.Title;


            command.Parameters.Add(
                "@Description",
                SqlDbType.NVarChar
            ).Value = complaint.Description;


            await connection.OpenAsync();

            object? result =
                await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }


        // =========================================
        // GET ALL
        // =========================================

        public async Task<IEnumerable<Complaint>>
            GetAllAsync()
        {
            List<Complaint> complaints = new();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Complaints_GetAll",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                complaints.Add(
                    MapComplaint(reader)
                );
            }

            return complaints;
        }


        // =========================================
        // GET BY ID
        // =========================================

        public async Task<Complaint?>
            GetByIdAsync(int id)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Complaints_GetById",
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

            return MapComplaint(reader);
        }


        // =========================================
        // GET BY USER
        // =========================================

        public async Task<IEnumerable<Complaint>>
            GetByUserAsync(int userId)
        {
            List<Complaint> complaints = new();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Complaints_GetByUser",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@UserId",
                SqlDbType.Int
            ).Value = userId;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                complaints.Add(
                    new Complaint
                    {
                        Id = Convert.ToInt32(
                            reader["Id"]
                        ),

                        Title = reader["Title"]
                            .ToString()!,

                        Description = reader["Description"]
                            .ToString()!,

                        CategoryName =
                            reader["CategoryName"] == DBNull.Value
                                ? null
                                : reader["CategoryName"].ToString(),

                        DepartmentName =
                            reader["DepartmentName"] == DBNull.Value
                                ? null
                                : reader["DepartmentName"].ToString(),

                        StatusName =
                            reader["StatusName"] == DBNull.Value
                                ? null
                                : reader["StatusName"].ToString(),

                        PriorityName =
                            reader["PriorityName"] == DBNull.Value
                                ? null
                                : reader["PriorityName"].ToString(),

                        CreatedAt =
                            Convert.ToDateTime(
                                reader["CreatedAt"]
                            ),

                        UpdatedAt =
                            reader["UpdatedAt"] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(
                                    reader["UpdatedAt"]
                                ),

                        ResolvedAt =
                            reader["ResolvedAt"] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(
                                    reader["ResolvedAt"]
                                ),

                        ClosedAt =
                            reader["ClosedAt"] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(
                                    reader["ClosedAt"]
                                )
                    });
            }

            return complaints;
        }


        // =========================================
        // GET ASSIGNED TO ADMIN (DUPLICATE-FREE)
        // =========================================

        public async Task<IEnumerable<Complaint>>
            GetAssignedToAdminAsync(int adminId)
        {
            List<Complaint> complaints = new();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT 
                    c.Id,
                    c.Title,
                    c.Description,
                    c.UserId,
                    u.Name AS UserName,
                    u.ImageUrl AS UserImageUrl,
                    c.CategoryId,
                    cat.Name AS CategoryName,
                    c.DepartmentId,
                    dept.Name AS DepartmentName,
                    c.StatusId,
                    s.Name AS StatusName,
                    c.PriorityId,
                    p.Name AS PriorityName,
                    c.CreatedAt,
                    c.UpdatedAt,
                    c.ResolvedAt,
                    c.ClosedAt
                FROM Complaints c
                INNER JOIN Users u ON c.UserId = u.Id
                LEFT JOIN ComplaintCategories cat ON c.CategoryId = cat.Id
                LEFT JOIN Departments dept ON c.DepartmentId = dept.Id
                LEFT JOIN ComplaintStatuses s ON c.StatusId = s.Id
                LEFT JOIN Priorities p ON c.PriorityId = p.Id
                WHERE EXISTS (
                    SELECT 1 
                    FROM ComplaintAssignments a 
                    WHERE a.ComplaintId = c.Id 
                      AND a.AssignedToUserId = @AdminId 
                      AND a.UnassignedAt IS NULL
                )
                ORDER BY c.CreatedAt DESC;";

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.CommandType = CommandType.Text;

            command.Parameters.Add(
                "@AdminId",
                SqlDbType.Int
            ).Value = adminId;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                complaints.Add(
                    MapComplaint(reader)
                );
            }

            return complaints;
        }


        // =========================================
        // UPDATE
        // =========================================

        public async Task<bool> UpdateAsync(
            Complaint complaint)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Complaints_Update",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = complaint.Id;


            command.Parameters.Add(
                "@CategoryId",
                SqlDbType.Int
            ).Value = complaint.CategoryId;


            command.Parameters.Add(
                "@DepartmentId",
                SqlDbType.Int
            ).Value =
                (object?)complaint.DepartmentId
                ?? DBNull.Value;


            command.Parameters.Add(
                "@PriorityId",
                SqlDbType.Int
            ).Value = complaint.PriorityId;


            command.Parameters.Add(
                "@Title",
                SqlDbType.NVarChar,
                200
            ).Value = complaint.Title;


            command.Parameters.Add(
                "@Description",
                SqlDbType.NVarChar
            ).Value = complaint.Description;


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
                    "sp_Complaints_Delete",
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
        // UPDATE STATUS
        // =========================================

        public async Task<bool> UpdateStatusAsync(
            int complaintId,
            int statusId,
            int userId)
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Complaints_UpdateStatus",
                    connection
                );

            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                "@ComplaintId",
                SqlDbType.Int
            ).Value = complaintId;


            command.Parameters.Add(
                "@StatusId",
                SqlDbType.Int
            ).Value = statusId;


            command.Parameters.Add(
                "@UserId",
                SqlDbType.Int
            ).Value = userId;


            await connection.OpenAsync();

            int rowsAffected =
                await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }


        // =========================================
        // MAP COMPLAINT
        // =========================================

        private static Complaint MapComplaint(
            SqlDataReader reader)
        {
            return new Complaint
            {
                Id = Convert.ToInt32(
                    reader["Id"]
                ),

                Title = reader["Title"]
                    .ToString()!,

                Description = reader["Description"]
                    .ToString()!,

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

                CategoryId = Convert.ToInt32(
                    reader["CategoryId"]
                ),

                CategoryName =
                    reader["CategoryName"] == DBNull.Value
                        ? null
                        : reader["CategoryName"].ToString(),

                DepartmentId =
                    reader["DepartmentId"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(
                            reader["DepartmentId"]
                        ),

                DepartmentName =
                    reader["DepartmentName"] == DBNull.Value
                        ? null
                        : reader["DepartmentName"].ToString(),

                StatusId = Convert.ToInt32(
                    reader["StatusId"]
                ),

                StatusName =
                    reader["StatusName"] == DBNull.Value
                        ? null
                        : reader["StatusName"].ToString(),

                PriorityId = Convert.ToInt32(
                    reader["PriorityId"]
                ),

                PriorityName =
                    reader["PriorityName"] == DBNull.Value
                        ? null
                        : reader["PriorityName"].ToString(),

                CreatedAt =
                    Convert.ToDateTime(
                        reader["CreatedAt"]
                    ),

                UpdatedAt =
                    reader["UpdatedAt"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(
                            reader["UpdatedAt"]
                        ),

                ResolvedAt =
                    reader["ResolvedAt"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(
                            reader["ResolvedAt"]
                        ),

                ClosedAt =
                    reader["ClosedAt"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(
                            reader["ClosedAt"]
                        )
            };
        }



    }
}