using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using AjulBisik_Hutspeng.Models;
using AjulBisik_Hutspeng.Services;

namespace AjulBisik_Hutspeng.Repositories
{
    public class MessageRepository
    {
        private readonly DatabaseService _dbService;

        public MessageRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<List<Message>> GetInboxAsync(Guid receiverId)
        {
            var list = new List<Message>();
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT m.*, u.FullName, u.Username, u.ProfilePhotoData 
                FROM Messages m
                INNER JOIN Users u ON m.SenderId = u.Id
                WHERE m.ReceiverId = @ReceiverId
                ORDER BY m.CreatedAt DESC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ReceiverId", receiverId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var msg = new Message
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    SenderId = reader.GetGuid(reader.GetOrdinal("SenderId")),
                    ReceiverId = reader.GetGuid(reader.GetOrdinal("ReceiverId")),
                    Content = reader.GetString(reader.GetOrdinal("Content")),
                    IsAnonymous = reader.GetBoolean(reader.GetOrdinal("IsAnonymous")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
                    ReplyContent = reader.IsDBNull(reader.GetOrdinal("ReplyContent")) ? null : reader.GetString(reader.GetOrdinal("ReplyContent")),
                    Reaction = reader.IsDBNull(reader.GetOrdinal("Reaction")) ? null : reader.GetString(reader.GetOrdinal("Reaction"))
                };

                // Map sender info for UI
                msg.Sender = new User
                {
                    Id = msg.SenderId,
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    Username = reader.GetString(reader.GetOrdinal("Username")),
                    ProfilePhotoData = reader.IsDBNull(reader.GetOrdinal("ProfilePhotoData")) ? null : (byte[])reader["ProfilePhotoData"]
                };

                list.Add(msg);
            }
            return list;
        }

        public async Task<bool> CreateMessageAsync(Message message)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = @"
                INSERT INTO Messages (Id, SenderId, ReceiverId, Content, IsAnonymous, CreatedAt, IsRead, ReplyContent, Reaction)
                VALUES (@Id, @SenderId, @ReceiverId, @Content, @IsAnonymous, @CreatedAt, @IsRead, @ReplyContent, @Reaction)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", message.Id);
            cmd.Parameters.AddWithValue("@SenderId", message.SenderId);
            cmd.Parameters.AddWithValue("@ReceiverId", message.ReceiverId);
            cmd.Parameters.AddWithValue("@Content", message.Content);
            cmd.Parameters.AddWithValue("@IsAnonymous", message.IsAnonymous);
            cmd.Parameters.AddWithValue("@CreatedAt", message.CreatedAt);
            cmd.Parameters.AddWithValue("@IsRead", message.IsRead);
            cmd.Parameters.AddWithValue("@ReplyContent", (object)message.ReplyContent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Reaction", (object)message.Reaction ?? DBNull.Value);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> UpdateMessageAsync(Message message)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = @"
                UPDATE Messages SET 
                    IsRead = @IsRead,
                    ReplyContent = @ReplyContent,
                    Reaction = @Reaction
                WHERE Id = @Id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", message.Id);
            cmd.Parameters.AddWithValue("@IsRead", message.IsRead);
            cmd.Parameters.AddWithValue("@ReplyContent", (object)message.ReplyContent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Reaction", (object)message.Reaction ?? DBNull.Value);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> HasSentMessageInLast5MinutesAsync(Guid senderId)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT TOP 1 CreatedAt 
                FROM Messages 
                WHERE SenderId = @SenderId 
                ORDER BY CreatedAt DESC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@SenderId", senderId);

            var result = await cmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                var lastSent = (DateTime)result;
                if ((DateTime.UtcNow - lastSent).TotalMinutes < 5)
                {
                    return true; // Sent within last 5 minutes
                }
            }
            return false;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            using var conn = _dbService.GetConnection();
            await conn.OpenAsync();

            string query = "DELETE FROM Messages WHERE Id = @Id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}
