﻿using Microsoft.Maui.Controls.StyleSheets;

namespace RandomStudentPicker
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
        
    }
}
