using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.ViewModels
{
    public class AdminViewModel : INotifyPropertyChanged
    {
        private readonly IAnnouncementService _announcementService;
        private readonly Services.IUserService _userService;

        public ObservableCollection<Game> Games { get; } = new();
        public ObservableCollection<Announcement> Announcements { get; } = new();

        public AdminViewModel(IGameService gameService, IAnnouncementService announcementService, Services.IUserService userService)
        {
            _announcementService = announcementService;
            _userService = userService;

            // Games ve Announcements servislerden yüklenebilir; şimdilik boş
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
