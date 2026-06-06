using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using AjulBisik_Hutspeng.Models;
using AjulBisik_Hutspeng.Services;

namespace AjulBisik_Hutspeng.Repositories
{
    public class OtpRepository
    {
        private readonly DatabaseService _dbService;

        public OtpRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<bool> CreateOrUpdateOtpAsync(OtpRecord otp)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            // Mark older OTPs of the same type for this email as used so they are invalidated
            string invalidateQuery = "UPDATE Otps SET IsUsed = 1 WHERE Email = @Email AND Type = @Type AND IsUsed = 0";
            using var cmdInvalidate = new SqlCommand(invalidateQuery, conn);
            cmdInvalidate.Parameters.AddWithValue("@Email", otp.Email);
            cmdInvalidate.Parameters.AddWithValue("@Type", otp.Type);
            await cmdInvalidate.ExecuteNonQueryAsync();

            string query = @"
                INSERT INTO Otps (Id, Email, OtpCode, Type, ExpiryTime, ResendCount, IsUsed, CreatedAt)
                VALUES (@Id, @Email, @OtpCode, @Type, @ExpiryTime, @ResendCount, @IsUsed, @CreatedAt)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", otp.Id);
            cmd.Parameters.AddWithValue("@Email", otp.Email);
            cmd.Parameters.AddWithValue("@OtpCode", otp.OtpCode);
            cmd.Parameters.AddWithValue("@Type", otp.Type);
            cmd.Parameters.AddWithValue("@ExpiryTime", otp.ExpiryTime);
            cmd.Parameters.AddWithValue("@ResendCount", otp.ResendCount);
            cmd.Parameters.AddWithValue("@IsUsed", otp.IsUsed);
            cmd.Parameters.AddWithValue("@CreatedAt", otp.CreatedAt);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<OtpRecord> GetValidOtpAsync(string email, string type)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = "SELECT TOP 1 * FROM Otps WHERE Email = @Email AND Type = @Type AND IsUsed = 0 ORDER BY CreatedAt DESC";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Type", type);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new OtpRecord
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    OtpCode = reader.GetString(reader.GetOrdinal("OtpCode")),
                    Type = reader.GetString(reader.GetOrdinal("Type")),
                    ExpiryTime = reader.GetDateTime(reader.GetOrdinal("ExpiryTime")),
                    ResendCount = reader.GetInt32(reader.GetOrdinal("ResendCount")),
                    IsUsed = reader.GetBoolean(reader.GetOrdinal("IsUsed")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                };
            }
            return null;
        }

        public async Task<bool> MarkAsUsedAsync(Guid id)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = "UPDATE Otps SET IsUsed = 1 WHERE Id = @Id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> IncrementResendCountAsync(Guid id, string newOtpCode, DateTime newExpiryTime)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = "UPDATE Otps SET ResendCount = ResendCount + 1, OtpCode = @OtpCode, ExpiryTime = @ExpiryTime WHERE Id = @Id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@OtpCode", newOtpCode);
            cmd.Parameters.AddWithValue("@ExpiryTime", newExpiryTime);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}
