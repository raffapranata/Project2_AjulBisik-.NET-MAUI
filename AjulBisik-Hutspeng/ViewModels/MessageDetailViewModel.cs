using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjulBisik_Hutspeng.Models;
using AjulBisik_Hutspeng.Repositories;
using AjulBisik_Hutspeng.Services;
using Microsoft.Maui.Controls;

namespace AjulBisik_Hutspeng.ViewModels
{
    [QueryProperty(nameof(Message), "Message")]
    public partial class MessageDetailViewModel : BaseViewModel
    {
        private readonly MessageRepository _messageRepository;
        private readonly ShareService _shareService;

        [ObservableProperty]
        private Message _message;

        [ObservableProperty]
        private ImageSource _senderProfilePhoto;

        [ObservableProperty]
        private string _replyInput;

        [ObservableProperty]
        private string _originalReply;

        [ObservableProperty]
        private bool _isSharing;

        public bool HasReply => !string.IsNullOrEmpty(Message?.ReplyContent);
        public bool HasReaction => !string.IsNullOrEmpty(Message?.Reaction);
        
        public string CurrentReactionText => HasReaction ? $"Reaksi: {Message.Reaction}" : string.Empty;

        public MessageDetailViewModel(MessageRepository messageRepository, ShareService shareService)
        {
            _messageRepository = messageRepository;
            _shareService = shareService;
        }

        partial void OnMessageChanged(Message value)
        {
            if (value != null)
            {
                // Mask sender info if anonymous
                if (value.IsAnonymous && value.Sender != null)
                {
                    value.Sender.FullName = "*********";
                    value.Sender.Username = "*********";
                    value.Sender.ProfilePhotoData = null;
                }

                if (value.Sender?.ProfilePhotoData != null)
                {
                    SenderProfilePhoto = ImageSource.FromStream(() => new MemoryStream(value.Sender.ProfilePhotoData));
                }
                else
                {
                    SenderProfilePhoto = "ic_account.png";
                }
                
                ReplyInput = value.ReplyContent;
                OriginalReply = value.ReplyContent; // Track original state
                
                OnPropertyChanged(nameof(HasReply));
                OnPropertyChanged(nameof(HasReaction));
                OnPropertyChanged(nameof(CurrentReactionText));
                
                // Mark as read if not already
                if (!value.IsRead)
                {
                    value.IsRead = true;
                    _ = _messageRepository.UpdateMessageAsync(value);
                }
            }
        }

        partial void OnReplyInputChanged(string value)
        {
            SaveReplyCommand.NotifyCanExecuteChanged();
        }

        partial void OnOriginalReplyChanged(string value)
        {
            SaveReplyCommand.NotifyCanExecuteChanged();
        }

        private bool CanSaveReply()
        {
            // Enable if input is different from original
            return ReplyInput != OriginalReply;
        }

        [RelayCommand]
        private async Task ReactAsync(string reaction)
        {
            if (Message == null) return;
            
            Message.Reaction = reaction;
            bool success = await _messageRepository.UpdateMessageAsync(Message);
            if (success)
            {
                OnPropertyChanged(nameof(Message));
                OnPropertyChanged(nameof(HasReaction));
                OnPropertyChanged(nameof(CurrentReactionText));
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Gagal menyimpan reaksi.", "OK");
            }
        }

        [RelayCommand(CanExecute = nameof(CanSaveReply))]
        private async Task SaveReplyAsync()
        {
            if (Message == null) return;

            Message.ReplyContent = ReplyInput;
            bool success = await _messageRepository.UpdateMessageAsync(Message);
            if (success)
            {
                OriginalReply = ReplyInput; // Update original to current state
                OnPropertyChanged(nameof(Message));
                OnPropertyChanged(nameof(HasReply));
                await Shell.Current.DisplayAlert("Sukses", "Balasan berhasil disimpan.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Gagal menyimpan balasan.", "OK");
            }
        }

        [RelayCommand]
        private async Task ShareAsync(VisualElement element)
        {
            if (element == null) return;

            IsSharing = true;

            await Task.Delay(100); // give UI time to update
            
            await _shareService.ShareElementAsImageAsync(element, "Pesan dari AjulBisik");
            
            IsSharing = false;
        }
    }
}