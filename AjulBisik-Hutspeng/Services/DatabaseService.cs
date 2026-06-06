using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace AjulBisik_Hutspeng.Services
{
    public class DatabaseService
    {
        private const string BaseConnectionString = "Server=DESKTOP-5ISDHS4\\SQLEXPRESS01;Database=ajul_bisikDB;User Id=sa;Password=dbraffa131009;TrustServerCertificate=True;MultipleActiveResultSets=True";

        public SqlConnection GetConnection()
        {
            string connString = BaseConnectionString;
            
            
            if (Microsoft.Maui.Devices.DeviceInfo.Platform == Microsoft.Maui.Devices.DevicePlatform.Android)
            {
                
                connString = connString.Replace("DESKTOP-5ISDHS4\\SQLEXPRESS01", "10.0.2.2,1433");
            }

            return new SqlConnection(connString);
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();

                string createUsersTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE name='Users' AND type='U')
                    BEGIN
                        CREATE TABLE Users (
                            Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                            FullName NVARCHAR(100) NOT NULL,
                            Username NVARCHAR(50) NOT NULL UNIQUE,
                            Email NVARCHAR(255) NOT NULL UNIQUE,
                            PasswordHash NVARCHAR(255) NOT NULL,
                            ProfilePhotoData VARBINARY(MAX) NULL,
                            CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                            UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
                        );
                        CREATE INDEX IX_Users_Username ON Users(Username);
                        CREATE INDEX IX_Users_Email ON Users(Email);
                    END
                    ELSE
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'Email' AND Object_ID = Object_ID(N'dbo.Users'))
                        BEGIN
                            ALTER TABLE Users ADD Email NVARCHAR(255) NOT NULL DEFAULT 'temp@email.com';
                            ALTER TABLE Users ADD CONSTRAINT UQ_Users_Email UNIQUE(Email);
                            CREATE INDEX IX_Users_Email ON Users(Email);
                        END
                    END";

                using var cmd1 = new SqlCommand(createUsersTable, connection);
                await cmd1.ExecuteNonQueryAsync();

                string createMessagesTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE name='Messages' AND type='U')
                    BEGIN
                        CREATE TABLE Messages (
                            Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                            SenderId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Users(Id),
                            ReceiverId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Users(Id),
                            Content NVARCHAR(MAX) NOT NULL,
                            IsAnonymous BIT NOT NULL DEFAULT 0,
                            CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                            IsRead BIT NOT NULL DEFAULT 0,
                            ReplyContent NVARCHAR(MAX) NULL,
                            Reaction NVARCHAR(50) NULL
                        );
                        CREATE INDEX IX_Messages_ReceiverId ON Messages(ReceiverId);
                        CREATE INDEX IX_Messages_SenderId ON Messages(SenderId);
                    END
                    ELSE
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IsRead' AND Object_ID = Object_ID(N'dbo.Messages'))
                        BEGIN
                            ALTER TABLE Messages ADD IsRead BIT NOT NULL DEFAULT 0;
                        END
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'ReplyContent' AND Object_ID = Object_ID(N'dbo.Messages'))
                        BEGIN
                            ALTER TABLE Messages ADD ReplyContent NVARCHAR(MAX) NULL;
                        END
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'Reaction' AND Object_ID = Object_ID(N'dbo.Messages'))
                        BEGIN
                            ALTER TABLE Messages ADD Reaction NVARCHAR(50) NULL;
                        END
                    END";

                using var cmd2 = new SqlCommand(createMessagesTable, connection);
                await cmd2.ExecuteNonQueryAsync();

                string createOtpsTable = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Otps' and xtype='U')
                    BEGIN
                        CREATE TABLE Otps (
                            Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                            Email NVARCHAR(255) NOT NULL,
                            OtpCode NVARCHAR(10) NOT NULL,
                            Type NVARCHAR(50) NOT NULL,
                            ExpiryTime DATETIME2 NOT NULL,
                            ResendCount INT NOT NULL DEFAULT 0,
                            IsUsed BIT NOT NULL DEFAULT 0,
                            CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
                        );
                        CREATE INDEX IX_Otps_Email_Type ON Otps(Email, Type);
                    END";

                using var cmd3 = new SqlCommand(createOtpsTable, connection);
                await cmd3.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DB Init Error: {ex.Message}");
            }
        }
    }
}
