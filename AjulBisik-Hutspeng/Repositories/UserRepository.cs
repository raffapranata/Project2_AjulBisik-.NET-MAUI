using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using AjulBisik_Hutspeng.Models;
using AjulBisik_Hutspeng.Services;

namespace AjulBisik_Hutspeng.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseService _dbService;

        public UserRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = "SELECT * FROM Users WHERE Username = @Username";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Username", username);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapToUser(reader);
            }
            return null;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = "SELECT * FROM Users WHERE Email = @Email";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapToUser(reader);
            }
            return null;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = "SELECT * FROM Users WHERE Id = @Id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapToUser(reader);
            }
            return null;
        }

        public async Task<List<User>> SearchByUsernameAsync(string queryText, Guid currentUserId)
        {
            var list = new List<User>();
            if (string.IsNullOrWhiteSpace(queryText)) return list;

            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = "SELECT TOP 20 * FROM Users WHERE Username LIKE @Search AND Id != @CurrentUserId ORDER BY Username";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Search", $"%{queryText}%");
            cmd.Parameters.AddWithValue("@CurrentUserId", currentUserId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapToUser(reader));
            }
            return list;
        }

        public async Task<bool> CreateAsync(User user)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = @"
                INSERT INTO Users (Id, FullName, Username, Email, PasswordHash, ProfilePhotoData, CreatedAt, UpdatedAt)
                VALUES (@Id, @FullName, @Username, @Email, @PasswordHash, @ProfilePhotoData, @CreatedAt, @UpdatedAt)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", user.Id);
            cmd.Parameters.AddWithValue("@FullName", user.FullName);
            cmd.Parameters.AddWithValue("@Username", user.Username);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            
            var photoParam = new SqlParameter("@ProfilePhotoData", SqlDbType.VarBinary, -1);
            photoParam.Value = user.ProfilePhotoData ?? (object)DBNull.Value;
            cmd.Parameters.Add(photoParam);
            
            cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
            cmd.Parameters.AddWithValue("@UpdatedAt", user.UpdatedAt);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = @"
                UPDATE Users SET 
                    FullName = @FullName, 
                    Username = @Username, 
                    Email = @Email,
                    PasswordHash = @PasswordHash, 
                    ProfilePhotoData = @ProfilePhotoData,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", user.Id);
            cmd.Parameters.AddWithValue("@FullName", user.FullName);
            cmd.Parameters.AddWithValue("@Username", user.Username);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);

            var photoParam = new SqlParameter("@ProfilePhotoData", SqlDbType.VarBinary, -1);
            photoParam.Value = user.ProfilePhotoData ?? (object)DBNull.Value;
            cmd.Parameters.Add(photoParam);
            
            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            // First delete user's messages to avoid FK constraint error
            string deleteMessages = "DELETE FROM Messages WHERE SenderId = @Id OR ReceiverId = @Id";
            using var cmd1 = new SqlCommand(deleteMessages, conn);
            cmd1.Parameters.AddWithValue("@Id", id);
            await cmd1.ExecuteNonQueryAsync();

            string query = "DELETE FROM Users WHERE Id = @Id";
            using var cmd2 = new SqlCommand(query, conn);
            cmd2.Parameters.AddWithValue("@Id", id);

            int rows = await cmd2.ExecuteNonQueryAsync();
            return rows > 0;
        }

        private User MapToUser(SqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                ProfilePhotoData = reader.IsDBNull(reader.GetOrdinal("ProfilePhotoData")) ? null : (byte[])reader["ProfilePhotoData"],
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }
    }
}
