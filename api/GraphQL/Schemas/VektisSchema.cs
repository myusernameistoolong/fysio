using api.GraphQLTypes;
using Core.Domain;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Schemas
{
    public class VektisSchema : Schema
    {
        public VektisSchema(IServiceProvider provider)
        : base(provider)
        {
            Query = provider.GetRequiredService<VektisQuery>();
        }
    }
}
