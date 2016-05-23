﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Tests.Unit.TestFactories;

namespace WB.Tests.Unit
{
    internal class Create
    {
        public static readonly CommandFactory Command = new CommandFactory(); // cleaned by TLK (remove this comment after KP-7207 is done_
        public static readonly EventFactory Event = new EventFactory();
        public static readonly ViewModelFactory ViewModel = new ViewModelFactory();
        public static readonly ControllerFactory Controller = new ControllerFactory();
        public static readonly AnswerFactory Answer = new AnswerFactory();
        public static readonly ServiceFactory Service = new ServiceFactory(); // cleaned by TLK (remove this comment after KP-7207 is done_

        public static readonly OtherFactory Other = new OtherFactory();
    }
}
