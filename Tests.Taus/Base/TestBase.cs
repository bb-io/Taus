﻿using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Taus.Base;
public abstract class TestBase
{
    protected IEnumerable<AuthenticationCredentialsProvider> Credentials { get; set; }

    protected InvocationContext InvocationContext { get; set; }

    protected FileManager FileManager { get; set; }

    protected TestBase()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        Credentials = config.GetSection("ConnectionDefinition")
            .GetChildren()
            .Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value))
            .ToList();

        InvocationContext = new InvocationContext
        {
            AuthenticationCredentialsProviders = Credentials,
        };

        FileManager = new FileManager();
    }
}
