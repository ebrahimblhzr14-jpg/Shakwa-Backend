using ComplaintManagement.Access.Database;
using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ComplaintManagement.Access.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public DashboardRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        // ================================================
        // Summary
        // ================================================

        public async Task<DashboardSummary>
            GetSummaryAsync()
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Dashboard_Summary",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return new DashboardSummary();
            }

            return new DashboardSummary
            {
                TotalComplaints =
                    Convert.ToInt32(
                        reader["TotalComplaints"]),

                PendingComplaints =
                    Convert.ToInt32(
                        reader["PendingComplaints"]),

                InProgressComplaints =
                    Convert.ToInt32(
                        reader["InProgressComplaints"]),

                ResolvedComplaints =
                    Convert.ToInt32(
                        reader["ResolvedComplaints"]),

                ClosedComplaints =
                    Convert.ToInt32(
                        reader["ClosedComplaints"])
            };
        }


        // ================================================
        // Complaints By Status
        // ================================================

        public async Task<IEnumerable<ComplaintsByStatus>>
            GetComplaintsByStatusAsync()
        {
            var result =
                new List<ComplaintsByStatus>();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Dashboard_ComplaintsByStatus",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(
                    new ComplaintsByStatus
                    {
                        Id = Convert.ToInt32(
                            reader["StatusId"]),

                        Name = reader["StatusName"]
                            .ToString() ?? string.Empty,

                        ComplaintCount =
                            Convert.ToInt32(
                                reader["ComplaintCount"])
                    });
            }

            return result;
        }


        // ================================================
        // Complaints By Priority
        // ================================================

        public async Task<IEnumerable<ComplaintsByPriority>>
            GetComplaintsByPriorityAsync()
        {
            var result =
                new List<ComplaintsByPriority>();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Dashboard_ComplaintsByPriority",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(
                    new ComplaintsByPriority
                    {
                        Id = Convert.ToInt32(
                            reader["Id"]),

                        Name = reader["Name"]
                            .ToString() ?? string.Empty,

                        ComplaintCount =
                            Convert.ToInt32(
                                reader["ComplaintCount"])
                    });
            }

            return result;
        }


        // ================================================
        // Complaints By Category
        // ================================================

        public async Task<IEnumerable<ComplaintsByCategory>>
            GetComplaintsByCategoryAsync()
        {
            var result =
                new List<ComplaintsByCategory>();

            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(
                    "sp_Dashboard_ComplaintsByCategory",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            await connection.OpenAsync();

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(
                    new ComplaintsByCategory
                    {
                        Id = Convert.ToInt32(
                            reader["Id"]),

                        Name = reader["Name"]
                            .ToString() ?? string.Empty,

                        ComplaintCount =
                            Convert.ToInt32(
                                reader["ComplaintCount"])
                    });
            }

            return result;
        }
    }
}