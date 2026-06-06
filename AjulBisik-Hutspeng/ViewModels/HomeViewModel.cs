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
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly MessageRepository _messageRepository;
        private readonly AuthService _authService;

        [ObservableProperty]
        private ObservableCollection<Message> _messages;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private bool _isEmpty;

        [ObservableProperty]
        private Message _selectedMessage;

        public HomeViewModel(MessageRepository messageRepository, AuthService authService)
        {
            _messageRepository = messageRepository;
            _authService = authService;
            Messages = new ObservableCollection<Message>();
        }

        partial void OnSelectedMessageChanged(Message value)
        {
            if (value != null)
            {
                var navParams = new Dictionary<string, object>
                {
                    { "Message", value }
                };
                Shell.Current.GoToAsync("MessageDetailPage", navParams);
                SelectedMessage = null; // Reset selection
            }
        }

        [RelayCommand]
        public async Task LoadMessagesAsync()
        {
            if (_authService.CurrentUserId == null) return;
            
            if (!IsRefreshing) IsBusy = true;
            
            try
            {
                var list = await _messageRepository.GetInboxAsync(_authService.CurrentUserId.Value);
                Messages.Clear();
                foreach (var msg in list)
                {
                    if (msg.IsAnonymous)
                    {
                        msg.Sender.FullName = "*********";
                        msg.Sender.Username = "*********";
                        msg.Sender.ProfilePhotoData = null;
                    }
                    Messages.Add(msg);
                }
                IsEmpty = Messages.Count == 0;
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task DeleteMessageAsync(Message message)
        {
            if (message == null) return;
            
            bool confirm = await Shell.Current.DisplayAlert("Sudah dibaca?", "Tandai pesan ini sudah dibaca dan hapus dari beranda?", "Ya", "Batal");
            if (!confirm) return;

            bool success = await _messageRepository.DeleteAsync(message.Id);
            if (success)
            {
                Messages.Remove(message);
                IsEmpty = Messages.Count == 0;
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Gagal menghapus pesan.", "OK");
            }
        }
    }
}
