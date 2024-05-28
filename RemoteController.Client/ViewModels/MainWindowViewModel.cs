﻿using CommunityToolkit.Mvvm.ComponentModel;
using RemoteController.Client.Pages;
using System.Windows.Controls;

namespace RemoteController.Client.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private Page _currentPage;
        public Page CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public MainWindowViewModel(MainPage mainPage)
        {
            CurrentPage = mainPage;
        }
    }
}
