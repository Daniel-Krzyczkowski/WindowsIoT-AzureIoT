using AzureIoT.UWP.Core.Services.Messaging.Contract;
using AzureIoT.UWP.Data;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.UWP.Core.ViewModel
{
    public class MessagingViewModel : AppViewModel
    {
        private readonly IMessagingService _messagingService;

        public bool IsLoading { get; set; }
        public bool IsSendMessageEnabled { get; set; } = true;

        private bool _canSendMessage;
        public bool CanSendMessage
        {
            get
            {
                return _canSendMessage;
            }
            set
            {
                _canSendMessage = value;
                RaisePropertyChanged(nameof(CanSendMessage));
            }
        }
        private string _messageContent;
        public string MessageContent
        {
            get
            {
                return _messageContent;
            }

            set
            {
                _messageContent = value;
                if(string.IsNullOrEmpty(_messageContent))
                {
                    CanSendMessage = false;
                }
                else
                {
                    CanSendMessage = true;
                }
                SendMessageAsyncCommand.RaiseCanExecuteChanged();
            }
        }

        private RelayCommand _sendMessageAsyncCommand;
        public RelayCommand SendMessageAsyncCommand
        {
            get
            {
                if (_sendMessageAsyncCommand == null)
                {
                    _sendMessageAsyncCommand = new RelayCommand(async () =>
                    {
                        IsLoading = !IsLoading;
                        RaisePropertyChanged(nameof(IsLoading));

                        IsSendMessageEnabled = !IsSendMessageEnabled;
                        RaisePropertyChanged(nameof(IsSendMessageEnabled));

                        await _messagingService.SendMessage(new MessageData { Content = MessageContent });

                        IsLoading = !IsLoading;
                        RaisePropertyChanged(nameof(IsLoading));

                        IsSendMessageEnabled = !IsSendMessageEnabled;
                        RaisePropertyChanged(nameof(IsSendMessageEnabled));

                        MessageContent = string.Empty;
                        RaisePropertyChanged(nameof(MessageContent));
                    },
                    () => CanSendMessage);
                }

                return _sendMessageAsyncCommand;
            }
        }

        public MessagingViewModel(IMessagingService messagingService)
        {
            _messagingService = messagingService;
        }
    }
}
