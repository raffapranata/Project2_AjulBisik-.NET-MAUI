using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace AjulBisik_Hutspeng.Services
{
    public class EmailService
    {
        private string SmtpHost => Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
        private int SmtpPort => int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) ? port : 587;
        
        private string SmtpUser => Environment.GetEnvironmentVariable("SMTP_USER") ?? "masukkan email disini"; 
        private string SmtpPass => Environment.GetEnvironmentVariable("SMTP_PASS") ?? "masukkan kunci sandi aplikasi google nya disini (bukan kata sandi)"; 

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string reason)
        {
            var message = new MimeMessage();
            toEmail = toEmail?.Trim();
            
            try 
            {
                if (string.IsNullOrEmpty(SmtpUser) || string.IsNullOrEmpty(SmtpPass) || SmtpUser.Contains("your-email"))
                {
                    System.Diagnostics.Debug.WriteLine("[ERROR] Kredensial SMTP belum valid.");
                    return false;
                }

                message.From.Add(new MailboxAddress("AjulBisik Support", SmtpUser));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = $"[{otpCode}] Kode Verifikasi AjulBisik";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <div style='font-family: Arial; padding: 20px;'>
                            <h2 style='color: #512BD4;'>Verifikasi OTP</h2>
                            <p>Proses: <b>{reason}</b></p>
                            <h1 style='letter-spacing: 5px; color: #333;'>{otpCode}</h1>
                            <p>Kode ini berlaku selama 2 menit.</p>
                            <hr>
                            <p style='font-size: 12px; color: gray;'>Email dikirim otomatis ke {toEmail}</p>
                        </div>"
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
             
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

          
                System.Diagnostics.Debug.WriteLine($"[SMTP] Menghubungkan ke {SmtpHost}:{SmtpPort}...");
                await client.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.StartTls);
                
         
                System.Diagnostics.Debug.WriteLine($"[SMTP] Autentikasi {SmtpUser}...");
                await client.AuthenticateAsync(SmtpUser, SmtpPass);
                
        
                System.Diagnostics.Debug.WriteLine($"[SMTP] Mengirim ke {toEmail}...");
                await client.SendAsync(message);
                
                await client.DisconnectAsync(true);
                
                System.Diagnostics.Debug.WriteLine($"[SMTP] SUKSES terkirim ke {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                string errMsg = $"Gagal kirim email: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[SMTP ERROR] {errMsg}");
    
                MainThread.BeginInvokeOnMainThread(async () => {
                    await Shell.Current.DisplayAlert("SMTP Error Detail", errMsg, "OK");
                });

                return false;
            }
        }

        public string GenerateOtp()
        {
            var rnd = new Random();
            return rnd.Next(100000, 999999).ToString(); 
        }
    }
}
