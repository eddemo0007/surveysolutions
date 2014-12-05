﻿using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class ResetPasswordAccountCommand : AccountCommandBase
    {
        public ResetPasswordAccountCommand(Guid accountId, string password, string passwordSalt)
            : base(accountId)
        {
            this.Password = password;
            this.PasswordSalt = passwordSalt;
        }

        public string Password { get; private set; }
        public string PasswordSalt { get; private set; }
    }
}