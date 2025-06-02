using Core.Domain;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.GraphQLTypes
{
    public class TreatmentTypesType : ObjectGraphType<TreatmentType>
    {
        public TreatmentTypesType()
        {
            Field(x => x.Id);
            Field(x => x.Code);
            Field(x => x.Desc);
            Field(x => x.Required);
        }
    }
}
