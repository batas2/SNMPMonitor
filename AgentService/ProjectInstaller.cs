﻿using System.ComponentModel;
using System.Configuration.Install;

namespace AgentService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}