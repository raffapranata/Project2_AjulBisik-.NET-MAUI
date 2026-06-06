using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjulBisik_Hutspeng.Models;
using AjulBisik_Hutspeng.Repositories;
using AjulBisik_Hutspeng.Services;
using Microsoft.Maui.Controls;

namespace AjulBisik_Hutspeng.ViewModels
{
    public partial class SendMessageViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository;
        private readonly MessageRepository _messageRepository;
        private readonly AuthService _authService;

        [ObservableProperty]
        private string _searchQuery;

        [ObservableProperty]
        private ObservableCollection<User> _searchResults;

        [ObservableProperty]
        private User _selectedUser;

        [ObservableProperty]
        private string _messageContent;

        [ObservableProperty]
        private bool _isAnonymous;

        public SendMessageViewModel(UserRepository userRepository, MessageRepository messageRepository, AuthService authService)
        {
            _userRepository = userRepository;
            _messageRepository = messageRepository;
            _authService = authService;
            SearchResults = new ObservableCollection<User>();
        }

        [RelayCommand]
        private async Task SearchUsersAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchResults.Clear();
                return;
            }

            var currentUserId = _authService.CurrentUserId ?? Guid.Empty;
            var list = await _userRepository.SearchByUsernameAsync(SearchQuery, currentUserId);
            SearchResults.Clear();
            foreach (var u in list)
            {
                SearchResults.Add(u);
            }
        }

        [RelayCommand]
        private void SelectUser(User user)
        {
            SelectedUser = user;
            SearchQuery = user.Username;
            SearchResults.Clear();
        }

        [RelayCommand]
        private void ClearSelectedUser()
        {
            SelectedUser = null;
            SearchQuery = string.Empty;
        }

        [RelayCommand]
        private async Task SendMessageAsync()
        {
            if (SelectedUser == null)
            {
                await Shell.Current.DisplayAlert("Error", "Pilih penerima pesan terlebih dahulu.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(MessageContent))
            {
                await Shell.Current.DisplayAlert("Error", "Pesan tidak boleh kosong.", "OK");
                return;
            }

            var senderId = _authService.CurrentUserId;
            if (senderId == null) return;

            IsBusy = true;
            try
            {
                bool isSpamming = await _messageRepository.HasSentMessageInLast5MinutesAsync(senderId.Value);
                if (isSpamming)
                {
                    await Shell.Current.DisplayAlert("Warning", "Anda hanya dapat mengirim pesan setiap 5 menit. Harap tunggu.", "OK");
                    return;
                }

                var msg = new Message
                {
                    Id = Guid.NewGuid(),
                    SenderId = senderId.Value,
                    ReceiverId = SelectedUser.Id,
                    Content = MessageContent,
                    IsAnonymous = IsAnonymous,
                    CreatedAt = DateTime.UtcNow
                };

                bool success = await _messageRepository.CreateMessageAsync(msg);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Sukses", "✔ berhasil terkirim", "OK");
                    ClearSelectedUser();
                    MessageContent = string.Empty;
                    IsAnonymous = false;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Gagal mengirim pesan.", "OK");
                }
            }
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Terjadi kesalahan: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
