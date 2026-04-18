using System.Collections.Generic;
using Dapper;
using MySqlConnector;

public class RoleRepository
{
    private readonly string connectionString;

    public RoleRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public List<RoleData> LoadRoles()
    {
        const string query = "SELECT id AS Id, role_name AS Name, description_role AS Description FROM role ORDER BY id ASC;";

        List<RoleData> roles = new List<RoleData>();

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            List<RoleRow> rows = new List<RoleRow>(conn.Query<RoleRow>(query));

            for (int i = 0; i < rows.Count; i++)
            {
                RoleRow row = rows[i];
                roles.Add(new RoleData(row.Id, row.Name ?? string.Empty, row.Description ?? string.Empty));
            }
        }

        return roles;
    }

    private class RoleRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
