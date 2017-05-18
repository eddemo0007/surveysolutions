﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class UserApiDetails 
    {
        public UserApiDetails(UserView userView)
        {
            if (userView == null)
                return;

            this.UserId = userView.PublicKey;
            this.UserName = userView.UserName;
            this.Email = userView.Email;
            this.CreationDate = userView.CreationDate;
            this.IsLocked = userView.IsLockedBySupervisor || userView.IsLockedByHQ;
            this.Roles = userView.Roles.ToList();
        }

        [DataMember]
        [Required]
        public IList<UserRoles> Roles { get; private set; }

        [DataMember]
        [Required]
        public bool IsLocked { get; private set; }

        [DataMember]
        [Required]
        public DateTime CreationDate { get; private set; }

        [DataMember]
        public string Email { get; private set; }

        [DataMember]
        [Required]
        public Guid UserId { get; private set; }

        [DataMember]
        [Required]
        public string UserName { get; private set; }
    }
}