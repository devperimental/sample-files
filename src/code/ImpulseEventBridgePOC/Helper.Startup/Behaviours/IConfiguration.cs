﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Startup.Behaviours
{
    public interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }
}
