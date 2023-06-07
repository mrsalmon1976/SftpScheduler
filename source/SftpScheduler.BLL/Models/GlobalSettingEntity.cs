﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Models
{
    public record GlobalSettingEntity
    {
        public GlobalSettingEntity()
        {

        }

        public GlobalSettingEntity(string id, string settingValue)
        {
            this.Id = id;
            this.SettingValue = settingValue;
        }


        public string Id { get; set; } = String.Empty;

        public string SettingValue { get; set; } = String.Empty;
    }
}
